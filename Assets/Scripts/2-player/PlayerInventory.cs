using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool hasBoat     { get; private set; }
    public bool hasGoat     { get; private set; }
    public bool hasPickaxe  { get; private set; }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"OnTriggerEnter2D with {other.name}, tag={other.tag}");

        if (other.CompareTag("Boat"))
        {
            hasBoat = true;
            Debug.Log("Picked up BOAT, hasBoat = " + hasBoat);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Goat"))
        {
            hasGoat = true;
            Debug.Log("Picked up GOAT, hasGoat = " + hasGoat);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Pickaxe"))
        {
            hasPickaxe = true;
            Debug.Log("Picked up PICKAXE, hasPickaxe = " + hasPickaxe);
            Destroy(other.gameObject);
        }
    }
}
