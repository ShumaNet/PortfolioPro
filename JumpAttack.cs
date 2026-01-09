using UnityEngine;
using System.Collections;

public class JumpAttack : MonoBehaviour
{
    [Header("Настройки прыжка-атаки")]
    public float maxJumpDistance = 15f;           // Максимальная дистанция прыжка
    public float minJumpDistance = 3f;            // Минимальная дистанция прыжка
    public float maxJumpHeight = 5f;              // Максимальная высота прыжка
    public float minJumpHeight = 1.5f;            // Минимальная высота прыжка
    public float maxJumpDuration = 1.5f;          // Максимальная длительность прыжка
    public float minJumpDuration = 0.5f;          // Минимальная длительность прыжка
    public int attackDamage = 15;                 // Урон при прыжке-атаке
    public float attackCooldown = 2f;             // Время перезарядки
    public float attackRadius = 5f;               // Радиус поражения при приземлении
    public AnimationCurve jumpHeightCurve = AnimationCurve.Linear(0, 0, 1, 1); // Кривая высоты прыжка
    public AnimationCurve jumpSpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);  // Кривая скорости прыжка

    [Header("Настройки обнаружения")]
    public LayerMask enemyLayer;                  // Слой врагов
    public LayerMask obstacleLayer;               // Слой препятствий

    [Header("Визуальные эффекты")]
    public GameObject jumpChargeEffect;           // Эффект при заряде прыжка
    public GameObject jumpAttackEffect;           // Эффект при прыжке-атаке
    public GameObject landingEffect;              // Эффект при приземлении
    public AudioClip jumpChargeSound;             // Звук заряда прыжка
    public AudioClip jumpSound;                   // Звук прыжка
    public AudioClip attackSound;                 // Звук атаки

    private bool isJumpAttacking = false;         // Флаг прыжка-атаки
    private bool canJumpAttack = true;            // Может ли выполнить прыжок-атаку
    private float lastJumpAttackTime;             // Время последней прыжк-атаки

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
            Debug.Log("Нет подходящих целей для прыжка-атаки!");
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

            // Пропускаем слишком близких врагов
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

        // Расчет расстояния до цели
        float distance = Vector3.Distance(startPosition, targetPosition);
        distance = Mathf.Clamp(distance, minJumpDistance, maxJumpDistance);

        // Расчет параметров прыжка в зависимости от расстояния
        float normalizedDistance = (distance - minJumpDistance) / (maxJumpDistance - minJumpDistance);
        float jumpHeight = Mathf.Lerp(minJumpHeight, maxJumpHeight, normalizedDistance);
        float jumpDuration = Mathf.Lerp(minJumpDuration, maxJumpDuration, normalizedDistance);

        // Позиция приземления (немного перед врагом)
        Vector3 directionToTarget = (targetPosition - startPosition).normalized;
        float landingOffset = Mathf.Lerp(1f, 2f, normalizedDistance); // Отступ увеличивается с расстоянием
        targetPosition = targetPosition - directionToTarget * landingOffset;
        targetPosition.y = startPosition.y;

        // Эффект заряда прыжка
        if (jumpChargeEffect != null)
            Instantiate(jumpChargeEffect, transform.position, Quaternion.identity);

        if (jumpChargeSound != null)
            audioSource.PlayOneShot(jumpChargeSound);

        yield return new WaitForSeconds(0.2f); // Короткая задержка для заряда

        // Отключаем физику Rigidbody на время прыжка
        bool wasKinematic = playerRigidbody.isKinematic;
        playerRigidbody.isKinematic = true;

        // Запускаем анимацию прыжка-атаки
        if (animator != null)
            animator.Play("Atack");

        // Воспроизводим звук прыжка
        if (jumpSound != null)
            audioSource.PlayOneShot(jumpSound);

        // Создаем эффект прыжка
        if (jumpAttackEffect != null)
            Instantiate(jumpAttackEffect, transform.position, Quaternion.identity);

        // Двигаем персонажа во время анимации
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / jumpDuration;

            // Используем кривую для плавности
            float curvedProgress = jumpSpeedCurve.Evaluate(progress);

            // Кривая прыжка (парабола с использованием кривой AnimationCurve)
            float heightCurve = jumpHeightCurve.Evaluate(progress);
            float height = Mathf.Sin(heightCurve * Mathf.PI) * jumpHeight;

            // Позиция с учетом кривой
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, curvedProgress);
            newPosition.y = startPosition.y + height;

            // Применяем позицию напрямую (Rigidbody отключен)
            transform.position = newPosition;

            // Плавный поворот к цели во время прыжка
            Vector3 lookDirection = (target.position - transform.position).normalized;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            }

            yield return null;
        }

        // Включаем обратно физику
        playerRigidbody.isKinematic = wasKinematic;

        // Наносим урон всем врагам в радиусе при приземлении
        OnJumpAttackLand();
        CharacterControl.canWalk = true;

        // Запускаем перезарядку
        StartCoroutine(JumpAttackCooldown());
    }

    void OnJumpAttackLand()
    {
        // Находим всех врагов в радиусе атаки
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRadius, enemyLayer);

        int enemiesHit = 0;

        foreach (Collider enemy in hitEnemies)
        {
            // Наносим урон каждому врагу в радиусе
            EnemyAI enemyHealth = enemy.GetComponent<EnemyAI>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeEnemyDamage(attackDamage);
                enemiesHit++;
                Debug.Log($"Прыжок-атака! Нанесено урона: {attackDamage} врагу {enemy.name}");
            }
        }

        if (enemiesHit > 0)
        {
            Debug.Log($"Прыжок-атака поразила {enemiesHit} врагов!");

            // Создаем эффект приземления
            if (landingEffect != null)
                Instantiate(landingEffect, transform.position, Quaternion.identity);

            // Воспроизводим звук атаки
            if (attackSound != null)
                audioSource.PlayOneShot(attackSound);
        }
        else
        {
            Debug.Log("Прыжок-атака не попала ни по одному врагу!");
        }
    }

    IEnumerator JumpAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canJumpAttack = true;
        isJumpAttacking = false;
    }

   

    // Расчет параметров прыжка для отладки
    public void CalculateJumpParameters(float distance, out float jumpHeight, out float jumpDuration)
    {
        distance = Mathf.Clamp(distance, minJumpDistance, maxJumpDistance);
        float normalizedDistance = (distance - minJumpDistance) / (maxJumpDistance - minJumpDistance);

        jumpHeight = Mathf.Lerp(minJumpHeight, maxJumpHeight, normalizedDistance);
        jumpDuration = Mathf.Lerp(minJumpDuration, maxJumpDuration, normalizedDistance);
    }
}