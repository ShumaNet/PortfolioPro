using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [Header("Настройки аптечки")]
    public int healAmount = 25;
    public string packId = ""; // Оставь ПУСТЫМ - заполнится автоматически

    private bool isCollected = false;

    void Start()
    {
        // Если ID не задан в инспекторе - генерируем
        if (string.IsNullOrEmpty(packId))
        {
            packId = GenerateStaticId();
        }

        // Проверяем, не собрана ли аптечка
        CheckIfCollected();
    }

    void CheckIfCollected()
    {
        // Ключ для сохранения состояния аптечки
        string saveKey = "HealthPackCollected_" + packId;

        if (PlayerPrefs.HasKey(saveKey))
        {
            isCollected = true;
            gameObject.SetActive(false);
            Debug.Log($"Аптечка {packId} уже собрана ранее");
        }
        else
        {
            Debug.Log($"Аптечка {packId} доступна для сбора");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            // Получаем компонент игрока
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

        // 1. Лечим игрока
        player.Heal(healAmount);

        // 2. Помечаем аптечку как собранную
        string saveKey = "HealthPackCollected_" + packId;
        PlayerPrefs.SetInt(saveKey, 1);

        // 3. Сохраняем всё
        PlayerPrefs.Save();

        // 4. Визуальные эффекты
        StartCoroutine(CollectEffect());

        Debug.Log($"Аптечка {packId} использована! +{healAmount} HP");
    }

    System.Collections.IEnumerator CollectEffect()
    {
        // Простой эффект сбора
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Можно добавить частицы, звук и т.д.
        // if (collectSound != null) AudioSource.PlayClipAtPoint(collectSound, transform.position);
        // if (collectEffect != null) Instantiate(collectEffect, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        // Полностью скрываем
        gameObject.SetActive(false);
    }

    // Генерация уникального статического ID (как в CoinSimple)
    string GenerateStaticId()
    {
        // Используем позицию объекта
        Vector3 pos = transform.position;
        string positionId = $"X{pos.x:F2}_Y{pos.y:F2}_Z{pos.z:F2}";

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        return $"HEALTH_{sceneName}_{positionId}";
    }

    // Редактор: показывать ID в инспекторе
    void OnValidate()
    {
        if (string.IsNullOrEmpty(packId))
        {
            packId = "Health_" + gameObject.GetInstanceID();
        }
    }

    // Метод для сброса состояния (для тестирования)
    [ContextMenu("Сбросить состояние аптечки")]
    public void ResetHealthPack()
    {
        string saveKey = "HealthPackCollected_" + packId;
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();

        isCollected = false;
        gameObject.SetActive(true);
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;

        Debug.Log($"Аптечка {packId} сброшена!");
    }

    // Метод для ручного сбора (например, из других скриптов)
    public void ForceCollect(CharacterControl player)
    {
        if (!isCollected && player != null)
        {
            CollectHealthPack(player);
        }
    }
}