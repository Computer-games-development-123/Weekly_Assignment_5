using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

/**
 * This component just keeps a list of allowed tiles.
 * Such a list is used both for pathfinding and for movement.
 */
public class AllowedTiles : MonoBehaviour
{
    [Header("General walkable tiles")]
    [SerializeField] TileBase[] allowedTiles = null;

    [Header("Special tiles")]
    [SerializeField] TileBase[] waterTiles = null;
    [SerializeField] TileBase[] mountainTiles = null;

    public bool Contains(TileBase tile)
    {
        return allowedTiles != null && allowedTiles.Contains(tile);
    }

    public bool IsWater(TileBase tile)
    {
        return waterTiles != null && waterTiles.Contains(tile);
    }

    public bool IsMountain(TileBase tile)
    {
        return mountainTiles != null && mountainTiles.Contains(tile);
    }

    public TileBase[] Get()
    {
        return allowedTiles;
    }
}
