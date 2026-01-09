using UnityEngine;
using System.Collections;

public class JumpAttack : MonoBehaviour
{
    [Header("Íàñòðîéêè ïðûæêà-àòàêè")]
    public float maxJumpDistance = 15f;           // Ìàêñèìàëüíàÿ äèñòàíöèÿ ïðûæêà
    public float minJumpDistance = 3f;            // Ìèíèìàëüíàÿ äèñòàíöèÿ ïðûæêà
    public float maxJumpHeight = 5f;              // Ìàêñèìàëüíàÿ âûñîòà ïðûæêà
    public float minJumpHeight = 1.5f;            // Ìèíèìàëüíàÿ âûñîòà ïðûæêà
    public float maxJumpDuration = 1.5f;          // Ìàêñèìàëüíàÿ äëèòåëüíîñòü ïðûæêà
    public float minJumpDuration = 0.5f;          // Ìèíèìàëüíàÿ äëèòåëüíîñòü ïðûæêà
    public int attackDamage = 15;                 // Óðîí ïðè ïðûæêå-àòàêå
    public float attackCooldown = 2f;             // Âðåìÿ ïåðåçàðÿäêè
    public float attackRadius = 5f;               // Ðàäèóñ ïîðàæåíèÿ ïðè ïðèçåìëåíèè
    public AnimationCurve jumpHeightCurve = AnimationCurve.Linear(0, 0, 1, 1); // Êðèâàÿ âûñîòû ïðûæêà
    public AnimationCurve jumpSpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);  // Êðèâàÿ ñêîðîñòè ïðûæêà

    [Header("Íàñòðîéêè îáíàðóæåíèÿ")]
    public LayerMask enemyLayer;                  // Ñëîé âðàãîâ
    public LayerMask obstacleLayer;               // Ñëîé ïðåïÿòñòâèé

    [Header("Âèçóàëüíûå ýôôåêòû")]
    public GameObject jumpChargeEffect;           // Ýôôåêò ïðè çàðÿäå ïðûæêà
    public GameObject jumpAttackEffect;           // Ýôôåêò ïðè ïðûæêå-àòàêå
    public GameObject landingEffect;              // Ýôôåêò ïðè ïðèçåìëåíèè
    public AudioClip jumpChargeSound;             // Çâóê çàðÿäà ïðûæêà
    public AudioClip jumpSound;                   // Çâóê ïðûæêà
    public AudioClip attackSound;                 // Çâóê àòàêè

    private bool isJumpAttacking = false;         // Ôëàã ïðûæêà-àòàêè
    private bool canJumpAttack = true;            // Ìîæåò ëè âûïîëíèòü ïðûæîê-àòàêó
    private float lastJumpAttackTime;             // Âðåìÿ ïîñëåäíåé ïðûæê-àòàêè

    private Animator animator;
    private AudioSource audioSource;
    private Camera mainCamera;
    private Rigidbody playerRigidbody;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        playerRigidbody = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && canJumpAttack && !isJumpAttacking)
            TryPerformJumpAttack();
    }

    void TryPerformJumpAttack()
    {
        GameObject target = FindJumpAttackTarget();

        if (target != null)
            StartCoroutine(PerformJumpAttack(target.transform));
        else
            Debug.Log("Íåò ïîäõîäÿùèõ öåëåé äëÿ ïðûæêà-àòàêè!");
    }

    GameObject FindJumpAttackTarget()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, maxJumpDistance, enemyLayer);

        if (enemiesInRange.Length == 0)
            return null;

        GameObject bestTarget = null;
        float bestScore = -Mathf.Infinity;

        Vector3 cameraDirection = mainCamera.transform.forward;
        cameraDirection.y = 0;
        cameraDirection.Normalize();

        foreach (Collider enemyCollider in enemiesInRange)
        {
            GameObject enemy = enemyCollider.gameObject;
            Vector3 toEnemy = (enemy.transform.position - transform.position);
            float distance = toEnemy.magnitude;

            // Ïðîïóñêàåì ñëèøêîì áëèçêèõ âðàãîâ
            if (distance < minJumpDistance)
                continue;

            if (Physics.Raycast(transform.position + Vector3.up, toEnemy.normalized, distance, obstacleLayer))
                continue;

            float distanceScore = 1f - (distance / maxJumpDistance);
            Vector3 toEnemyNormalized = toEnemy.normalized;
            toEnemyNormalized.y = 0;
            float directionScore = Vector3.Dot(cameraDirection, toEnemyNormalized);
            float totalScore = distanceScore * 0.6f + (directionScore + 1f) * 0.4f;

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    IEnumerator PerformJumpAttack(Transform target)
    {
        isJumpAttacking = true;
        CharacterControl.canWalk = false;
        canJumpAttack = false;
        lastJumpAttackTime = Time.time;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = target.position;

        // Ðàñ÷åò ðàññòîÿíèÿ äî öåëè
        float distance = Vector3.Distance(startPosition, targetPosition);
        distance = Mathf.Clamp(distance, minJumpDistance, maxJumpDistance);

        // Ðàñ÷åò ïàðàìåòðîâ ïðûæêà â çàâèñèìîñòè îò ðàññòîÿíèÿ
        float normalizedDistance = (distance - minJumpDistance) / (maxJumpDistance - minJumpDistance);
        float jumpHeight = Mathf.Lerp(minJumpHeight, maxJumpHeight, normalizedDistance);
        float jumpDuration = Mathf.Lerp(minJumpDuration, maxJumpDuration, normalizedDistance);

        // Ïîçèöèÿ ïðèçåìëåíèÿ (íåìíîãî ïåðåä âðàãîì)
        Vector3 directionToTarget = (targetPosition - startPosition).normalized;
        float landingOffset = Mathf.Lerp(1f, 2f, normalizedDistance); // Îòñòóï óâåëè÷èâàåòñÿ ñ ðàññòîÿíèåì
        targetPosition = targetPosition - directionToTarget * landingOffset;
        targetPosition.y = startPosition.y;

        // Ýôôåêò çàðÿäà ïðûæêà
        if (jumpChargeEffect != null)
            Instantiate(jumpChargeEffect, transform.position, Quaternion.identity);

        if (jumpChargeSound != null)
            audioSource.PlayOneShot(jumpChargeSound);

        yield return new WaitForSeconds(0.2f); // Êîðîòêàÿ çàäåðæêà äëÿ çàðÿäà

        // Îòêëþ÷àåì ôèçèêó Rigidbody íà âðåìÿ ïðûæêà
        bool wasKinematic = playerRigidbody.isKinematic;
        playerRigidbody.isKinematic = true;

        // Çàïóñêàåì àíèìàöèþ ïðûæêà-àòàêè
        if (animator != null)
            animator.Play("Atack");

        // Âîñïðîèçâîäèì çâóê ïðûæêà
        if (jumpSound != null)
            audioSource.PlayOneShot(jumpSound);

        // Ñîçäàåì ýôôåêò ïðûæêà
        if (jumpAttackEffect != null)
            Instantiate(jumpAttackEffect, transform.position, Quaternion.identity);

        // Äâèãàåì ïåðñîíàæà âî âðåìÿ àíèìàöèè
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / jumpDuration;

            // Èñïîëüçóåì êðèâóþ äëÿ ïëàâíîñòè
            float curvedProgress = jumpSpeedCurve.Evaluate(progress);

            // Êðèâàÿ ïðûæêà (ïàðàáîëà ñ èñïîëüçîâàíèåì êðèâîé AnimationCurve)
            float heightCurve = jumpHeightCurve.Evaluate(progress);
            float height = Mathf.Sin(heightCurve * Mathf.PI) * jumpHeight;

            // Ïîçèöèÿ ñ ó÷åòîì êðèâîé
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, curvedProgress);
            newPosition.y = startPosition.y + height;

            // Ïðèìåíÿåì ïîçèöèþ íàïðÿìóþ (Rigidbody îòêëþ÷åí)
            transform.position = newPosition;

            // Ïëàâíûé ïîâîðîò ê öåëè âî âðåìÿ ïðûæêà
            Vector3 lookDirection = (target.position - transform.position).normalized;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            }

            yield return null;
        }

        // Âêëþ÷àåì îáðàòíî ôèçèêó
        playerRigidbody.isKinematic = wasKinematic;

        // Íàíîñèì óðîí âñåì âðàãàì â ðàäèóñå ïðè ïðèçåìëåíèè
        OnJumpAttackLand();
        CharacterControl.canWalk = true;

        // Çàïóñêàåì ïåðåçàðÿäêó
        StartCoroutine(JumpAttackCooldown());
    }

    void OnJumpAttackLand()
    {
        // Íàõîäèì âñåõ âðàãîâ â ðàäèóñå àòàêè
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRadius, enemyLayer);

        int enemiesHit = 0;

        foreach (Collider enemy in hitEnemies)
        {
            // Íàíîñèì óðîí êàæäîìó âðàãó â ðàäèóñå
            EnemyAI enemyHealth = enemy.GetComponent<EnemyAI>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeEnemyDamage(attackDamage);
                enemiesHit++;
                Debug.Log($"Ïðûæîê-àòàêà! Íàíåñåíî óðîíà: {attackDamage} âðàãó {enemy.name}");
            }
        }

        if (enemiesHit > 0)
        {
            Debug.Log($"Ïðûæîê-àòàêà ïîðàçèëà {enemiesHit} âðàãîâ!");

            // Ñîçäàåì ýôôåêò ïðèçåìëåíèÿ
            if (landingEffect != null)
                Instantiate(landingEffect, transform.position, Quaternion.identity);

            // Âîñïðîèçâîäèì çâóê àòàêè
            if (attackSound != null)
                audioSource.PlayOneShot(attackSound);
        }
        else
        {
            Debug.Log("Ïðûæîê-àòàêà íå ïîïàëà íè ïî îäíîìó âðàãó!");
        }
    }

    IEnumerator JumpAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canJumpAttack = true;
        isJumpAttacking = false;
    }

   

    // Ðàñ÷åò ïàðàìåòðîâ ïðûæêà äëÿ îòëàäêè
    public void CalculateJumpParameters(float distance, out float jumpHeight, out float jumpDuration)
    {
        distance = Mathf.Clamp(distance, minJumpDistance, maxJumpDistance);
        float normalizedDistance = (distance - minJumpDistance) / (maxJumpDistance - minJumpDistance);

        jumpHeight = Mathf.Lerp(minJumpHeight, maxJumpHeight, normalizedDistance);
        jumpDuration = Mathf.Lerp(minJumpDuration, maxJumpDuration, normalizedDistance);
    }
}
