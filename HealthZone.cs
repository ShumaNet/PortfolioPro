using UnityEngine;
using System.Collections;
using TMPro;

public class HealthZone : MonoBehaviour
{
    [Header("Настройки зоны")]
    public int damagePerSecond = 10;
    public float warningRange = 10f;
    public float damageRange = 5f;
    public string warningMessage = "ВНИМАНИЕ: Радиационная зона! Уйдите немедленно!";

    [Header("Визуальные эффекты")]
    public ParticleSystem warningParticles;
    public ParticleSystem damageParticles;
    public AudioClip warningSound;
    public AudioClip damageSound;

    [Header("UI")]
    public GameObject warningUI;
    public TextMeshProUGUI warningText;
    public TextMeshProUGUI countdownText;

    private AudioSource audioSource;
    private Transform player;
    private CharacterControl playerHealth;
    private bool isPlayerInZone = false;
    private float timeInZone = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            playerHealth = player.GetComponent<CharacterControl>();
        }

        if (warningUI != null)
            warningUI.SetActive(false);
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= damageRange)
        {
            // Игрок в зоне урона
            if (!isPlayerInZone)
            {
                EnterDamageZone();
            }

            timeInZone += Time.deltaTime;

            // Наносим урон каждую секунду
            if (timeInZone >= 1f)
            {
                playerHealth.TakeDamage(damagePerSecond);
                timeInZone = 0f;

                // Эффект урона
                if (damageParticles != null && !damageParticles.isPlaying)
                    damageParticles.Play();

                if (damageSound != null)
                    audioSource.PlayOneShot(damageSound);
            }

            UpdateCountdownUI();
        }
        else if (distance <= warningRange)
        {
            // Игрок в зоне предупреждения
            if (isPlayerInZone)
            {
                ExitDamageZone();
            }

            ShowWarningUI();
        }
        else
        {
            // Игрок вне зоны
            if (isPlayerInZone)
            {
                ExitDamageZone();
            }
            HideWarningUI();
        }
    }

    void EnterDamageZone()
    {
        isPlayerInZone = true;
        timeInZone = 0f;

        Debug.Log("Игрок вошел в опасную зону!");

        if (warningParticles != null)
            warningParticles.Stop();

        if (damageParticles != null)
            damageParticles.Play();
    }

    void ExitDamageZone()
    {
        isPlayerInZone = false;
        timeInZone = 0f;

        Debug.Log("Игрок вышел из опасной зоны");

        if (damageParticles != null)
            damageParticles.Stop();

        if (warningParticles != null)
            warningParticles.Play();

        HideWarningUI();
    }

    void ShowWarningUI()
    {
        if (warningUI != null)
        {
            warningUI.SetActive(true);

            if (warningText != null)
                warningText.text = warningMessage;

            if (countdownText != null)
                countdownText.text = "";
        }

        if (warningParticles != null && !warningParticles.isPlaying)
            warningParticles.Play();

        if (warningSound != null && !audioSource.isPlaying)
            audioSource.PlayOneShot(warningSound);
    }

    void UpdateCountdownUI()
    {
        if (warningUI != null && warningUI.activeSelf)
        {
            if (countdownText != null)
            {
                // Обратный отсчет до следующего тика урона
                float timeLeft = 1f - timeInZone;
                countdownText.text = $"Урон через: {timeLeft:F1}с";
                countdownText.color = Color.Lerp(Color.red, Color.yellow, timeLeft);
            }

            if (warningText != null)
                warningText.text = "ОПАСНОСТЬ! УКЛОНИТЕСЬ!";
        }
    }

    void HideWarningUI()
    {
        if (warningUI != null)
            warningUI.SetActive(false);

        if (warningParticles != null)
            warningParticles.Stop();
    }

    void OnDrawGizmosSelected()
    {
        // Зона предупреждения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, warningRange);

        // Зона урона
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);

        // Показываем направление к игроку
        if (player != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}