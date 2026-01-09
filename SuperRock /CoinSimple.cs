using UnityEngine;

public class CoinSimple : MonoBehaviour
{
    [Header("Íàñòðîéêè ìîíåòû")]
    public int coinValue = 1;
    public string coinId = ""; // Îñòàâü ÏÓÑÒÛÌ - çàïîëíèòñÿ àâòîìàòè÷åñêè

    private bool isCollected = false;

    void Start()
    {
        // Åñëè ID íå çàäàí â èíñïåêòîðå - ãåíåðèðóåì
        if (string.IsNullOrEmpty(coinId))
        {
            coinId = GenerateStaticId();
        }

        // Ïðîâåðÿåì, íå ñîáðàíà ëè ìîíåòà
        CheckIfCollected();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            CollectCoin();
        }
    }

    void CheckIfCollected()
    {
        // Êëþ÷ äëÿ ñîõðàíåíèÿ ñîñòîÿíèÿ ìîíåòû
        string saveKey = "CoinCollected_" + coinId;

        if (PlayerPrefs.HasKey(saveKey))
        {
            isCollected = true;
            gameObject.SetActive(false);
            Debug.Log($"Ìîíåòà {coinId} óæå ñîáðàíà ðàíåå");
        }
        else
        {
            Debug.Log($"Ìîíåòà {coinId} äîñòóïíà äëÿ ñáîðà");
        }
    }

    void CollectCoin()
    {
        if (isCollected) return;

        isCollected = true;

        // 1. Äîáàâëÿåì äåíüãè
        int currentMoney = PlayerPrefs.GetInt("Money", 0);
        currentMoney += coinValue;
        PlayerPrefs.SetInt("Money", currentMoney);

        // 2. Ïîìå÷àåì ìîíåòó êàê ñîáðàííóþ
        string saveKey = "CoinCollected_" + coinId;
        PlayerPrefs.SetInt(saveKey, 1);

        // 3. Ñîõðàíÿåì âñ¸
        PlayerPrefs.Save();

        // 4. Âèçóàëüíûå ýôôåêòû
        StartCoroutine(CollectEffect());

        Debug.Log($"Ìîíåòà {coinId} ñîáðàíà! +{coinValue}$. Âñåãî: {currentMoney}$");
    }

    System.Collections.IEnumerator CollectEffect()
    {
        // Ïðîñòîé ýôôåêò ñáîðà
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Ìîæíî äîáàâèòü ÷àñòèöû, çâóê è ò.ä.

        yield return new WaitForSeconds(0.5f);

        // Ïîëíîñòüþ ñêðûâàåì
        gameObject.SetActive(false);
    }

    string GenerateStaticId()
    {
        // Èñïîëüçóåì ïîçèöèþ îáúåêòà - îíà óíèêàëüíà íà ñöåíå
        Vector3 pos = transform.position;
        string positionId = $"X{pos.x:F2}_Y{pos.y:F2}_Z{pos.z:F2}";

        // Èìÿ ñöåíû äëÿ ðàçíûõ óðîâíåé
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        return $"COIN_{sceneName}_{positionId}";
    }

    // Ðåäàêòîð: ïîêàçûâàòü ID â èíñïåêòîðå
    void OnValidate()
    {
        if (string.IsNullOrEmpty(coinId))
        {
            coinId = "Coin_" + gameObject.GetInstanceID();
        }
    }


}
