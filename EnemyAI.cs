using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Основные настройки")]
    public float health = 100f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    [Header("Обнаружение игрока")]
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Патрулирование")]
    public Transform[] patrolPoints;
    public float pointReachedDistance = 0.5f;

    [Header("Атака")]
    public int attackDamage = 10;
    public float attackCooldown = 2f;
    public float attackDuration = 1f;

    [Header("Метод проверки попадания")]
    public bool useSwordCollision = true; // Вкл/выкл проверку через коллизию меча
    public GameObject swordObject; // Ссылка на объект меча
    public Collider swordCollider; // Коллайдер меча для определения попадания
    public float attackHitRadius = 1.5f; // Радиус попадания (если не используется меч)

    [Header("Эффекты")]
    public GameObject deathEffect;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip attackSound;
    public AudioClip hitSound; // Звук попадания по игроку

    // Компоненты
    private Animator animator;
    private AudioSource audioSource;
    private Transform player;
    private CharacterControl playerHealth;

    // Состояния
    private enum EnemyState { Patrolling, Chasing, Attacking, Dead }
    private EnemyState currentState = EnemyState.Patrolling;

    // Переменные для логики
    private int currentPatrolIndex = 0;
    private bool canAttack = true;
    private Vector3 currentTargetPosition;
    private float originalMoveSpeed;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool hasHitPlayer = false; // Флаг, что уже попали по игроку в этой атаке

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<CharacterControl>();

        // Если AudioSource нет - добавляем
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        originalMoveSpeed = moveSpeed;

        // Настройка меча (если используется проверка через коллизию)
        if (useSwordCollision)
        {
            // Отключаем коллайдер меча по умолчанию
            if (swordCollider != null)
            {
                swordCollider.enabled = false;
                swordCollider.isTrigger = true;
            }

            // Находим меч автоматически если не указан
            if (swordObject == null)
            {
                FindSwordInChildren();
            }

            if (swordCollider == null && swordObject != null)
            {
                Debug.LogWarning("Sword collider not found! Check sword object setup.");
            }
        }

        // Начинаем патрулирование
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

        // Проверяем, жив ли игрок
        if (playerHealth != null && playerHealth.GetCurrentHealth() <= 0)
        {
            ReturnToPatrol();
            return;
        }

        // Проверяем расстояние до игрока
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
                // В состоянии атаки ничего не делаем - ждем завершения анимации
                break;
        }
    }

    void HandlePatrolState(float distanceToPlayer)
    {
        // Проверяем, видим ли игрока
        if (distanceToPlayer <= detectionRange && CanSeePlayer())
        {
            StartChasing();
            return;
        }

        // Патрулируем без остановок
        PatrolToPoint();
    }

    void HandleChaseState(float distanceToPlayer)
    {
        // Если игрок мертв или вышел из радиуса обнаружения, возвращаемся к патрулированию
        if (distanceToPlayer > detectionRange || (playerHealth != null && playerHealth.GetCurrentHealth() <= 0))
        {
            ReturnToPatrol();
            return;
        }

        // Если можем атаковать - атакуем
        if (distanceToPlayer <= attackRange && canAttack && !isAttacking)
        {
            AttackPlayer();
            return;
        }

        // Преследуем игрока
        ChasePlayer();
    }

    void PatrolToPoint()
    {
        if (patrolPoints.Length == 0) return;

        // Двигаемся к текущей точке
        Vector3 direction = (currentTargetPosition - transform.position).normalized;
        direction.y = 0;

        // Движение
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Поворот
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Проверяем, достигли ли точки и сразу переходим к следующей
        if (Vector3.Distance(transform.position, currentTargetPosition) <= pointReachedDistance)
        {
            // Немедленно переходим к следующей точке без остановки
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

        // Движение к игроку
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Поворот к игроку
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
        moveSpeed = originalMoveSpeed * 1.5f; // Увеличиваем скорость при преследовании
        SetRunningAnimation(true);

        Debug.Log("Игрок обнаружен! Начинаю преследование.");
    }

    void ReturnToPatrol()
    {
        if (currentState == EnemyState.Patrolling) return;

        currentState = EnemyState.Patrolling;
        moveSpeed = originalMoveSpeed;

        // Возвращаемся к ближайшей точке патрулирования
        FindNearestPatrolPoint();

        SetRunningAnimation(true);

        Debug.Log("Игрок мертв или потерян! Возвращаюсь к патрулированию.");
    }

    void AttackPlayer()
    {
        currentState = EnemyState.Attacking;
        isAttacking = true;
        hasHitPlayer = false;
        SetRunningAnimation(false);

        // Поворачиваемся к игроку перед атакой
        if (player != null)
        {
            Vector3 lookDirection = (player.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        // Запускаем анимацию атаки
        animator.Play("Attack");

        // Звук атаки
        if (attackSound != null)
            audioSource.PlayOneShot(attackSound);

        // Включаем проверку попадания
        if (useSwordCollision)
        {
            // Включаем коллайдер меча
            StartCoroutine(ActivateSwordCollider());
        }
        else
        {
            // Используем проверку по радиусу
            StartCoroutine(CheckRadiusHit());
        }

        // Перезарядка атаки
        canAttack = false;
        StartCoroutine(AttackCooldown());

        // Возвращаемся к преследованию после атаки
        StartCoroutine(ReturnToChaseAfterAttack());

        Debug.Log("Атакую игрока!");
    }

    // Включает коллайдер меча в нужный момент анимации
    IEnumerator ActivateSwordCollider()
    {
        // Задержка перед включением коллайдера (когда меч начинает двигаться)
        yield return new WaitForSeconds(0.3f);

        if (swordCollider != null)
        {
            swordCollider.enabled = true;
        }

        // Отключаем коллайдер через время
        yield return new WaitForSeconds(0.4f);

        if (swordCollider != null)
        {
            swordCollider.enabled = false;
        }
    }

    // Проверка попадания по радиусу (если не используется меч)
    IEnumerator CheckRadiusHit()
    {
        // Задержка перед проверкой (когда атака достигает пика)
        yield return new WaitForSeconds(0.4f);

        if (isDead||!isAttacking||hasHitPlayer)
            yield break;

        // Проверяем, находится ли игрок в радиусе атаки
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackHitRadius)
        {
            DealDamageToPlayer();
        }
    }

    // Нанесение урона игроку
    void DealDamageToPlayer()
    {
        if (playerHealth != null && playerHealth.GetCurrentHealth() > 0)
        {
            playerHealth.TakeDamage(attackDamage);
            hasHitPlayer = true;

            // Звук попадания
            if (hitSound != null)
                audioSource.PlayOneShot(hitSound);

            Debug.Log($"Попал по игроку! Нанесено урона: {attackDamage}");
        }
    }

    // Обработчик попадания меча (вызывается из события анимации или из OnTriggerEnter)
    public void OnSwordHit(Collider other)
    {
        if (!useSwordCollision || isDead || !isAttacking || hasHitPlayer) return;

        // Проверяем, попал ли меч в игрока
        if (other.CompareTag("Player"))
        {
            DealDamageToPlayer();
        }
    }

    // Автоматический поиск меча в дочерних объектах
    void FindSwordInChildren()
    {
        // Ищем объект с тегом Sword или содержащий sword в имени
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

                Debug.Log($"Найден меч: {child.name}");
                break;
            }
        }

        if (swordObject == null)
        {
            Debug.LogWarning("Меч не найден! Добавьте меч как дочерний объект или укажите вручную.");
        }
    }

    // Метод для вызова из события анимации (Animation Event)
    public void AnimationEvent_AttackStart()
    {
        if (!useSwordCollision) return;

        // Можно использовать для включения коллайдера в нужный момент
        if (swordCollider != null)
        {
            swordCollider.enabled = true;
        }
    }

    // Метод для вызова из события анимации (Animation Event)
    public void AnimationEvent_AttackEnd()
    {
        if (!useSwordCollision) return;

        // Отключаем коллайдер меча
        if (swordCollider != null)
        {
            swordCollider.enabled = false;
        }
        hasHitPlayer = false;
    }

    // Метод для вызова из события анимации (Animation Event) - для проверки по радиусу
    public void AnimationEvent_CheckHit()
    {
        if (useSwordCollision) return;

        // Проверяем попадание в момент анимации
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackHitRadius)
        {
            DealDamageToPlayer();
        }
    }

    // Обработка триггера меча (если используется триггер)
    void OnTriggerEnter(Collider other)
    {
        if (!useSwordCollision || !isAttacking || hasHitPlayer) return;

        OnSwordHit(other);
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // Проверяем, нет ли препятствий между врагом и игроком
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

    // Метод для получения урона от прыжковой атаки
    public void TakeEnemyDamage(float damage)
    {
        if (isDead) return;

        health -= damage;

        // Визуальная обратная связь
        StartCoroutine(DamageFlash());

        // Звук получения урона
        if (hurtSound != null)
            audioSource.PlayOneShot(hurtSound);

        // Если получили урон - начинаем преследование
        if (currentState == EnemyState.Patrolling)
        {
            StartChasing();
        }

        // Проверяем смерть
        if (health <= 0)
        {
            Die();
        }

        Debug.Log($"Враг получил {damage} урона. Осталось здоровья: {health}");
    }

    void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        isAttacking = false;

        // Отключаем коллайдер меча
        if (swordCollider != null && useSwordCollision)
            swordCollider.enabled = false;

        // Анимация смерти
        animator.Play("DieEnemy");

        // Звук смерти
        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);

        // Эффект смерти
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Отключаем коллайдер врага
        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        Debug.Log("Враг умер!");

        // Уничтожаем объект через некоторое время
        StartCoroutine(DestroyAfterDeath());
    }

    // Корутины
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

            // Проверяем, жив ли игрок перед возвращением к преследованию
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
        // Простая визуальная обратная связь
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
        // Ждем завершения анимации смерти
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }


    // ========== МЕТОДЫ ДЛЯ СОХРАНЕНИЯ ==========

    // Получить текущее состояние как строку
    public string GetStateForSave()
    {
        return currentState.ToString();
    }

    // Установить состояние из строки
    public void SetStateFromSave(string state)
    {
        try
        {
            currentState = (EnemyState)System.Enum.Parse(typeof(EnemyState), state);
        }
        catch
        {
            Debug.LogWarning($"Не удалось распознать состояние: {state}");
            currentState = EnemyState.Patrolling;
        }
    }

    // Получить здоровье
    public float GetHealthForSave()
    {
        return health;
    }

    // Установить здоровье
    public void SetHealthFromSave(float newHealth)
    {
        health = newHealth;

        if (health <= 0)
        {
            // Если здоровье 0 или меньше, враг должен быть мертв
            Die();
        }
    }

    // Установить позицию (для загрузки)
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    // Установить вращение (для загрузки)
    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    // Получить индекс текущей точки патрулирования
    public int GetCurrentPatrolIndex()
    {
        return currentPatrolIndex;
    }

    // Установить индекс точки патрулирования
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

    // Получить целевую позицию
    public Vector3 GetTargetPosition()
    {
        return currentTargetPosition;
    }

    // Установить целевую позицию
    public void SetTargetPosition(Vector3 position)
    {
        currentTargetPosition = position;
    }

    // Отключить врага (при загрузке мертвого врага)
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

        // Отключаем меч если используется
        if (useSwordCollision && swordCollider != null)
            swordCollider.enabled = false;

        gameObject.SetActive(false);
    }


    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        // Радиус обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Радиус попадания (если не используется меч)
        if (!useSwordCollision)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, attackHitRadius);
        }

        // Точки патрулирования
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

        // Линия к игроку если видим
        if (player != null && currentState == EnemyState.Chasing)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // Показываем меч если есть и используется
        if (useSwordCollision && swordObject != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(swordObject.transform.position, 0.2f);
        }
    }
}