using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * A graph that represents a tilemap, using only the allowed tiles.
 */
public class TilemapGraph : IGraph<Vector3Int>
{
    private Tilemap tilemap;
    private AllowedTiles allowedTiles;
    private PlayerInventory inventory;

    public TilemapGraph(Tilemap tilemap, AllowedTiles allowedTiles, PlayerInventory inventory)
    {
        this.tilemap = tilemap;
        this.allowedTiles = allowedTiles;
        this.inventory = inventory;
    }

    public bool IsWalkable(Vector3Int pos)
    {
        TileBase tile = tilemap.GetTile(pos);
        if (tile == null)
            return false;

        if (!allowedTiles.Contains(tile))
            return false;

        if (allowedTiles.IsWater(tile))
        {
            if (inventory == null || !inventory.hasBoat)
                return false;
        }
        if (allowedTiles.IsMountain(tile))
        {
            if (inventory == null || (!inventory.hasGoat && !inventory.hasPickaxe))
                return false;
        }

        return true;
    }

    static Vector3Int[] directions = {
            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 1, 0),
    };

    public IEnumerable<Vector3Int> Neighbors(Vector3Int node)
    {
        foreach (var direction in directions)
        {
            Vector3Int neighborPos = node + direction;
            if (IsWalkable(neighborPos))
                yield return neighborPos;
        }
    }
}
