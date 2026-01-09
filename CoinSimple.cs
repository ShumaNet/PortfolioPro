using UnityEngine;

public class CoinSimple : MonoBehaviour
{
    [Header("Настройки монеты")]
    public int coinValue = 1;
    public string coinId = ""; // Оставь ПУСТЫМ - заполнится автоматически

    private bool isCollected = false;

    void Start()
    {
        // Если ID не задан в инспекторе - генерируем
        if (string.IsNullOrEmpty(coinId))
        {
            coinId = GenerateStaticId();
        }

        // Проверяем, не собрана ли монета
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
        // Ключ для сохранения состояния монеты
        string saveKey = "CoinCollected_" + coinId;

        if (PlayerPrefs.HasKey(saveKey))
        {
            isCollected = true;
            gameObject.SetActive(false);
            Debug.Log($"Монета {coinId} уже собрана ранее");
        }
        else
        {
            Debug.Log($"Монета {coinId} доступна для сбора");
        }
    }

    void CollectCoin()
    {
        if (isCollected) return;

        isCollected = true;

        // 1. Добавляем деньги
        int currentMoney = PlayerPrefs.GetInt("Money", 0);
        currentMoney += coinValue;
        PlayerPrefs.SetInt("Money", currentMoney);

        // 2. Помечаем монету как собранную
        string saveKey = "CoinCollected_" + coinId;
        PlayerPrefs.SetInt(saveKey, 1);

        // 3. Сохраняем всё
        PlayerPrefs.Save();

        // 4. Визуальные эффекты
        StartCoroutine(CollectEffect());

        Debug.Log($"Монета {coinId} собрана! +{coinValue}$. Всего: {currentMoney}$");
    }

    System.Collections.IEnumerator CollectEffect()
    {
        // Простой эффект сбора
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Можно добавить частицы, звук и т.д.

        yield return new WaitForSeconds(0.5f);

        // Полностью скрываем
        gameObject.SetActive(false);
    }

    string GenerateStaticId()
    {
        // Используем позицию объекта - она уникальна на сцене
        Vector3 pos = transform.position;
        string positionId = $"X{pos.x:F2}_Y{pos.y:F2}_Z{pos.z:F2}";

        // Имя сцены для разных уровней
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        return $"COIN_{sceneName}_{positionId}";
    }

    // Редактор: показывать ID в инспекторе
    void OnValidate()
    {
        if (string.IsNullOrEmpty(coinId))
        {
            coinId = "Coin_" + gameObject.GetInstanceID();
        }
    }


}