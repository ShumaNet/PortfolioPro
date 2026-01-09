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
            // Ñîõðàíÿåì äàííûå èãðîêà
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

            // Ñîõðàíÿåì äåíüãè
            currentGameData.playerMoney = PlayerPrefs.GetInt("Money", 0);

            // Ñîõðàíÿåì òåêóùèé óðîâåíü
            currentGameData.currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            currentGameData.lastSaveTime = System.DateTime.Now;
            currentGameData.isNewGame = false;

            // Ñîõðàíÿåì ñîñòîÿíèå ìîíåò
            SaveCoinStates();

            // Ñîõðàíÿåì ñîñòîÿíèå âðàãîâ
            SaveEnemyStates();

            // Ñåðèàëèçóåì è ñîõðàíÿåì
            string json = JsonUtility.ToJson(currentGameData);
            string savePath = GetSavePath();
            File.WriteAllText(savePath, json);

            Debug.Log($"Èãðà ñîõðàíåíà: {savePath}");
            Debug.Log($"Çäîðîâüå: {currentGameData.playerHealth}, Äåíüãè: {currentGameData.playerMoney}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Îøèáêà ïðè ñîõðàíåíèè: {e.Message}");
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
                Debug.Log("Èãðà çàãðóæåíà");
            }
            else
            {
                currentGameData = new GameData();
                Debug.Log("Ñîçäàíî íîâîå ñîõðàíåíèå");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Îøèáêà ïðè çàãðóçêå: {e.Message}");
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

            // Î÷èùàåì PlayerPrefs
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            currentGameData = new GameData();

            Debug.Log("Âñå ñîõðàíåíèÿ óäàëåíû");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Îøèáêà ïðè óäàëåíèè ñîõðàíåíèé: {e.Message}");
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
        // Âîññòàíàâëèâàåì äåíüãè
        PlayerPrefs.SetInt("Money", currentGameData.playerMoney);
        PlayerPrefs.Save();

        // Âîññòàíàâëèâàåì ìîíåòû
        RestoreCoinStates();

        // Âîññòàíàâëèâàåì âðàãîâ
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
