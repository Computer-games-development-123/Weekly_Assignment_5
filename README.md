# README – Tilemap Pathfinding & Cave Generation Assignment

**ITCH.IO:** https://itzhak173.itch.io/cave-explorer

## 🎯 Overview
This assignment consists of **two main parts**:

1. **Abstract algorithm design (Console Project)**  
2. **Unity integration (Tilemap + Player + Items + Environment)**  

All algorithms were implemented first as pure C# logic and only afterwards integrated into Unity, following the assignment requirements.

---

## 🔹 What Was Added in This Assignment

In addition to the base code, we implemented the following features as required in the homework:

1. **Boat, Goat, and Pickaxe items**
   - Boat: allows the player to walk over all water tiles (shallow, medium, deep).
   - Goat: allows the player to walk over mountain tiles.
   - Pickaxe: allows the player to walk over mountain tiles and dig them into floor tiles.

2. **Random cave map with valid player spawn**
   - The game generates a random cave map.
   - The player is placed at a random tile on the map.
   - We run BFS from that position and check if the player can reach **at least 100 different tiles**.
   - If the reachable area is **less than 100 tiles**, the game chooses another random position and checks again.
   - This process repeats until the player is placed on a tile that satisfies the 100-tiles reachability condition.

---

# 🟦 Part 1 – Abstract Algorithm Implementation (Console Project)

The logic was implemented in a separate **Console App**, fully independent of Unity.

## ✔ Abstract Map Representation
Includes:
- `CellType` (Floor, Water, Mountain)
- `Inventory` (Boat, Goat, Pickaxe)
- `GridMap` (walkability rules, neighbor detection, digging mechanics)

## ✔ Movement Rules
- Floor → always walkable  
- Water → walkable only with **boat**  
- Mountain → walkable with **goat OR pickaxe**  
- Pickaxe can **dig mountains into floor(Grass) tiles**

## ✔ BFS (Reachability & Pathfinding)
A pure BFS implementation:
- Determines reachable tiles
- Used to validate spawn positions (must reach at least 100 tiles)
- Used to test walkability rules independently of Unity

## ✔ Cave Generation Logic
Using a cellular automata-based generator to produce 0/1 cave maps.

## ✔ Player Spawn Validation
Algorithm:
1. Select random floor tile  
2. Run BFS  
3. If reachable < 100 → retry  
4. Accept only valid spawn points  

This step ensures maps are always playable.

---

# 🟩 Part 2 – Unity Integration

After verifying abstract logic, it was integrated into Unity.

## ✔ TilemapGraph
A graph wrapper for Tilemap:
- Determines neighbors based on walkability  
- Supports water/mountain groups  
- Uses Inventory rules identical to Part 1  

## ✔ PlayerInventory
Player can collect:
- Boat → walk on all water tiles  
- Goat → walk on mountains  
- Pickaxe → walk & dig mountains  

## ✔ PickaxeDig
Press **E** to dig mountains into floor tiles.

## ✔ TilemapCaveGenerator
Expanded generator:
- Produces clustered water tiles  
- Generates mountains and floors  
- Places:
  - Boat near water  
  - Goat near mountains  
  - Pickaxe on a floor tile  
- Ensures valid BFS spawn region  
- Produces complete, playable worlds  

## ✔ TargetMover (BFS Movement)
- Moves toward target using BFS  
- Updates paths dynamically when inventory changes  
- Compatible with digging & changing tilemap  

---

# 🟧 Extra Features
- Full separation of algorithm vs Unity code  
- Dynamic pathfinding that adapts to player inventory  
- Procedural map generation with guaranteed reachability  
- Realistic placement of items (boat/goat/pickaxe)

---

# 🟪 Unit Tests
A full test suite exists in the Console Project.
To run the test:
- First make sure you are in the folder "TestCaveGenerator10"
- Then run with:

```
dotnet run
```

---

# ✅ Conclusion
This assignment provides:
- Abstract BFS & navigation logic  
- Full Unity gameplay integration  
- Procedural cave generation  
- Item-based traversal mechanics  
- Guaranteed playable maps  

All parts follow the structure and requirements of the official assignment.
