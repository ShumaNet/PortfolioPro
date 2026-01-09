using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public string nextLevelName;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Сохраняем перед переходом
            SaveBeforeLoading();

            // Переходим на следующий уровень
            SceneManager.LoadScene(nextLevelName);
        }
    }

    // Для кнопок или других триггеров
    public void LoadLevel()
    {
        // Сохраняем перед переходом
        SaveBeforeLoading();

        SceneManager.LoadScene(nextLevelName);
    }

    // Альтернативный метод с авто-сохранением
    public void LoadLevelWithAutoSave()
    {
        // Сохраняем текущее состояние
        SaveBeforeLoading();

        // Обновляем уровень в сохранении
        if (SaveManager.Instance != null)
        {
            GameData data = SaveManager.Instance.GetGameData();
            if (data != null)
            {
                data.currentLevel = nextLevelName;
                SaveManager.Instance.SaveGame(); // Сохраняем с новым уровнем
            }
        }

        SceneManager.LoadScene(nextLevelName);
    }

    // Общий метод сохранения перед загрузкой уровня
    private void SaveBeforeLoading()
    {
        if (SaveManager.Instance != null)
        {
            // Сохраняем данные игрока перед переходом
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterControl character = player.GetComponent<CharacterControl>();
                if (character != null)
                {
                    // Обновляем данные в GameData
                    GameData data = SaveManager.Instance.GetGameData();
                    data.playerHealth = character.GetCurrentHealth();
                    data.playerPosition = player.transform.position;
                    data.currentLevel = nextLevelName;
                    data.playerMoney = PlayerPrefs.GetInt("Money", 0);

                    // Сохраняем
                    SaveManager.Instance.SaveGame();
                }
            }
        }
    }
}