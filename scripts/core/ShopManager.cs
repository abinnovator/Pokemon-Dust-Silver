using Godot;
using Godot.Collections;
using System.Threading.Tasks;
using Game.Gameplay;

namespace Game.Core
{
	public partial class ShopManager : CanvasLayer
	{
		public static ShopManager Instance { get; private set; }

		[ExportCategory("UI Components")]
		[Export] public Control ShopContainer;
		[Export] public RichTextLabel GreetingLabel;
		[Export] public ItemList ItemListControl;
		[Export] public RichTextLabel ItemDescriptionLabel;
		[Export] public RichTextLabel PlayerMoneyLabel;
		[Export] public Label QuantityLabel;
		[Export] public Panel ConfirmPanel;
		[Export] public RichTextLabel ConfirmLabel;

		private Array<ShopItem> _currentShopItems;
		private int _selectedItemIndex = 0;
		private int _purchaseQuantity = 1;
		private bool _isShopOpen = false;
		private bool _isConfirmingPurchase = false;
		private int _confirmSelection = 0; // 0 = Yes, 1 = No

		public override void _Ready()
		{
			Instance = this;
			if (ShopContainer != null)
			{
				ShopContainer.Visible = false;
			}
			if (ConfirmPanel != null)
			{
				ConfirmPanel.Visible = false;
			}
		}

		public override void _Process(double delta)
		{
			if (!_isShopOpen) return;

			if (_isConfirmingPurchase)
			{
				HandleConfirmInput();
			}
			else
			{
				HandleShopInput();
			}
		}

		private void HandleShopInput()
		{
			if (Input.IsActionJustPressed("ui_cancel") || Input.IsActionJustPressed("back"))
			{
				CloseShop();
				return;
			}

			if (Input.IsActionJustPressed("ui_down"))
			{
				_selectedItemIndex = (_selectedItemIndex + 1) % _currentShopItems.Count;
				UpdateItemSelection();
			}
			else if (Input.IsActionJustPressed("ui_up"))
			{
				_selectedItemIndex--;
				if (_selectedItemIndex < 0) _selectedItemIndex = _currentShopItems.Count - 1;
				UpdateItemSelection();
			}
			else if (Input.IsActionJustPressed("ui_right"))
			{
				_purchaseQuantity++;
				if (_purchaseQuantity > 99) _purchaseQuantity = 99;
				UpdateQuantityDisplay();
			}
			else if (Input.IsActionJustPressed("ui_left"))
			{
				_purchaseQuantity--;
				if (_purchaseQuantity < 1) _purchaseQuantity = 1;
				UpdateQuantityDisplay();
			}
			else if (Input.IsActionJustPressed("use") || Input.IsActionJustPressed("ui_accept"))
			{
				ShowConfirmDialog();
			}
		}

		private void HandleConfirmInput()
		{
			if (Input.IsActionJustPressed("ui_left") || Input.IsActionJustPressed("ui_right"))
			{
				_confirmSelection = 1 - _confirmSelection;
				UpdateConfirmDisplay();
			}
			else if (Input.IsActionJustPressed("use") || Input.IsActionJustPressed("ui_accept"))
			{
				if (_confirmSelection == 0) // Yes
				{
					PurchaseItem();
				}
				HideConfirmDialog();
			}
			else if (Input.IsActionJustPressed("ui_cancel") || Input.IsActionJustPressed("back"))
			{
				HideConfirmDialog();
			}
		}

