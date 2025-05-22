using UnityEngine;

public class ScinPlayer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite[] scins;

    private void Start()
    {
        int scinsID = PlayerPrefs.GetInt("scinsID");

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = scins[scinsID];
    }
}
