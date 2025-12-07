using UnityEngine;
using UnityEngine.Tilemaps;

public class PickaxeDig : MonoBehaviour
{
    [Header("Tilemap + tiles")]
    [SerializeField] Tilemap tilemap;
    [SerializeField] TileBase mountainTile;
    [SerializeField] TileBase floorTile;

    [Header("Input")]
    [SerializeField] KeyCode digKey = KeyCode.E;

    private PlayerInventory inventory;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (tilemap == null)
        {
            tilemap = FindFirstObjectByType<Tilemap>();
        }
    }

    void Update()
    {
        if (inventory == null || !inventory.hasPickaxe)
            return;

        if (Input.GetKeyDown(digKey))
        {
            DigMountainUnderPlayer();
        }
    }

    void DigMountainUnderPlayer()
    {
        Vector3 worldPos = transform.position;
        Vector3Int cellPos = tilemap.WorldToCell(worldPos);

        TileBase tile = tilemap.GetTile(cellPos);
        if (tile == mountainTile)
        {
            tilemap.SetTile(cellPos, floorTile);
            Debug.Log("Pickaxe: turned mountain into floor at " + cellPos);
        }
        else
        {
            Debug.Log("Pickaxe: no mountain tile under the player to dig.");
        }
    }
}
