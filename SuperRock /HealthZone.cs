using UnityEngine;
using System.Collections;
using TMPro;

public class HealthZone : MonoBehaviour
{
    [Header("Íàñòðîéêè çîíû")]
    public int damagePerSecond = 10;
    public float warningRange = 10f;
    public float damageRange = 5f;
    public string warningMessage = "ÂÍÈÌÀÍÈÅ: Ðàäèàöèîííàÿ çîíà! Óéäèòå íåìåäëåííî!";

    [Header("Âèçóàëüíûå ýôôåêòû")]
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
            // Èãðîê â çîíå óðîíà
            if (!isPlayerInZone)
            {
                EnterDamageZone();
            }

            timeInZone += Time.deltaTime;

            // Íàíîñèì óðîí êàæäóþ ñåêóíäó
            if (timeInZone >= 1f)
            {
                playerHealth.TakeDamage(damagePerSecond);
                timeInZone = 0f;

                // Ýôôåêò óðîíà
                if (damageParticles != null && !damageParticles.isPlaying)
                    damageParticles.Play();

                if (damageSound != null)
                    audioSource.PlayOneShot(damageSound);
            }

            UpdateCountdownUI();
        }
        else if (distance <= warningRange)
        {
            // Èãðîê â çîíå ïðåäóïðåæäåíèÿ
            if (isPlayerInZone)
            {
                ExitDamageZone();
            }

            ShowWarningUI();
        }
        else
        {
            // Èãðîê âíå çîíû
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

        Debug.Log("Èãðîê âîøåë â îïàñíóþ çîíó!");

        if (warningParticles != null)
            warningParticles.Stop();

        if (damageParticles != null)
            damageParticles.Play();
    }

    void ExitDamageZone()
    {
        isPlayerInZone = false;
        timeInZone = 0f;

        Debug.Log("Èãðîê âûøåë èç îïàñíîé çîíû");

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
                // Îáðàòíûé îòñ÷åò äî ñëåäóþùåãî òèêà óðîíà
                float timeLeft = 1f - timeInZone;
                countdownText.text = $"Óðîí ÷åðåç: {timeLeft:F1}ñ";
                countdownText.color = Color.Lerp(Color.red, Color.yellow, timeLeft);
            }

            if (warningText != null)
                warningText.text = "ÎÏÀÑÍÎÑÒÜ! ÓÊËÎÍÈÒÅÑÜ!";
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
        // Çîíà ïðåäóïðåæäåíèÿ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, warningRange);

        // Çîíà óðîíà
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRange);

        // Ïîêàçûâàåì íàïðàâëåíèå ê èãðîêó
        if (player != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}
