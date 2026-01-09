using TMPro;
using UnityEngine;


public class CoinScript : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 3f;
    public TextMeshProUGUI TextCoin;
    
    void Start()
    {
        
        TextCoin = GameObject.Find("TextCoin").GetComponent<TextMeshProUGUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GhostScript ghost = other.GetComponent<GhostScript>();
            if (ghost != null)
            {
                ghost.AddCash();
                TextCoin.SetText("Coin: {0}", ghost.Cash);

                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("GhostScript component not found on collider.");
            }
        }
    }
    void Update()
    {
       
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
