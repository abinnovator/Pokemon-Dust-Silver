# Shop System Documentation

## Overview
The Shop System allows NPCs to function as shop clerks, selling items to the player. This system integrates with the existing StoryNPC framework and PlayerSaveResource.

## Components

### 1. ShopManager (`scripts/core/ShopManager.cs`)
Singleton manager that handles the shop UI and purchase logic.
- Displays shop items with prices
- Manages quantity selection
- Handles purchase transactions
- Updates player inventory and money

### 2. ShopItem (`scripts/gameplay/items/ShopItem.cs`)
Resource class representing an item that can be sold in shops.

**Properties:**
- `ItemName` (string): Display name of the item
- `ItemId` (int): Unique identifier for the item
- `Description` (string): Item description shown in shop UI
- `Price` (int): Cost in Pokedollars
- `Category` (ItemCategory): Item category (Pokeball, Medicine, etc.)
- `Stock` (int): Available quantity (-1 = infinite)
- `IsKeyItem` (bool): Whether this is a key item
- `Icon` (Texture2D): Optional icon for the item

### 3. StoryNpcInputConfig Extensions
Added shop clerk functionality to the existing NPC config system.

**New Properties:**
- `IsShopClerk` (bool): Marks this NPC as a shop clerk
- `ShopItems` (Array<ShopItem>): Items this clerk sells
- `ShopGreeting` (string): Custom greeting when shop opens

### 4. PlayerSaveResource Extensions
Enhanced inventory management with helper methods.

**New Properties:**
- `VisitedShops` (Array<string>): Optional tracking of visited shops

**New Methods:**
- `AddItem(int itemId, int quantity)`: Add items to inventory
- `RemoveItem(int itemId, int quantity)`: Remove items from inventory
- `HasItem(int itemId, int quantity)`: Check if player has item
- `GetItemQuantity(int itemId)`: Get quantity of specific item

## Usage

### Creating a Shop Clerk

1. **Create Shop Items** (or use existing ones)
   - Navigate to `resources/items/shop_items/`
   - Use existing items like `potion.tres`, `pokeball.tres`, etc.
   - Or create new ShopItem resources

2. **Create NPC Config**
   ```
   - In Godot, create a new StoryNpcInputConfig resource
   - Set IsShopClerk = true
   - Add ShopItems by dragging item resources
   - Set ShopGreeting to customize the greeting message
   - Set NpcMovementType to Static (usually for shopkeepers)
   ```

3. **Add NPC to Scene**
   ```
   - Add a StoryNpc node to your level
   - Set the NpcAppearance (choose appropriate sprite)
   - Set InputConfig to your shop clerk config resource
   - Position the NPC in your scene
   ```

### Example Shop Clerk Config
See `resources/items/shop_items/example_pokemart_clerk_config.tres` for a complete example.

## Shop UI Controls

**Navigation:**
- `Up/Down Arrow`: Select different items
- `Left/Right Arrow`: Adjust purchase quantity
- `Z / Enter / Use`: Confirm purchase
- `X / Escape / Back`: Close shop

**Features:**
- Real-time money display
- Quantity selector with total cost preview
- Item descriptions
- Confirmation dialog before purchase
- Automatic inventory updates
- Automatic save after purchase

## Integration with Story System

Shop clerks work seamlessly with the existing story system:

```cs
EventTrigger = PlayerStoryState.GOT_FIRST_BADGE

StoryNotMetMessage = "Sorry, we only sell to licensed trainers!"
```

## Code Flow

1. Player interacts with NPC
2. `StoryNpc.PlayMessage()` checks if `IsShopClerk == true`
3. Greeting message is displayed
4. `ShopManager.OpenShop()` is called with items and greeting
5. Shop UI opens with available items
6. Player navigates and selects items
7. On purchase confirmation:
   - Check if player has enough money
   - Deduct cost from player money
   - Add items to inventory
   - Save game
   - Show confirmation message
8. Shop remains open until player exits

## Future Enhancements

Potential additions to the shop system:
- Sell items back to shops (buyback system)
- Item stock management (limited quantities)
- Special shop events (sales, discounts)
- Shop reputation/loyalty system
- Item availability based on story progress
- Different shop types (specialty shops)

## Troubleshooting

**Shop doesn't open:**
- Verify ShopManager is in the scene (`scenes/ui/ShopUI.tscn`)
- Check that ShopManager is instantiated in `game_manager.tscn`
- Ensure IsShopClerk = true in NPC config
- Confirm ShopItems array is not empty

**Items not purchasing:**
- Check SaveManager.Instance is not null
- Verify player has enough money
- Ensure ItemId is set correctly
- Check console for error messages

**UI not displaying correctly:**
- Verify all node paths in ShopUI.tscn
- Check that UI layer is set correctly (layer 10)
- Ensure ShopContainer is initially hidden

## Testing

To test the shop system:
1. Load a save with money (default: $3000)
2. Interact with a shop clerk NPC
3. Navigate the shop UI
4. Purchase items
5. Check inventory to verify items were added
6. Reload the game to verify save persistence
