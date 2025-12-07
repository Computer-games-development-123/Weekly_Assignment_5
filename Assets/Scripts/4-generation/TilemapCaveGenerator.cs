using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/**
 * This class demonstrates the CaveGenerator on a Tilemap.
 * 
 * By: Erel Segal-Halevi
 * Since: 2020-12
 */

public class TilemapCaveGenerator : MonoBehaviour
{
    [SerializeField] Tilemap tilemap = null;

    [Tooltip("The tile that represents a wall (an impassable block / mountain)")]
    [SerializeField] TileBase wallTile = null;

    [Tooltip("The tile that represents a floor (a passable block)")]
    [SerializeField] TileBase floorTile = null;

    [Header("Water tiles")]
    [Tooltip("Shallow water (easiest to walk on)")]
    [SerializeField] TileBase shallowWaterTile = null;

    [Tooltip("Medium depth water")]
    [SerializeField] TileBase mediumWaterTile = null;

    [Tooltip("Deep water")]
    [SerializeField] TileBase deepWaterTile = null;

    [Tooltip("Initial percent of floor tiles that start as water seeds (0-1)")]
    [Range(0, 1)]
    [SerializeField] float waterSeedPercent = 0.15f;

    [Tooltip("How many smoothing steps to run on the water map to create clusters")]
    [SerializeField] int waterSimulationSteps = 3;

    [Tooltip("The percent of walls in the initial random map")]
    [Range(0, 1)]
    [SerializeField] float randomFillPercent = 0.5f;

    [Tooltip("Length and height of the grid")]
    [SerializeField] int gridSize = 100;

    [Tooltip("How many steps do we want to simulate for the cave shape?")]
    [SerializeField] int simulationSteps = 20;

    [Tooltip("For how long will we pause between each simulation step so we can look at the result?")]
    [SerializeField] float pauseTime = 1f;

    [Header("Player placement")]
    [Tooltip("Player transform to place on a reachable tile")]
    [SerializeField] Transform player = null;

    [Tooltip("Minimum number of tiles the player must be able to reach from the spawn point")]
    [SerializeField] int minReachableTiles = 100;

    [Header("Pathfinding setup")]
    [Tooltip("Allowed tiles component used by TilemapGraph (should include floor + water + mountains)")]
    [SerializeField] AllowedTiles allowedTiles = null;

    [Header("Item prefabs")]
    [SerializeField] GameObject boatPrefab = null;
    [SerializeField] GameObject goatPrefab = null;
    [SerializeField] GameObject pickaxePrefab = null;

    [Tooltip("Maximum attempts to find positions for items / player")]
    [SerializeField] int maxPlacementTries = 1000;

    private CaveGenerator caveGenerator;
    private PlayerInventory inventory;
    private TilemapGraph tilemapGraph;
    private int[,] waterMap = null;

    void Start()
    {
        // To get the same random numbers each time we run the script
        Random.InitState(100);

        caveGenerator = new CaveGenerator(randomFillPercent, gridSize);
        caveGenerator.RandomizeMap();

        ShowPatternOnTileMap(caveGenerator.GetMap(), null);

        // start simulation (async)
        SimulateCavePattern();
    }

