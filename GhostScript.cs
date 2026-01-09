using UnityEngine;

public class GhostScript : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Visuals")]
    [SerializeField] private SkinnedMeshRenderer[] meshRenderers;

    private Animator anim;
    private BoxCollider boxCollider;
    private Rigidbody rb;
    private bool isDead = false;
    private float dissolveValue = 1f;
    private Vector3 respawnPoint = Vector3.zero;

    // Cash
    int cash = 0;
    public int Cash => cash;

    void Start()
    {
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        respawnPoint = transform.position;
    }

    void Update()
    {
        if (isDead)
        {
            HandleRespawn();
            return;
        }

        Move();
    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(horizontal, 0, vertical);

        if (input.magnitude > 0.1f)
        {
            // ������������ � �������
            transform.rotation = Quaternion.LookRotation(input);
            Vector3 move = transform.forward * moveSpeed * Time.deltaTime;

            // ���������� Rigidbody ��� ��������
            rb.MovePosition(transform.position + move);

            // �������� ������
            anim.CrossFade("Base Layer.move", 0.1f);
        }
        else
        {
            // �������� �������
            anim.CrossFade("Base Layer.idle", 0.1f);
        }
    }

    void HandleRespawn()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Respawn();
        }
    }

    public void TakeLavaDamage()
    {
        if (isDead) return;

        Die();
    }

    void Die()
    {
        isDead = true;
        anim.CrossFade("Base Layer.dissolve", 0.1f);

        // ������ �����������
        dissolveValue = 0f;
        UpdateDissolveShader();

        // ��������� ��������� � ������
        if (boxCollider) boxCollider.enabled = false;
        if (rb) rb.isKinematic = true;
    }

    void Respawn()
    {
        isDead = false;

        // �������� ��������� � ������
        if (boxCollider) boxCollider.enabled = true;
        if (rb)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }

        // ������������� � ����� ��������
        transform.position = respawnPoint;

        // ���������� �����������
        dissolveValue = 1f;
        UpdateDissolveShader();

        // �������� �������
        anim.CrossFade("Base Layer.idle", 0.1f);
    }

    void UpdateDissolveShader()
    {
        if (meshRenderers == null) return;

        foreach (var renderer in meshRenderers)
        {
            if (renderer != null && renderer.material != null)
            {
                renderer.material.SetFloat("_Dissolve", dissolveValue);
            }
        }
    }

    // ������������ � ����� ����� BoxCollider
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Lava"))
        {
            TakeLavaDamage();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Lava"))
        {
            TakeLavaDamage();
        }
    }

    public void AddCash(int amount = 10)
    {
        cash += amount;
        Debug.Log($"��������� �����: {amount}. �����: {cash}");
    }
}