using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [Header("Íàñòðîéêè àïòå÷êè")]
    public int healAmount = 25;
    public string packId = ""; // Îñòàâü ÏÓÑÒÛÌ - çàïîëíèòñÿ àâòîìàòè÷åñêè

    private bool isCollected = false;

    void Start()
    {
        // Åñëè ID íå çàäàí â èíñïåêòîðå - ãåíåðèðóåì
        if (string.IsNullOrEmpty(packId))
        {
            packId = GenerateStaticId();
        }

        // Ïðîâåðÿåì, íå ñîáðàíà ëè àïòå÷êà
        CheckIfCollected();
    }

    void CheckIfCollected()
    {
        // Êëþ÷ äëÿ ñîõðàíåíèÿ ñîñòîÿíèÿ àïòå÷êè
        string saveKey = "HealthPackCollected_" + packId;

        if (PlayerPrefs.HasKey(saveKey))
        {
            isCollected = true;
            gameObject.SetActive(false);
            Debug.Log($"Àïòå÷êà {packId} óæå ñîáðàíà ðàíåå");
        }
        else
        {
            Debug.Log($"Àïòå÷êà {packId} äîñòóïíà äëÿ ñáîðà");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            // Ïîëó÷àåì êîìïîíåíò èãðîêà
            CharacterControl player = other.GetComponent<CharacterControl>();
            if (player != null)
            {
                CollectHealthPack(player);
            }
        }
    }

    public void CollectHealthPack(CharacterControl player)
    {
        if (isCollected) return;

        isCollected = true;

        // 1. Ëå÷èì èãðîêà
        player.Heal(healAmount);

        // 2. Ïîìå÷àåì àïòå÷êó êàê ñîáðàííóþ
        string saveKey = "HealthPackCollected_" + packId;
        PlayerPrefs.SetInt(saveKey, 1);

        // 3. Ñîõðàíÿåì âñ¸
        PlayerPrefs.Save();

        // 4. Âèçóàëüíûå ýôôåêòû
        StartCoroutine(CollectEffect());

        Debug.Log($"Àïòå÷êà {packId} èñïîëüçîâàíà! +{healAmount} HP");
    }

    System.Collections.IEnumerator CollectEffect()
    {
        // Ïðîñòîé ýôôåêò ñáîðà
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Ìîæíî äîáàâèòü ÷àñòèöû, çâóê è ò.ä.
        // if (collectSound != null) AudioSource.PlayClipAtPoint(collectSound, transform.position);
        // if (collectEffect != null) Instantiate(collectEffect, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        // Ïîëíîñòüþ ñêðûâàåì
        gameObject.SetActive(false);
    }

    // Ãåíåðàöèÿ óíèêàëüíîãî ñòàòè÷åñêîãî ID (êàê â CoinSimple)
    string GenerateStaticId()
    {
        // Èñïîëüçóåì ïîçèöèþ îáúåêòà
        Vector3 pos = transform.position;
        string positionId = $"X{pos.x:F2}_Y{pos.y:F2}_Z{pos.z:F2}";

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        return $"HEALTH_{sceneName}_{positionId}";
    }

    // Ðåäàêòîð: ïîêàçûâàòü ID â èíñïåêòîðå
    void OnValidate()
    {
        if (string.IsNullOrEmpty(packId))
        {
            packId = "Health_" + gameObject.GetInstanceID();
        }
    }

    // Ìåòîä äëÿ ñáðîñà ñîñòîÿíèÿ (äëÿ òåñòèðîâàíèÿ)
    [ContextMenu("Ñáðîñèòü ñîñòîÿíèå àïòå÷êè")]
    public void ResetHealthPack()
    {
        string saveKey = "HealthPackCollected_" + packId;
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();

        isCollected = false;
        gameObject.SetActive(true);
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;

        Debug.Log($"Àïòå÷êà {packId} ñáðîøåíà!");
    }

    // Ìåòîä äëÿ ðó÷íîãî ñáîðà (íàïðèìåð, èç äðóãèõ ñêðèïòîâ)
    public void ForceCollect(CharacterControl player)
    {
        if (!isCollected && player != null)
        {
            CollectHealthPack(player);
        }
    }
}
