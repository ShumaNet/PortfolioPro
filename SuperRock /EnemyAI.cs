using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Îñíîâíûå íàñòðîéêè")]
    public float health = 100f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    [Header("Îáíàðóæåíèå èãðîêà")]
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Ïàòðóëèðîâàíèå")]
    public Transform[] patrolPoints;
    public float pointReachedDistance = 0.5f;

    [Header("Àòàêà")]
    public int attackDamage = 10;
    public float attackCooldown = 2f;
    public float attackDuration = 1f;

    [Header("Ìåòîä ïðîâåðêè ïîïàäàíèÿ")]
    public bool useSwordCollision = true; // Âêë/âûêë ïðîâåðêó ÷åðåç êîëëèçèþ ìå÷à
    public GameObject swordObject; // Ññûëêà íà îáúåêò ìå÷à
    public Collider swordCollider; // Êîëëàéäåð ìå÷à äëÿ îïðåäåëåíèÿ ïîïàäàíèÿ
    public float attackHitRadius = 1.5f; // Ðàäèóñ ïîïàäàíèÿ (åñëè íå èñïîëüçóåòñÿ ìå÷)

    [Header("Ýôôåêòû")]
    public GameObject deathEffect;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip attackSound;
    public AudioClip hitSound; // Çâóê ïîïàäàíèÿ ïî èãðîêó

    // Êîìïîíåíòû
    private Animator animator;
    private AudioSource audioSource;
    private Transform player;
    private CharacterControl playerHealth;

    // Ñîñòîÿíèÿ
    private enum EnemyState { Patrolling, Chasing, Attacking, Dead }
    private EnemyState currentState = EnemyState.Patrolling;

    // Ïåðåìåííûå äëÿ ëîãèêè
    private int currentPatrolIndex = 0;
    private bool canAttack = true;
    private Vector3 currentTargetPosition;
    private float originalMoveSpeed;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool hasHitPlayer = false; // Ôëàã, ÷òî óæå ïîïàëè ïî èãðîêó â ýòîé àòàêå

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<CharacterControl>();

        // Åñëè AudioSource íåò - äîáàâëÿåì
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        originalMoveSpeed = moveSpeed;

        // Íàñòðîéêà ìå÷à (åñëè èñïîëüçóåòñÿ ïðîâåðêà ÷åðåç êîëëèçèþ)
        if (useSwordCollision)
        {
            // Îòêëþ÷àåì êîëëàéäåð ìå÷à ïî óìîë÷àíèþ
            if (swordCollider != null)
            {
                swordCollider.enabled = false;
                swordCollider.isTrigger = true;
            }

            // Íàõîäèì ìå÷ àâòîìàòè÷åñêè åñëè íå óêàçàí
            if (swordObject == null)
            {
                FindSwordInChildren();
            }

            if (swordCollider == null && swordObject != null)
            {
                Debug.LogWarning("Sword collider not found! Check sword object setup.");
            }
        }

        // Íà÷èíàåì ïàòðóëèðîâàíèå
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentTargetPosition = patrolPoints[0].position;
            SetRunningAnimation(false);
        }
        else
        {
            Debug.LogWarning("No patrol points assigned to enemy!");
        }
    }

    void Update()
    {
        if (isDead) return;

        // Ïðîâåðÿåì, æèâ ëè èãðîê
        if (playerHealth != null && playerHealth.GetCurrentHealth() <= 0)
        {
            ReturnToPatrol();
            return;
        }

        // Ïðîâåðÿåì ðàññòîÿíèå äî èãðîêà
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Patrolling:
                HandlePatrolState(distanceToPlayer);
                break;
            case EnemyState.Chasing:
                HandleChaseState(distanceToPlayer);
                break;
            case EnemyState.Attacking:
                // Â ñîñòîÿíèè àòàêè íè÷åãî íå äåëàåì - æäåì çàâåðøåíèÿ àíèìàöèè
                break;
        }
    }

    void HandlePatrolState(float distanceToPlayer)
    {
        // Ïðîâåðÿåì, âèäèì ëè èãðîêà
        if (distanceToPlayer <= detectionRange && CanSeePlayer())
        {
            StartChasing();
            return;
        }

        // Ïàòðóëèðóåì áåç îñòàíîâîê
        PatrolToPoint();
    }

    void HandleChaseState(float distanceToPlayer)
    {
        // Åñëè èãðîê ìåðòâ èëè âûøåë èç ðàäèóñà îáíàðóæåíèÿ, âîçâðàùàåìñÿ ê ïàòðóëèðîâàíèþ
        if (distanceToPlayer > detectionRange || (playerHealth != null && playerHealth.GetCurrentHealth() <= 0))
        {
            ReturnToPatrol();
            return;
        }

        // Åñëè ìîæåì àòàêîâàòü - àòàêóåì
        if (distanceToPlayer <= attackRange && canAttack && !isAttacking)
        {
            AttackPlayer();
            return;
        }

        // Ïðåñëåäóåì èãðîêà
        ChasePlayer();
    }

    void PatrolToPoint()
    {
        if (patrolPoints.Length == 0) return;

        // Äâèãàåìñÿ ê òåêóùåé òî÷êå
        Vector3 direction = (currentTargetPosition - transform.position).normalized;
        direction.y = 0;

        // Äâèæåíèå
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Ïîâîðîò
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Ïðîâåðÿåì, äîñòèãëè ëè òî÷êè è ñðàçó ïåðåõîäèì ê ñëåäóþùåé
        if (Vector3.Distance(transform.position, currentTargetPosition) <= pointReachedDistance)
        {
            // Íåìåäëåííî ïåðåõîäèì ê ñëåäóþùåé òî÷êå áåç îñòàíîâêè
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            currentTargetPosition = patrolPoints[currentPatrolIndex].position;
        }

        SetRunningAnimation(true);
    }

    void ChasePlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        // Äâèæåíèå ê èãðîêó
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Ïîâîðîò ê èãðîêó
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        SetRunningAnimation(true);
    }

    void StartChasing()
    {
        currentState = EnemyState.Chasing;
        moveSpeed = originalMoveSpeed * 1.5f; // Óâåëè÷èâàåì ñêîðîñòü ïðè ïðåñëåäîâàíèè
        SetRunningAnimation(true);

        Debug.Log("Èãðîê îáíàðóæåí! Íà÷èíàþ ïðåñëåäîâàíèå.");
    }

    void ReturnToPatrol()
    {
        if (currentState == EnemyState.Patrolling) return;

        currentState = EnemyState.Patrolling;
        moveSpeed = originalMoveSpeed;

        // Âîçâðàùàåìñÿ ê áëèæàéøåé òî÷êå ïàòðóëèðîâàíèÿ
        FindNearestPatrolPoint();

        SetRunningAnimation(true);

        Debug.Log("Èãðîê ìåðòâ èëè ïîòåðÿí! Âîçâðàùàþñü ê ïàòðóëèðîâàíèþ.");
    }

    void AttackPlayer()
    {
        currentState = EnemyState.Attacking;
        isAttacking = true;
        hasHitPlayer = false;
        SetRunningAnimation(false);

        // Ïîâîðà÷èâàåìñÿ ê èãðîêó ïåðåä àòàêîé
        if (player != null)
        {
            Vector3 lookDirection = (player.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        // Çàïóñêàåì àíèìàöèþ àòàêè
        animator.Play("Attack");

        // Çâóê àòàêè
        if (attackSound != null)
            audioSource.PlayOneShot(attackSound);

        // Âêëþ÷àåì ïðîâåðêó ïîïàäàíèÿ
        if (useSwordCollision)
        {
            // Âêëþ÷àåì êîëëàéäåð ìå÷à
            StartCoroutine(ActivateSwordCollider());
        }
        else
        {
            // Èñïîëüçóåì ïðîâåðêó ïî ðàäèóñó
            StartCoroutine(CheckRadiusHit());
        }

        // Ïåðåçàðÿäêà àòàêè
        canAttack = false;
        StartCoroutine(AttackCooldown());

        // Âîçâðàùàåìñÿ ê ïðåñëåäîâàíèþ ïîñëå àòàêè
        StartCoroutine(ReturnToChaseAfterAttack());

        Debug.Log("Àòàêóþ èãðîêà!");
    }

    // Âêëþ÷àåò êîëëàéäåð ìå÷à â íóæíûé ìîìåíò àíèìàöèè
    IEnumerator ActivateSwordCollider()
    {
        // Çàäåðæêà ïåðåä âêëþ÷åíèåì êîëëàéäåðà (êîãäà ìå÷ íà÷èíàåò äâèãàòüñÿ)
        yield return new WaitForSeconds(0.3f);

        if (swordCollider != null)
        {
            swordCollider.enabled = true;
        }

        // Îòêëþ÷àåì êîëëàéäåð ÷åðåç âðåìÿ
        yield return new WaitForSeconds(0.4f);

        if (swordCollider != null)
        {
            swordCollider.enabled = false;
        }
    }

    // Ïðîâåðêà ïîïàäàíèÿ ïî ðàäèóñó (åñëè íå èñïîëüçóåòñÿ ìå÷)
    IEnumerator CheckRadiusHit()
    {
        // Çàäåðæêà ïåðåä ïðîâåðêîé (êîãäà àòàêà äîñòèãàåò ïèêà)
        yield return new WaitForSeconds(0.4f);

        if (isDead||!isAttacking||hasHitPlayer)
            yield break;

        // Ïðîâåðÿåì, íàõîäèòñÿ ëè èãðîê â ðàäèóñå àòàêè
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackHitRadius)
        {
            DealDamageToPlayer();
        }
    }

    // Íàíåñåíèå óðîíà èãðîêó
    void DealDamageToPlayer()
    {
        if (playerHealth != null && playerHealth.GetCurrentHealth() > 0)
        {
            playerHealth.TakeDamage(attackDamage);
            hasHitPlayer = true;

            // Çâóê ïîïàäàíèÿ
            if (hitSound != null)
                audioSource.PlayOneShot(hitSound);

            Debug.Log($"Ïîïàë ïî èãðîêó! Íàíåñåíî óðîíà: {attackDamage}");
        }
    }

    // Îáðàáîò÷èê ïîïàäàíèÿ ìå÷à (âûçûâàåòñÿ èç ñîáûòèÿ àíèìàöèè èëè èç OnTriggerEnter)
    public void OnSwordHit(Collider other)
    {
        if (!useSwordCollision || isDead || !isAttacking || hasHitPlayer) return;

        // Ïðîâåðÿåì, ïîïàë ëè ìå÷ â èãðîêà
        if (other.CompareTag("Player"))
        {
            DealDamageToPlayer();
        }
    }

    // Àâòîìàòè÷åñêèé ïîèñê ìå÷à â äî÷åðíèõ îáúåêòàõ
    void FindSwordInChildren()
    {
        // Èùåì îáúåêò ñ òåãîì Sword èëè ñîäåðæàùèé sword â èìåíè
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag("Sword") || child.name.ToLower().Contains("sword"))
            {
                swordObject = child.gameObject;
                swordCollider = child.GetComponent<Collider>();

                if (swordCollider == null)
                {
                    swordCollider = child.gameObject.AddComponent<BoxCollider>();
                    swordCollider.isTrigger = true;
                }

                Debug.Log($"Íàéäåí ìå÷: {child.name}");
                break;
            }
        }

        if (swordObject == null)
        {
            Debug.LogWarning("Ìå÷ íå íàéäåí! Äîáàâüòå ìå÷ êàê äî÷åðíèé îáúåêò èëè óêàæèòå âðó÷íóþ.");
        }
    }

    // Ìåòîä äëÿ âûçîâà èç ñîáûòèÿ àíèìàöèè (Animation Event)
    public void AnimationEvent_AttackStart()
    {
        if (!useSwordCollision) return;

        // Ìîæíî èñïîëüçîâàòü äëÿ âêëþ÷åíèÿ êîëëàéäåðà â íóæíûé ìîìåíò
        if (swordCollider != null)
        {
            swordCollider.enabled = true;
        }
    }

    // Ìåòîä äëÿ âûçîâà èç ñîáûòèÿ àíèìàöèè (Animation Event)
    public void AnimationEvent_AttackEnd()
    {
        if (!useSwordCollision) return;

        // Îòêëþ÷àåì êîëëàéäåð ìå÷à
        if (swordCollider != null)
        {
            swordCollider.enabled = false;
        }
        hasHitPlayer = false;
    }

    // Ìåòîä äëÿ âûçîâà èç ñîáûòèÿ àíèìàöèè (Animation Event) - äëÿ ïðîâåðêè ïî ðàäèóñó
    public void AnimationEvent_CheckHit()
    {
        if (useSwordCollision) return;

        // Ïðîâåðÿåì ïîïàäàíèå â ìîìåíò àíèìàöèè
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackHitRadius)
        {
            DealDamageToPlayer();
        }
    }

    // Îáðàáîòêà òðèããåðà ìå÷à (åñëè èñïîëüçóåòñÿ òðèããåð)
    void OnTriggerEnter(Collider other)
    {
        if (!useSwordCollision || !isAttacking || hasHitPlayer) return;

        OnSwordHit(other);
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Ïðîâåðÿåì, íåò ëè ïðåïÿòñòâèé ìåæäó âðàãîì è èãðîêîì
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, detectionRange, ~obstacleLayer))
        {
            return hit.collider.CompareTag("Player");
        }

        return false;
    }

    void FindNearestPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        float nearestDistance = Mathf.Infinity;
        int nearestIndex = 0;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        currentPatrolIndex = nearestIndex;
        currentTargetPosition = patrolPoints[currentPatrolIndex].position;
    }

    void SetRunningAnimation(bool isRunning)
    {
        if (animator != null)
        {
            animator.SetFloat("Blend", isRunning ? 1f : 0f);
        }
    }

    // Ìåòîä äëÿ ïîëó÷åíèÿ óðîíà îò ïðûæêîâîé àòàêè
    public void TakeEnemyDamage(float damage)
    {
        if (isDead) return;

        health -= damage;

        // Âèçóàëüíàÿ îáðàòíàÿ ñâÿçü
        StartCoroutine(DamageFlash());

        // Çâóê ïîëó÷åíèÿ óðîíà
        if (hurtSound != null)
            audioSource.PlayOneShot(hurtSound);

        // Åñëè ïîëó÷èëè óðîí - íà÷èíàåì ïðåñëåäîâàíèå
        if (currentState == EnemyState.Patrolling)
        {
            StartChasing();
        }

        // Ïðîâåðÿåì ñìåðòü
        if (health <= 0)
        {
            Die();
        }

        Debug.Log($"Âðàã ïîëó÷èë {damage} óðîíà. Îñòàëîñü çäîðîâüÿ: {health}");
    }

    void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        isAttacking = false;

        // Îòêëþ÷àåì êîëëàéäåð ìå÷à
        if (swordCollider != null && useSwordCollision)
            swordCollider.enabled = false;

        // Àíèìàöèÿ ñìåðòè
        animator.Play("DieEnemy");

        // Çâóê ñìåðòè
        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);

        // Ýôôåêò ñìåðòè
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Îòêëþ÷àåì êîëëàéäåð âðàãà
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        Debug.Log("Âðàã óìåð!");

        // Óíè÷òîæàåì îáúåêò ÷åðåç íåêîòîðîå âðåìÿ
        StartCoroutine(DestroyAfterDeath());
    }

    // Êîðóòèíû
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    IEnumerator ReturnToChaseAfterAttack()
    {
        yield return new WaitForSeconds(attackDuration);

        if (!isDead && currentState != EnemyState.Dead)
        {
            isAttacking = false;

            // Ïðîâåðÿåì, æèâ ëè èãðîê ïåðåä âîçâðàùåíèåì ê ïðåñëåäîâàíèþ
            if (playerHealth != null && playerHealth.GetCurrentHealth() > 0)
            {
                currentState = EnemyState.Chasing;
                SetRunningAnimation(true);
            }
            else
            {
                ReturnToPatrol();
            }
        }
    }

    IEnumerator DamageFlash()
    {
        // Ïðîñòàÿ âèçóàëüíàÿ îáðàòíàÿ ñâÿçü
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = originalColor;
        }
    }

    IEnumerator DestroyAfterDeath()
    {
        // Æäåì çàâåðøåíèÿ àíèìàöèè ñìåðòè
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }


    // ========== ÌÅÒÎÄÛ ÄËß ÑÎÕÐÀÍÅÍÈß ==========

    // Ïîëó÷èòü òåêóùåå ñîñòîÿíèå êàê ñòðîêó
    public string GetStateForSave()
    {
        return currentState.ToString();
    }

    // Óñòàíîâèòü ñîñòîÿíèå èç ñòðîêè
    public void SetStateFromSave(string state)
    {
        try
        {
            currentState = (EnemyState)System.Enum.Parse(typeof(EnemyState), state);
        }
        catch
        {
            Debug.LogWarning($"Íå óäàëîñü ðàñïîçíàòü ñîñòîÿíèå: {state}");
            currentState = EnemyState.Patrolling;
        }
    }

    // Ïîëó÷èòü çäîðîâüå
    public float GetHealthForSave()
    {
        return health;
    }

    // Óñòàíîâèòü çäîðîâüå
    public void SetHealthFromSave(float newHealth)
    {
        health = newHealth;

        if (health <= 0)
        {
            // Åñëè çäîðîâüå 0 èëè ìåíüøå, âðàã äîëæåí áûòü ìåðòâ
            Die();
        }
    }

    // Óñòàíîâèòü ïîçèöèþ (äëÿ çàãðóçêè)
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    // Óñòàíîâèòü âðàùåíèå (äëÿ çàãðóçêè)
    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    // Ïîëó÷èòü èíäåêñ òåêóùåé òî÷êè ïàòðóëèðîâàíèÿ
    public int GetCurrentPatrolIndex()
    {
        return currentPatrolIndex;
    }

    // Óñòàíîâèòü èíäåêñ òî÷êè ïàòðóëèðîâàíèÿ
    public void SetCurrentPatrolIndex(int index)
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            currentPatrolIndex = Mathf.Clamp(index, 0, patrolPoints.Length - 1);
            if (patrolPoints[currentPatrolIndex] != null)
            {
                currentTargetPosition = patrolPoints[currentPatrolIndex].position;
            }
        }
    }

    // Ïîëó÷èòü öåëåâóþ ïîçèöèþ
    public Vector3 GetTargetPosition()
    {
        return currentTargetPosition;
    }

    // Óñòàíîâèòü öåëåâóþ ïîçèöèþ
    public void SetTargetPosition(Vector3 position)
    {
        currentTargetPosition = position;
    }

    // Îòêëþ÷èòü âðàãà (ïðè çàãðóçêå ìåðòâîãî âðàãà)
    public void DisableEnemy()
    {
        isDead = true;
        currentState = EnemyState.Dead;

        if (animator != null)
        {
            animator.Play("DieEnemy");
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        // Îòêëþ÷àåì ìå÷ åñëè èñïîëüçóåòñÿ
        if (useSwordCollision && swordCollider != null)
            swordCollider.enabled = false;

        gameObject.SetActive(false);
    }


    // Âèçóàëèçàöèÿ â ðåäàêòîðå
    void OnDrawGizmosSelected()
    {
        // Ðàäèóñ îáíàðóæåíèÿ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Ðàäèóñ àòàêè
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Ðàäèóñ ïîïàäàíèÿ (åñëè íå èñïîëüçóåòñÿ ìå÷)
        if (!useSwordCollision)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, attackHitRadius);
        }

        // Òî÷êè ïàòðóëèðîâàíèÿ
        if (patrolPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.3f);
                    Gizmos.DrawLine(transform.position, point.position);
                }
            }
        }

        // Ëèíèÿ ê èãðîêó åñëè âèäèì
        if (player != null && currentState == EnemyState.Chasing)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // Ïîêàçûâàåì ìå÷ åñëè åñòü è èñïîëüçóåòñÿ
        if (useSwordCollision && swordObject != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(swordObject.transform.position, 0.2f);
        }
    }
}
