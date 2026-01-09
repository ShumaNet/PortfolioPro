using UnityEngine;
       
public class LavaScript : MonoBehaviour
{

    
    void OnTriggerEnter(Collider other)
    {
        GhostScript ghostScript = other.GetComponent<GhostScript>();
        if (other.CompareTag("Lava"))
        {
            ghostScript.TakeLavaDamage();
        }
    }

}


