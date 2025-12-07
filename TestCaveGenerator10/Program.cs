using System;
using System.Collections.Generic;


public enum CellType
{
    Floor,
    Water,
    Mountain
}

public struct Inventory
{
    public bool HasBoat;
    public bool HasGoat;
    public bool HasPickaxe;
}

public class GridMap
{
    private readonly CellType[,] cells;
    private readonly int width;
    private readonly int height;
    private readonly Inventory inventory;

    public GridMap(CellType[,] cells, Inventory inventory)
    {
        this.cells = cells;
        this.inventory = inventory;
        width = cells.GetLength(0);
        height = cells.GetLength(1);
    }

    public CellType GetCell(int x, int y) => cells[x, y];

    // Simulates pickaxe digging: Mountain => Floor(Grass)
    public void Dig(int x, int y)
    {
        if (inventory.HasPickaxe && cells[x, y] == CellType.Mountain)
            cells[x, y] = CellType.Floor;
    }

    // Returns whether a cell is walkable based on inventory rules
    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return false;

        var cell = cells[x, y];

        if (cell == CellType.Floor)
            return true;

        if (cell == CellType.Water)
            return inventory.HasBoat;

        if (cell == CellType.Mountain)
            return inventory.HasGoat || inventory.HasPickaxe;

        return false;
    }

    // Returns only neighbors that are walkable
    public IEnumerable<(int x, int y)> Neighbors(int x, int y)
    {
        int[,] dirs = { { 1,0 }, { -1,0 }, { 0,1 }, { 0,-1 } };

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dirs[i,0];
            int ny = y + dirs[i,1];

            if (IsWalkable(nx, ny))
                yield return (nx, ny);
        }
    }
}

//
// =============================================================
//  BFS reachability (counts how many tiles are reachable)
// =============================================================
//

public static class GridBfs
{
    public static int CountReachable(GridMap map, int startX, int startY, int maxNeeded)
    {
        if (!map.IsWalkable(startX, startY))
            return 0;

        var q = new Queue<(int,int)>();
        var visited = new HashSet<(int,int)>();

        q.Enqueue((startX, startY));
        visited.Add((startX, startY));

        while (q.Count > 0 && visited.Count < maxNeeded)
        {
            var (x, y) = q.Dequeue();

            foreach (var n in map.Neighbors(x, y))
            {
                if (visited.Add(n))
                {
                    q.Enqueue(n);
                    if (visited.Count >= maxNeeded)
                        break;
                }
            }
        }

        return visited.Count;
    }
}

//
// =============================================================
//  Unit Tests (simple assert-based tests)
// =============================================================
//

public static class Tests
{
    public static void RunAll()
    {
        Test_WaterBlockedWithoutBoat();          // Test 1
        Test_WaterAllowedWithBoat();             // Test 2
        Test_MountainBlockedWithoutEquipment();  // Test 3
        Test_MountainAllowedWithGoat();          // Test 4
        Test_MountainAllowedWithPickaxe();       // Test 5
        Test_DigMountain();                      // Test 6
        Test_Reachability_LowArea();             // Test 7
        Test_Reachability_HighArea();            // Test 8

        Console.WriteLine("All tests passed!");
    }

    static void Assert(bool cond, string message)
    {
        if (!cond)
            throw new Exception("Test failed: " + message);
    }

    // =========================================================
    // 1. Water should block movement if player has no boat
    // =========================================================
    static void Test_WaterBlockedWithoutBoat()
    {
        var cells = new[,] { { CellType.Floor, CellType.Water, CellType.Floor } };

        var inv = new Inventory { HasBoat = false };
        var map = new GridMap(cells, inv);

        int r = GridBfs.CountReachable(map, 0, 0, 10);
        Assert(r == 1, "Player must NOT be able to cross water without boat");
    }

    // =========================================================
    // 2. Water should be walkable if player owns a boat
    // =========================================================
    static void Test_WaterAllowedWithBoat()
    {
        var cells = new[,] { { CellType.Floor, CellType.Water, CellType.Floor } };

        var inv = new Inventory { HasBoat = true };
        var map = new GridMap(cells, inv);

        int r = GridBfs.CountReachable(map, 0, 0, 10);
        Assert(r == 3, "Player must be able to cross water WITH boat");
    }

    // =========================================================
    // 3. Mountain should block if player has no goat/pickaxe
    // =========================================================
    static void Test_MountainBlockedWithoutEquipment()
    {
        var cells = new[,] { { CellType.Floor, CellType.Mountain, CellType.Floor } };

        var inv = new Inventory();
        var map = new GridMap(cells, inv);

        int r = GridBfs.CountReachable(map, 0, 0, 10);
        Assert(r == 1, "Mountain must block if player has no goat/pickaxe");
    }

    // =========================================================
    // 4. Goat allows walking on mountains
    // =========================================================
    static void Test_MountainAllowedWithGoat()
    {
        var cells = new[,] { { CellType.Floor, CellType.Mountain, CellType.Floor } };

        var inv = new Inventory { HasGoat = true };
        var map = new GridMap(cells, inv);

        int r = GridBfs.CountReachable(map, 0, 0, 10);
        Assert(r == 3, "Goat must allow crossing mountains");
    }

    // =========================================================
    // 5. Pickaxe ALSO allows walking on mountains
    // =========================================================
    static void Test_MountainAllowedWithPickaxe()
    {
        var cells = new[,] { { CellType.Floor, CellType.Mountain, CellType.Floor } };

        var inv = new Inventory { HasPickaxe = true };
        var map = new GridMap(cells, inv);

        int r = GridBfs.CountReachable(map, 0, 0, 10);
        Assert(r == 3, "Pickaxe must allow crossing mountains");
    }

    // =========================================================
    // 6. Pickaxe digging converts Mountain => Floor(Grass)
    // =========================================================
    static void Test_DigMountain()
    {
        var cells = new[,] { { CellType.Floor, CellType.Mountain } };

        var inv = new Inventory { HasPickaxe = true };
        var map = new GridMap(cells, inv);

        map.Dig(0, 1);   // Dig the mountain tile

        Assert(map.GetCell(0, 1) == CellType.Floor,
            "Digging must convert mountain tile into floor(Grass)");
    }

    // =========================================================
    // 7. Small blocked area should NOT reach 100 tiles
    // =========================================================
    static void Test_Reachability_LowArea()
    {
        var cells = new CellType[10,10];

        // All floor(Grass) except a blocking mountain in the center
        for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                cells[x, y] = CellType.Floor;

        cells[5, 5] = CellType.Mountain;

        var inv = new Inventory(); // no tools
        var map = new GridMap(cells, inv);

        int reachable = GridBfs.CountReachable(map, 5, 4, 100);
        Assert(reachable < 100, "Small area must NOT reach 100 tiles");
    }

    // =========================================================
    // 8. Large open area MUST reach >= 100 tiles
    // =========================================================
    static void Test_Reachability_HighArea()
    {
        var cells = new CellType[30,30];

        for (int x = 0; x < 30; x++)
            for (int y = 0; y < 30; y++)
                cells[x, y] = CellType.Floor;

        var inv = new Inventory();
        var map = new GridMap(cells, inv);

        int reachable = GridBfs.CountReachable(map, 10, 10, 100);
        Assert(reachable >= 100, "Large open area must allow reaching at least 100 tiles");
    }
}

class Program
{
    static void Main()
    {
        Tests.RunAll();
    }
}
