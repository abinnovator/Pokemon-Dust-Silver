# Contributing to PERDICAN V1

Thanks for your interest in contributing! This is a software project so contributions can include optimizations, addition of art and styling, adding more music, improving mechanics.

---

## Table of Contents

- [What Contributions Are Welcome](#what-contributions-are-welcome)
- [Project Structure (Where to Put Files)](#project-structure-where-to-put-files)
- [How to Contribute](#how-to-contribute)
- [Submitting a Pull Request](#submitting-a-pull-request)
- [Reporting Issues](#reporting-issues)
- [Code of Conduct](#code-of-conduct)

---

## What Contributions Are Welcome

### MAP
- Detail Improvements
- Adding more towns, completing/adding missing interiors.

### Code
- Making code more opitimized.
- Adding new features the community would like to see.

### Documentation
- Fixing typos, improving clarity in README.

<br>
- and everything that you would like to be added to the project.

---

## Project Structure (Where to Put Files)

Please follow these conventions to keep the repo clean:

addons/
├── MoveImporter/           # The tool to import a specific amount of pokemon moves
├── pokemon_importer/       # Imports all the pokemon and their sprites
├── item_importer/          #  Pokeball Importer!
├── item_importer/          # Normal Items Importer
scripts/
├── core/           # Singletons (Globals, Logger)
├── gameplay/       # Movement, Input, Animation logic
├── utilities/      # State Machines, Math helpers
├── ui/             # Tailwind-integrated UI components
assets/
├── sprites/        # Character and NPC sheets
└── tilesets/       # Environment textures and collisions
└── audio/          # All the music for the game!
└── fonts/          # Text fonts!
└── items/          # item images!
└── fonts/          # pokemon sprites!
└── ui/             # Images for all the ui!
scenes/ #All the scenes
├── charecters/     # Character and NPC 
└── core/           # The core scenes like the game manager
└── levels/         # All the actual places you can visit
└── referance/      # Tracks how much of the map is done!
└── ui/             # All the ui scenes



Try to avoid committing random images/files to the root folder.

---

## How to Contribute

### 1. Fork the repository
Use the **Fork** button on GitHub.

### 2. Clone your fork
```bash
git clone https://github.com/<your-username>/Pokemon-Dust-Silver.git
cd Pokemon-Dust-Silver
```

### 3. Create a new branch
Use a descriptive name:
```bash
git checkout -b feat/add-lavender-town
```

### 4. Make your changes
- For Scenes: First check out the existing scenes. Duplicate one of them and start making changes or delete the tileset in the scene and retile.
- For documentation: update `README.md` & for images/videos use hack club cdn or google drive to share..

### 5. Commit your changes
```bash
git add .
git commit -m "feat: Add bike mechanics"
```

### 6. Push and open a Pull Request
```bash
git push origin feat/add-bike-machanics
```
Then open a PR targeting the default branch.

---

## Submitting a Pull Request

To keep PRs easy to review:
- Keep PRs focused on a single topic.
- Include screenshots when modifying maps.
- Add a changelog.md file showing all the files changed in your version with a brief description of what was changed.

---

## Reporting Issues

If you find a problem, open an issue and include:
- What you expected vs what happened
- Steps to reproduce (if applicable)
- Photos/screenshots/screen recordings if relevant

---

## Code of Conduct

Only commit if you are sure you have something that would make the project in some way. I have no restrictions on who can commit. Aslong as youre having fun and doing something purposefull go right ahead:D
