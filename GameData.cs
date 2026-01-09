using System;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public string currentLevel;
    public Vector3 playerPosition;
    public int playerHealth;
    public int playerMoney;
    public bool isNewGame;
    public DateTime lastSaveTime;

    // Состояние монет
    public SerializableDictionary<string, bool> collectedCoins;

    // Состояние врагов
    public SerializableDictionary<string, EnemySaveData> enemyStates;

    public GameData()
    {
        currentLevel = "Level1";
        playerPosition = Vector3.zero;
        playerHealth = 100;
        playerMoney = 0;
        isNewGame = true;
        collectedCoins = new SerializableDictionary<string, bool>();
        enemyStates = new SerializableDictionary<string, EnemySaveData>();
    }
}

[System.Serializable]
public class EnemySaveData
{
    public float health;
    public string state;
    public Vector3 position;
    public Quaternion rotation;
    public int patrolIndex;
}

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : System.Collections.Generic.Dictionary<TKey, TValue>,
    UnityEngine.ISerializationCallbackReceiver
{
    [SerializeField] private System.Collections.Generic.List<TKey> keys = new System.Collections.Generic.List<TKey>();
    [SerializeField] private System.Collections.Generic.List<TValue> values = new System.Collections.Generic.List<TValue>();

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();

        foreach (var pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
            throw new System.Exception($"keys count ({keys.Count}) != values count ({values.Count})");

        for (int i = 0; i < keys.Count; i++)
            this[keys[i]] = values[i];
    }
}