		public async Task OpenShop(Array<ShopItem> items, string greeting = "Welcome to my shop!")
		{
			_currentShopItems = items;
			_selectedItemIndex = 0;
			_purchaseQuantity = 1;
			_isShopOpen = true;

			if (ShopContainer != null)
			{
				ShopContainer.Visible = true;
			}

			if (GreetingLabel != null)
			{
				GreetingLabel.Text = greeting;
			}

			PopulateItemList();
			UpdateItemSelection();
			UpdatePlayerMoney();

			Signals.EmitGlobalSignal(Signals.SignalName.MessageBoxOpen, true);
			GameManager.IsPlayerMovementLocked = true;

			// Wait until shop is closed
			while (_isShopOpen)
			{
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
		}

		private void PopulateItemList()
		{
			if (ItemListControl == null) return;

			ItemListControl.Clear();
			foreach (var item in _currentShopItems)
			{
				string itemText = $"{item.ItemName} - ${item.Price}";
				ItemListControl.AddItem(itemText);
			}
		}

		private void UpdateItemSelection()
		{
			if (ItemListControl == null) return;

			ItemListControl.Select(_selectedItemIndex);

			if (_currentShopItems.Count > 0 && _selectedItemIndex < _currentShopItems.Count)
			{
				var selectedItem = _currentShopItems[_selectedItemIndex];
				if (ItemDescriptionLabel != null)
				{
					ItemDescriptionLabel.Text = selectedItem.Description;
				}
			}

			UpdateQuantityDisplay();
		}

		private void UpdateQuantityDisplay()
		{
			if (QuantityLabel != null)
			{
				QuantityLabel.Text = $"Qty: {_purchaseQuantity}";

				if (_currentShopItems.Count > 0 && _selectedItemIndex < _currentShopItems.Count)
				{
					var selectedItem = _currentShopItems[_selectedItemIndex];
					int totalCost = selectedItem.Price * _purchaseQuantity;
					QuantityLabel.Text += $" (Total: ${totalCost})";
				}
			}
		}

		private void UpdatePlayerMoney()
		{
			if (PlayerMoneyLabel != null && SaveManager.Instance?.CurrentSave != null)
			{
				PlayerMoneyLabel.Text = $"Money: ${SaveManager.Instance.CurrentSave.Money}";
			}
		}

		private void ShowConfirmDialog()
		{
			if (_currentShopItems.Count == 0 || _selectedItemIndex >= _currentShopItems.Count) return;

			var selectedItem = _currentShopItems[_selectedItemIndex];
			int totalCost = selectedItem.Price * _purchaseQuantity;

			if (ConfirmPanel != null)
			{
				ConfirmPanel.Visible = true;
			}

			if (ConfirmLabel != null)
			{
				ConfirmLabel.Text = $"Buy {_purchaseQuantity}x {selectedItem.ItemName} for ${totalCost}?";
			}

			_confirmSelection = 0;
			_isConfirmingPurchase = true;
			UpdateConfirmDisplay();
		}

		private void HideConfirmDialog()
		{
			if (ConfirmPanel != null)
			{
				ConfirmPanel.Visible = false;
			}
			_isConfirmingPurchase = false;
		}

		private void UpdateConfirmDisplay()
		{
			if (ConfirmLabel == null) return;

			var selectedItem = _currentShopItems[_selectedItemIndex];
			int totalCost = selectedItem.Price * _purchaseQuantity;

			string yesText = _confirmSelection == 0 ? "[color=yellow]> Yes[/color]" : "  Yes";
			string noText = _confirmSelection == 1 ? "[color=yellow]> No[/color]" : "  No";

			ConfirmLabel.Text = $"Buy {_purchaseQuantity}x {selectedItem.ItemName} for ${totalCost}?\n\n{yesText}  {noText}";
		}

		private async void PurchaseItem()
		{
			if (SaveManager.Instance?.CurrentSave == null) return;
			if (_currentShopItems.Count == 0 || _selectedItemIndex >= _currentShopItems.Count) return;

			var selectedItem = _currentShopItems[_selectedItemIndex];
			int totalCost = selectedItem.Price * _purchaseQuantity;
			var save = SaveManager.Instance.CurrentSave;

			// Check if player has enough money
			if (save.Money < totalCost)
			{
				await ShowMessage("You don't have enough money!");
				return;
			}

			// Deduct money
			save.Money -= totalCost;

			// Add items to inventory
			string itemKey = selectedItem.ItemId.ToString();
			if (save.Inventory.ContainsKey(itemKey))
			{
				int currentQuantity = save.Inventory[itemKey].AsInt32();
				save.Inventory[itemKey] = currentQuantity + _purchaseQuantity;
			}
			else
			{
				save.Inventory[itemKey] = _purchaseQuantity;
			}

			// Save the game
			SaveManager.Instance.SaveToDisk();

			// Update UI
			UpdatePlayerMoney();

			await ShowMessage($"Purchased {_purchaseQuantity}x {selectedItem.ItemName}!");

			// Reset quantity
			_purchaseQuantity = 1;
			UpdateQuantityDisplay();
		}

		private async Task ShowMessage(string message)
		{
			// Temporarily hide shop UI
			if (ShopContainer != null)
			{
				ShopContainer.Visible = false;
			}

			await MessageManager.PlayText(message);

			// Show shop UI again
			if (ShopContainer != null)
			{
				ShopContainer.Visible = true;
			}
		}

		private void CloseShop()
		{
			_isShopOpen = false;
			if (ShopContainer != null)
			{
				ShopContainer.Visible = false;
			}

			Signals.EmitGlobalSignal(Signals.SignalName.MessageBoxOpen, false);
			GameManager.IsPlayerMovementLocked = false;
		}

		public static bool IsShopOpen() => Instance?._isShopOpen ?? false;
	}
}
