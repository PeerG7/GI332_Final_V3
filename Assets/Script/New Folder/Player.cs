using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Player : Entity
{
    public static Player Instance;
    [Header("Visuals")]
    public GameObject visuals; // ลาก Model ตัวละครมาใส่ที่นี่
    private Collider playerCollider;

    protected InputSystem_Actions Controls;
    protected Vector2 MoveInput;
    public NetworkVariable<float> Cooldown = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float CanCast = 5f;
    protected override void Start()
    {
        playerCollider = GetComponent<Collider>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // ไม่ต้อง new อีกแล้ว แค่ Enable
            Controls.Enable();
            Controls.Player.Enable();
        }
        else
        {
            Controls.Disable();
        }
    }

    protected virtual void Class() { }
    protected override void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Controls = new InputSystem_Actions(); // สร้างแค่ครั้งเดียวตรงนี้
        Application.runInBackground = true;
    }
    protected override void Update()
    {
        if (!IsOwner) return;

        
        Class();
        UpdateCooldownServerRpc(Time.deltaTime);
        /*Debug.Log($"Player.enabled={Controls.Player.enabled} | Move.enabled={Controls.Player.Move.enabled}");
        Debug.Log($"HasFocus={Application.isFocused} | MoveInput={MoveInput}");*/
    }
    protected override void FixedUpdate()
    {
        if (!IsOwner) return;

        MoveInput = Controls.Player.Move.ReadValue<Vector2>();

        Vector3 targetDirection = new Vector3(MoveInput.x, 0, MoveInput.y).normalized;
        currentVelocity = Vector3.Lerp(currentVelocity, targetDirection, SmoothTime * Time.fixedDeltaTime);

        // ✅ เปลี่ยนจาก transform.position += เป็น rb.MovePosition
        Vector3 newPosition = rb.position + currentVelocity * Speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
        rb.AddTorque(Vector3.forward * -currentVelocity.x *Speed * 30000f, ForceMode.Impulse);
        if (rb != null) rb.mass = Def;
        if (IsServer && Hp.Value <= 0)
        {
            InGameController controller = Object.FindAnyObjectByType<InGameController>();
            if (controller != null)
                controller.OnPlayerDie(OwnerClientId);
            GetComponent<NetworkObject>().Despawn();
        }
    }
    protected void OnEnable()
    {
        if (Controls != null)
        {
            Controls.Enable();
            Controls.Player.Enable();
        }
    }
    protected void OnDisable()
    {
        if (Controls != null)
        {
            Controls.Player.Disable();
            Controls.UI.Disable();
            Controls.Disable();
        }
    }
    protected void OnDestroy()
    {
        if (Controls != null)
        {
            Controls.Dispose(); // ✅ Dispose เมื่อ Object ถูกทำลาย
        }
    }
    public void StartGame()
    {
        GameStart = true;
        isDead = false;
        Hp.Value = 100;
        ResetCooldownServerRpc();
    }
    public void ResetPlayerStatus()
    {
        isDead = false;
        Hp.Value = 100;
        ShowPlayerClientRpc(); // เรียกให้แสดงตัวกลับมาเมื่อเริ่มรอบใหม่
    }
    [ClientRpc]
    public void HidePlayerClientRpc()
    {
        if (visuals) visuals.SetActive(false); // ปิดโมเดล
        if (playerCollider) playerCollider.enabled = false; // ปิดการชน
    }

    // ฟังก์ชันสำหรับ "แสดง" ตัวละครกลับมา
    [ClientRpc]
    public void ShowPlayerClientRpc()
    {
        if (visuals) visuals.SetActive(true); // เปิดโมเดล
        if (playerCollider) playerCollider.enabled = true; // เปิดการชน
    }
    protected virtual void OnCollisionEnter(Collision collision)
    {
        // เฉพาะ "เจ้าของ" ตัวละครที่วิ่งไปชนเท่านั้นที่มีสิทธิ์สั่ง (ป้องกันการรันซ้ำซ้อน)
        if (!IsOwner) return;
        if (collision.gameObject.CompareTag("Destory"))
        {
            Die();
        }
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Player"))
        {

            var targetNetObj = collision.gameObject.GetComponent<NetworkObject>();
            if (targetNetObj != null)
            {
                // คำนวณทิศทางจากเครื่องเราส่งไปให้ Server
                Vector3 pushDir = (collision.transform.position - transform.position).normalized;
                RequestAtkServerRpc(targetNetObj.NetworkObjectId, pushDir);
            }
        }
    }

    [ServerRpc]
    void RequestAtkServerRpc(ulong targetId, Vector3 direction)
    {
        // ส่งสัญญาณไปหา Client ทุกเครื่อง (รวมถึง Host) ว่าให้จัดการแรงผลักตัวละครตัวนี้
        ApplyAtkEffectClientRpc(targetId, direction);
    }
    [ServerRpc(RequireOwnership = false)]
    protected void ResetCooldownServerRpc()
    {
        Cooldown.Value = 0f;
    }
    [ServerRpc]
    void UpdateCooldownServerRpc(float dt)
    {
        // Server เป็นคนแก้ค่า ทุกเครื่องจะเห็นค่าตรงกันแน่นอน
        Cooldown.Value = Mathf.Min(Cooldown.Value + dt, CanCast);
    }
    [ServerRpc]
    protected void RequestInitializeStatsServerRpc()
    {
        // Server เป็นคนเซ็ตค่าให้ ทุกคนจะเห็นตรงกันแน่นอน
        Hp.Value = 100;
    }
    [ClientRpc]
    void ApplyAtkEffectClientRpc(ulong targetId, Vector3 direction)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetNetObj))
        {
            Entity targetEntity = targetNetObj.GetComponent<Entity>();
            Rigidbody targetRb = targetNetObj.GetComponent<Rigidbody>();

            if (targetEntity != null)
            {
                // เรียกใช้ฟังก์ชัน public ที่เราสร้างไว้
                // ส่งแค่ค่า AtkPower ไป เดี๋ยว Entity ไปลบ Def เองข้างใน
                targetEntity.TakeDamageServerRpc(AtkPower);
            }

            if (targetRb != null)
            {
                targetRb.AddForce(direction * AtkPower * 2 , ForceMode.Impulse);
                targetRb.linearDamping = 5f;
            }
        }
    }
}