    async void SimulateCavePattern()
    {
        for (int i = 0; i < simulationSteps; i++)
        {
            await Awaitable.WaitForSecondsAsync(pauseTime);

            // Calculate the new values for cave shape
            caveGenerator.SmoothMap();

            // Show cave walls/floors during simulation (no water yet)
            ShowPatternOnTileMap(caveGenerator.GetMap(), null);
        }

        Debug.Log("Cave simulation completed!");

        int[,] caveData = caveGenerator.GetMap();
        waterMap = GenerateWaterClusters(caveData);

        ShowPatternOnTileMap(caveData, waterMap);

        if (player != null)
        {
            inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                Debug.LogWarning("Player has no PlayerInventory component. Movement rules will ignore items.");
            }

            if (allowedTiles == null)
            {
                Debug.LogWarning("AllowedTiles is not assigned on TilemapCaveGenerator. TilemapGraph will be null.");
            }
            else
            {
                tilemapGraph = new TilemapGraph(tilemap, allowedTiles, inventory);
            }

            if (tilemapGraph != null)
            {
                PlacePlayerInReachableLocation();

                SpawnItems(caveData);
            }
        }
        else
        {
            Debug.LogWarning("Player transform is not assigned on TilemapCaveGenerator.");
        }
    }

    private void ShowPatternOnTileMap(int[,] caveData, int[,] waterData)
    {
        int width = caveData.GetLength(0);
        int height = caveData.GetLength(1);

        tilemap.ClearAllTiles();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var position = new Vector3Int(x, y, 0);

                if (caveData[x, y] == 1)
                {
                    // Wall / mountain
                    tilemap.SetTile(position, wallTile);
                }
                else
                {
                    // Floor or water
                    if (waterData != null && waterData[x, y] == 1)
                    {
                        TileBase waterTileToPlace = shallowWaterTile;

                        float r = Random.value;
                        if (r < 0.33f && shallowWaterTile != null)
                        {
                            waterTileToPlace = shallowWaterTile;
                        }
                        else if (r < 0.66f && mediumWaterTile != null)
                        {
                            waterTileToPlace = mediumWaterTile;
                        }
                        else if (deepWaterTile != null)
                        {
                            waterTileToPlace = deepWaterTile;
                        }

                        tilemap.SetTile(position, waterTileToPlace);
                    }
                    else
                    {
                        tilemap.SetTile(position, floorTile);
                    }
                }
            }
        }
    }
    private int[,] GenerateWaterClusters(int[,] caveData)
    {
        int width = caveData.GetLength(0);
        int height = caveData.GetLength(1);

        int[,] water = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (caveData[x, y] == 0) // 0 = floor
                {
                    if (Random.value < waterSeedPercent)
                        water[x, y] = 1;
                }
            }
        }

        for (int step = 0; step < waterSimulationSteps; step++)
        {
            int[,] newWater = new int[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (caveData[x, y] == 1)
                    {
                        newWater[x, y] = 0;
                        continue;
                    }

                    int neighbors = CountWaterNeighbors(water, x, y);

                    if (neighbors >= 4)
                        newWater[x, y] = 1;
                    else if (neighbors <= 1)
                        newWater[x, y] = 0;
                    else
                        newWater[x, y] = water[x, y];
                }
            }

            water = newWater;
        }

        return water;
    }

    private int CountWaterNeighbors(int[,] water, int x, int y)
    {
        int count = 0;
        int width = water.GetLength(0);
        int height = water.GetLength(1);

        // 8-שכנים כדי לקבל צורה "עגולה" יותר
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                if (water[nx, ny] == 1)
                    count++;
            }
        }

        return count;
    }


    private int CountReachableTiles(Vector3Int start, int maxNeeded)
    {
        if (tilemapGraph == null)
            return 0;

        if (!tilemapGraph.IsWalkable(start))
            return 0;

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0 && visited.Count < maxNeeded)
        {
            var current = queue.Dequeue();

            foreach (var neighbor in tilemapGraph.Neighbors(current))
            {
                if (visited.Contains(neighbor))
                    continue;

                visited.Add(neighbor);
                queue.Enqueue(neighbor);

                if (visited.Count >= maxNeeded)
                    break;
            }
        }

        return visited.Count;
    }

    private void PlacePlayerInReachableLocation()
    {
        int width = gridSize;
        int height = gridSize;

        for (int attempt = 0; attempt < maxPlacementTries; attempt++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Vector3Int candidateCell = new Vector3Int(x, y, 0);

            int reachable = CountReachableTiles(candidateCell, minReachableTiles);

            if (reachable >= minReachableTiles)
            {
                Vector3 worldPos = tilemap.GetCellCenterWorld(candidateCell);
                player.position = worldPos;

                Debug.Log($"Placed player at {candidateCell} with {reachable} reachable tiles.");
                return;
            }
        }

        Debug.LogWarning($"Could not find a spawn point with {minReachableTiles} reachable tiles after {maxPlacementTries} attempts.");
    }
    private void SpawnItems(int[,] caveData)
    {
        int width = caveData.GetLength(0);
        int height = caveData.GetLength(1);

        if (boatPrefab != null)
        {
            Vector3Int? cell = FindFloorCellWithNeighborType(width, height,
                t => t == floorTile,
                t => t == shallowWaterTile || t == mediumWaterTile || t == deepWaterTile);

            if (cell.HasValue)
            {
                Vector3 worldPos = tilemap.GetCellCenterWorld(cell.Value);
                Instantiate(boatPrefab, worldPos, Quaternion.identity);
                Debug.LogWarning("The boat is at position: " + worldPos);
            }
        }

        if (goatPrefab != null)
        {
            Vector3Int? cell = FindFloorCellWithNeighborType(width, height,
                t => t == floorTile,
                t => t == wallTile);

            if (cell.HasValue)
            {
                Vector3 worldPos = tilemap.GetCellCenterWorld(cell.Value);
                Instantiate(goatPrefab, worldPos, Quaternion.identity);
                Debug.LogWarning("The goat is at position: " + worldPos);
            }
        }

        if (pickaxePrefab != null)
        {
            Vector3Int? cell = FindRandomFloorCell(width, height);
            if (cell.HasValue)
            {
                Vector3 worldPos = tilemap.GetCellCenterWorld(cell.Value);
                Instantiate(pickaxePrefab, worldPos, Quaternion.identity);
                Debug.LogWarning("The pickaxe is at position: " + worldPos);
            }
        }
    }

    private Vector3Int? FindRandomFloorCell(int width, int height)
    {
        for (int attempt = 0; attempt < maxPlacementTries; attempt++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Vector3Int cell = new Vector3Int(x, y, 0);

            TileBase tile = tilemap.GetTile(cell);
            if (tile == floorTile)
                return cell;
        }
        return null;
    }

    private Vector3Int? FindFloorCellWithNeighborType(
        int width,
        int height,
        System.Predicate<TileBase> floorPredicate,
        System.Predicate<TileBase> neighborPredicate)
    {
        for (int attempt = 0; attempt < maxPlacementTries; attempt++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Vector3Int cell = new Vector3Int(x, y, 0);

            TileBase tile = tilemap.GetTile(cell);
            if (tile == null || !floorPredicate(tile))
                continue;

            if (HasNeighborOfType(cell, neighborPredicate))
                return cell;
        }

        return null;
    }

    private bool HasNeighborOfType(Vector3Int cell, System.Predicate<TileBase> predicate)
    {
        Vector3Int[] dirs = new Vector3Int[]
        {
            new Vector3Int(1,0,0),
            new Vector3Int(-1,0,0),
            new Vector3Int(0,1,0),
            new Vector3Int(0,-1,0),
        };

        foreach (var d in dirs)
        {
            Vector3Int n = cell + d;
            TileBase t = tilemap.GetTile(n);
            if (t != null && predicate(t))
                return true;
        }
        return false;
    }
}
