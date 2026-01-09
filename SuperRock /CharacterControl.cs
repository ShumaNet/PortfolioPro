using UnityEngine;
using System.Collections;

public class CharacterControl : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float rotationSpeed = 15f;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] float airControl = 0.5f;
    [SerializeField] Transform startPoint;

    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    public static bool canWalk = true;

    [Header("References")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] Animator animator;
    [SerializeField] LayerMask groundLayer = 1;

    // Компоненты
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;

    // Переменные движения
    private bool isGrounded;
    private Vector3 movementInput;
    private bool isJumping = false;
    private bool isDead = false;

    // Ссылка на GameMenuManager
    private GameMenuManager uiManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        if (capsuleCollider == null)
        {
            capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.height = 2f;
            capsuleCollider.center = Vector3.up * 1f;
            capsuleCollider.radius = 0.3f;
        }

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 1f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.useGravity = true;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        if (animator == null)
            animator = GetComponent<Animator>();

        animator.SetFloat("Blend", 0);

        // Находим GameMenuManager
        uiManager = FindObjectOfType<GameMenuManager>();

        // Загрузка сохранения
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            GameData data = SaveManager.Instance.GetGameData();
            if (!data.isNewGame)
            {
                transform.position = data.playerPosition;
                currentHealth = data.playerHealth;

                // Восстанавливаем деньги
                PlayerPrefs.SetInt("Money", data.playerMoney);
                PlayerPrefs.Save();

                // Восстанавливаем состояние игры
                SaveManager.Instance.RestoreGameState();
            }
            else
            {
                currentHealth = maxHealth;
            }
        }
        else
        {
            currentHealth = maxHealth;

            if (startPoint != null)
            {
                transform.position = startPoint.position;
                transform.rotation = startPoint.rotation;
            }
        }

        // Обновляем UI здоровья после загрузки
        UpdateHealthUI();
    }

    void Update()
    {
        if (!canWalk || isDead) return;

        CheckGrounded();

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = CalculateMovementDirection(moveX, moveZ);
        movementInput = moveDirection;

        HandleAnimations(moveX, moveZ, moveDirection);

        if (isGrounded && Input.GetButtonDown("Jump") && !isJumping)
        {
            Jump();
        }

        if (moveDirection != Vector3.zero && canWalk)
        {
            RotateTowardsMovement(moveDirection);
        }
    }

    void FixedUpdate()
    {
        if (!canWalk || isDead) return;

        ApplyMovement();
        LimitHorizontalVelocity();
    }

    void ApplyMovement()
    {
        if (movementInput.magnitude > 0.1f)
        {
            Vector3 targetVelocity = movementInput * moveSpeed;

            if (!isGrounded)
            {
                targetVelocity *= airControl;
            }

            Vector3 velocityDifference = targetVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(velocityDifference * rb.mass * 10f, ForceMode.Force);
        }
        else if (isGrounded)
        {
            Vector3 brakeForce = -new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z) * rb.mass * 10f;
            rb.AddForce(brakeForce, ForceMode.Force);
        }
    }

    void LimitHorizontalVelocity()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > moveSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }
    }

    void CheckGrounded()
    {
        float rayLength = capsuleCollider.height * 0.5f + groundCheckDistance;
        Vector3 rayStart = transform.position + Vector3.up * capsuleCollider.radius;

        bool hit1 = Physics.Raycast(rayStart, Vector3.down, rayLength, groundLayer);
        bool hit2 = Physics.Raycast(rayStart + Vector3.right * capsuleCollider.radius * 0.5f, Vector3.down, rayLength, groundLayer);
        bool hit3 = Physics.Raycast(rayStart + Vector3.left * capsuleCollider.radius * 0.5f, Vector3.down, rayLength, groundLayer);
        bool hit4 = Physics.Raycast(rayStart + Vector3.forward * capsuleCollider.radius * 0.5f, Vector3.down, rayLength, groundLayer);
        bool hit5 = Physics.Raycast(rayStart + Vector3.back * capsuleCollider.radius * 0.5f, Vector3.down, rayLength, groundLayer);

        isGrounded = hit1 || hit2 || hit3 || hit4 || hit5;

        if (isGrounded && isJumping && rb.linearVelocity.y <= 0)
        {
            isJumping = false;
        }
    }

    void Jump()
    {
        isJumping = true;

        if (animator != null)
            animator.Play("Jump");

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    void HandleAnimations(float moveX, float moveZ, Vector3 moveDirection)
    {
        if (moveDirection != Vector3.zero)
        {
            float moveXZ = Mathf.Clamp01(Mathf.Abs(moveX) + Mathf.Abs(moveZ));
            animator.SetFloat("Blend", moveXZ);
        }
        else
        {
            animator.SetFloat("Blend", 0);
        }
    }

    Vector3 CalculateMovementDirection(float horizontal, float vertical)
    {
        if (cameraTransform == null) return Vector3.zero;

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 direction = (cameraForward * vertical) + (cameraRight * horizontal);

        if (direction.magnitude > 1f)
            direction.Normalize();

        return direction;
    }

    void RotateTowardsMovement(Vector3 moveDirection)
    {
        Vector3 horizontalDirection = new Vector3(moveDirection.x, 0, moveDirection.z);

        if (horizontalDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        UpdateHealthUI();

        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageFlash());
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        canWalk = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (animator != null)
            animator.Play("Die");

        if (uiManager != null)
        {
            uiManager.ShowDeathScreen();
        }

        Debug.Log("Игрок умер!");
    }

    IEnumerator DamageFlash()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.material.color = originalColor;
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
    }

    // Метод для обновления UI здоровья
    private void UpdateHealthUI()
    {
        if (uiManager != null)
        {
            // Если в GameMenuManager есть метод для обновления здоровья, вызываем его
            // Или напрямую обновляем текстовое поле, если оно публичное
            if (uiManager.healthText != null)
            {
                uiManager.healthText.text = "HP: " + currentHealth;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HealthPack"))
        {
            HealthPack healthPack = other.GetComponent<HealthPack>();
            if (healthPack != null)
            {
                healthPack.ForceCollect(this);
            }
        }
    }
}
