using Game.Core;
using Game.Gameplay;
using Godot;
using Godot.Collections;

public partial class PlayerSaveResource : Resource
{
    // Story & World State
    [Export] public PlayerStoryState StoryProgress;
    [Export] public Vector2 GlobalPosition;
    [Export] public string MapName;

    // Pokemon Specific Data
    [Export] public StarterChoice ChosenStarter;
    [Export] public Vector2 FacingDirection;

    // An array to store the names or IDs of every Pokemon the player has

    [Export] public Array<string> CaughtPokemonList = new Array<string>();

    // Detailed Party Data (for levels, HP, etc. later)
    [Export] public Dictionary PartyDetails = new Dictionary();

    [Export] public LevelName CurrentLevel;
    [Export]
    public Godot.Collections.Dictionary lastPosition = new Godot.Collections.Dictionary
    {
        { "posX", 0 },
        { "posY", 0 }
    };

    [Export] public Array<Badge> Badges = new Array<Badge>();

    [Export] public Array<PlayerStoryState> CompletedStoryProgress = new Array<PlayerStoryState>();
    [Export] public Array<Game.Core.Pokeball> Pokeballs = new();
    
    // Track defeated trainers to prevent re-battling
    [Export] public Array<string> DefeatedTrainers = new Array<string>();

    // Money and Inventory System
    [Export] public int Money = 3000; // Starting money
    
    // Inventory: Key = ItemId (or ItemName), Value = Quantity
    [Export] public Dictionary Inventory = new Dictionary();
    
    // Track visited shops (optional - for special dialogues)
    [Export] public Array<string> VisitedShops = new Array<string>();
    
    // Helper method to add item to inventory
    public void AddItem(int itemId, int quantity = 1)
    {
        string key = itemId.ToString();
        if (Inventory.ContainsKey(key))
        {
            int current = Inventory[key].AsInt32();
            Inventory[key] = current + quantity;
        }
        else
        {
            Inventory[key] = quantity;
        }
    }
    
    // Helper method to remove item from inventory
    public bool RemoveItem(int itemId, int quantity = 1)
    {
        string key = itemId.ToString();
        if (!Inventory.ContainsKey(key)) return false;
        
        int current = Inventory[key].AsInt32();
        if (current < quantity) return false;
        
        int newQuantity = current - quantity;
        if (newQuantity <= 0)
        {
            Inventory.Remove(key);
        }
        else
        {
            Inventory[key] = newQuantity;
        }
        return true;
    }
    
    // Helper method to check if player has item
    public bool HasItem(int itemId, int quantity = 1)
    {
        string key = itemId.ToString();
        if (!Inventory.ContainsKey(key)) return false;
        return Inventory[key].AsInt32() >= quantity;
    }
    
    // Helper method to get item quantity
    public int GetItemQuantity(int itemId)
    {
        string key = itemId.ToString();
        if (!Inventory.ContainsKey(key)) return 0;
        return Inventory[key].AsInt32();
    }

}
