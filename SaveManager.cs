using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SAVE_FILE = "save.dat";
    private GameData currentGameData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        try
        {
            // Сохраняем данные игрока
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterControl character = player.GetComponent<CharacterControl>();
                if (character != null)
                {
                    currentGameData.playerHealth = character.GetCurrentHealth();
                    currentGameData.playerPosition = player.transform.position;
                }
            }

            // Сохраняем деньги
            currentGameData.playerMoney = PlayerPrefs.GetInt("Money", 0);

            // Сохраняем текущий уровень
            currentGameData.currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            currentGameData.lastSaveTime = System.DateTime.Now;
            currentGameData.isNewGame = false;

            // Сохраняем состояние монет
            SaveCoinStates();

            // Сохраняем состояние врагов
            SaveEnemyStates();

            // Сериализуем и сохраняем
            string json = JsonUtility.ToJson(currentGameData);
            string savePath = GetSavePath();
            File.WriteAllText(savePath, json);

            Debug.Log($"Игра сохранена: {savePath}");
            Debug.Log($"Здоровье: {currentGameData.playerHealth}, Деньги: {currentGameData.playerMoney}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при сохранении: {e.Message}");
        }
    }

    public void LoadGame()
    {
        try
        {
            string savePath = GetSavePath();

            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                currentGameData = JsonUtility.FromJson<GameData>(json);
                Debug.Log("Игра загружена");
            }
            else
            {
                currentGameData = new GameData();
                Debug.Log("Создано новое сохранение");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при загрузке: {e.Message}");
            currentGameData = new GameData();
        }
    }

    public GameData GetGameData()
    {
        return currentGameData;
    }

    public bool HasSave()
    {
        string savePath = GetSavePath();
        return File.Exists(savePath) && !currentGameData.isNewGame;
    }

    public void DeleteAllSaves()
    {
        try
        {
            string savePath = GetSavePath();
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            // Очищаем PlayerPrefs
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            currentGameData = new GameData();

            Debug.Log("Все сохранения удалены");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при удалении сохранений: {e.Message}");
        }
    }

    public string GetSavedLevel()
    {
        return currentGameData.currentLevel;
    }

    private void SaveCoinStates()
    {
        currentGameData.collectedCoins.Clear();

        CoinSimple[] coins = FindObjectsOfType<CoinSimple>();
        foreach (CoinSimple coin in coins)
        {
            if (!string.IsNullOrEmpty(coin.coinId))
            {
                bool isCollected = !coin.gameObject.activeSelf;
                currentGameData.collectedCoins[coin.coinId] = isCollected;
            }
        }
    }

    private void SaveEnemyStates()
    {
        currentGameData.enemyStates.Clear();

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            string enemyId = enemy.gameObject.name + "_" + enemy.GetInstanceID();
            EnemySaveData saveData = new EnemySaveData
            {
                health = enemy.GetHealthForSave(),
                state = enemy.GetStateForSave(),
                position = enemy.transform.position,
                rotation = enemy.transform.rotation,
                patrolIndex = enemy.GetCurrentPatrolIndex()
            };

            currentGameData.enemyStates[enemyId] = saveData;
        }
    }

    public void RestoreGameState()
    {
        // Восстанавливаем деньги
        PlayerPrefs.SetInt("Money", currentGameData.playerMoney);
        PlayerPrefs.Save();

        // Восстанавливаем монеты
        RestoreCoinStates();

        // Восстанавливаем врагов
        RestoreEnemyStates();
    }

    private void RestoreCoinStates()
    {
        if (currentGameData.collectedCoins == null) return;

        CoinSimple[] coins = FindObjectsOfType<CoinSimple>();
        foreach (CoinSimple coin in coins)
        {
            if (!string.IsNullOrEmpty(coin.coinId) &&
                currentGameData.collectedCoins.ContainsKey(coin.coinId))
            {
                if (currentGameData.collectedCoins[coin.coinId])
                {
                    coin.gameObject.SetActive(false);
                }
            }
        }
    }

    private void RestoreEnemyStates()
    {
        if (currentGameData.enemyStates == null) return;

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            string enemyId = enemy.gameObject.name + "_" + enemy.GetInstanceID();

            if (currentGameData.enemyStates.ContainsKey(enemyId))
            {
                EnemySaveData saveData = currentGameData.enemyStates[enemyId];

                enemy.SetHealthFromSave(saveData.health);
                enemy.SetStateFromSave(saveData.state);
                enemy.SetPosition(saveData.position);
                enemy.SetRotation(saveData.rotation);
                enemy.SetCurrentPatrolIndex(saveData.patrolIndex);

                if (saveData.health <= 0)
                {
                    enemy.DisableEnemy();
                }
            }
        }
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FILE);
    }
}