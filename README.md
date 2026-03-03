![alt text](image.png)
# 🎮 Pokemon DustGrey - 100 Hour Alpha Edition

A grid-based RPG engine inspired by the classic Pokémon series, built with **Godot 4.3+ and C# (.NET 8)**. This project represents a **100-hour development sprint**, moving from a blank engine to a functional "Vertical Slice" of the Kanto region.

## ✨ Major Milestone: The 100-Hour Ship
This version marks the completion of the initial engine and the implementation of the first story arc.
- **Time Logged:** 100 Hours exactly.
- **Status:** Alpha v1.0 - Shipped.

## 🚀 Features & New Content

- **Modular Battle System:** A C# driven combat engine featuring level-scaling and move logic.
- **Reactive Story NPCs:** Advanced NPC logic using an Event-State Machine for path-blocking and forced interceptions (e.g., Prof. Oak and Giovanni).
- **Custom Pixel Art:** Includes hand-drawn sprites for Gym Leaders like Brock.
- **Save/Load Persistence:** Integrated system to track badges and player progress.

## 🗺️ World Map Progress

- **Pallet Town:** Home base featuring Delia (Healer) and Prof. Oak (Starter/Blocker).
- **Viridian City:** Featuring the high-stakes early encounter with Giovanni.
- **Pewter City:** Fully mapped Gym with a functional Brock boss fight.
- **Lavender Town:** Foundations and map layout implemented (Access restricted in Alpha).
- **Route Connectivity:** 4 fully playable routes with active NPC interaction.

## 🛠️ Updated Roadmap

### **Phase 1: Core Engine 🟢 (COMPLETE)**
- [x] Custom C# Logger
- [x] Grid-Based Movement & Snapping
- [x] Signal-Based Animation System

### **Phase 2: World & Interaction 🟢 (COMPLETE)**
- [x] Dialogue System & Save Manager
- [x] Story-State NPC Logic (Path Blocking)
- [x] Gym Leader Boss AI

### **Phase 3: Expansion 🟡 (NEXT STEPS)**
- [ ] Full Lavender Town scripts.
- [ ] Inventory and Item usage.
- [ ] Wild Encounter tables.

## 📂 Project Structure

```
scripts/
├── core/           # Singletons (Globals, Logger)
├── gameplay/       # Movement, Input, Animation logic
├── utilities/      # State Machines, Math helpers
├── ui/             # Tailwind-integrated UI components
assets/
├── sprites/        # Character and NPC sheets
└── tilesets/       # Environment textures and collisions
```

## Demo gameplay

**Controls**
- Movement: WASD keys or arrow keys
- Save game -Press  x to savhe the game
- Doing battles and quing messages - space(stuff like npcs, signs,more to come).

Steps to Play
 - Go to the first floor of the house and talk to mom.
 - navigate to0 the pokemon lab and meet proffeser oak to get your first pokemon.
- then you ccanj go exploring. Nore depth and lore will come in later updates. This release is meant to establish the base structure of the running game. The expansions will include an option to play the game in 3d and IOk will also bhe adding all of the regions one by one. Hope you like it !!
[Demo.mp4]


## Acknowledgements

- [Engine](https://godotengine.org/)
- Inspiration: Pokémon X/Y (Nintendo/GameFreak)
- Tutorial Foundations: Inspired by The Nerdy Canuck's Pokémon Clone series.
- Player and npc - https://www.deviantart.com/aveontrainer
- Tileset = https://www.deviantart.com/flurmimon/gallery/75279275/pokemon-rhin-concept
- Other assets - https://www.spriters-resource.com
- Brok sprite - JoshR691
