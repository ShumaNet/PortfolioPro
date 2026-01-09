using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public string nextLevelName;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Ñîõðàíÿåì ïåðåä ïåðåõîäîì
            SaveBeforeLoading();

            // Ïåðåõîäèì íà ñëåäóþùèé óðîâåíü
            SceneManager.LoadScene(nextLevelName);
        }
    }

    // Äëÿ êíîïîê èëè äðóãèõ òðèããåðîâ
    public void LoadLevel()
    {
        // Ñîõðàíÿåì ïåðåä ïåðåõîäîì
        SaveBeforeLoading();

        SceneManager.LoadScene(nextLevelName);
    }

    // Àëüòåðíàòèâíûé ìåòîä ñ àâòî-ñîõðàíåíèåì
    public void LoadLevelWithAutoSave()
    {
        // Ñîõðàíÿåì òåêóùåå ñîñòîÿíèå
        SaveBeforeLoading();

        // Îáíîâëÿåì óðîâåíü â ñîõðàíåíèè
        if (SaveManager.Instance != null)
        {
            GameData data = SaveManager.Instance.GetGameData();
            if (data != null)
            {
                data.currentLevel = nextLevelName;
                SaveManager.Instance.SaveGame(); // Ñîõðàíÿåì ñ íîâûì óðîâíåì
            }
        }

        SceneManager.LoadScene(nextLevelName);
    }

    // Îáùèé ìåòîä ñîõðàíåíèÿ ïåðåä çàãðóçêîé óðîâíÿ
    private void SaveBeforeLoading()
    {
        if (SaveManager.Instance != null)
        {
            // Ñîõðàíÿåì äàííûå èãðîêà ïåðåä ïåðåõîäîì
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterControl character = player.GetComponent<CharacterControl>();
                if (character != null)
                {
                    // Îáíîâëÿåì äàííûå â GameData
                    GameData data = SaveManager.Instance.GetGameData();
                    data.playerHealth = character.GetCurrentHealth();
                    data.playerPosition = player.transform.position;
                    data.currentLevel = nextLevelName;
                    data.playerMoney = PlayerPrefs.GetInt("Money", 0);

                    // Ñîõðàíÿåì
                    SaveManager.Instance.SaveGame();
                }
            }
        }
    }
}
