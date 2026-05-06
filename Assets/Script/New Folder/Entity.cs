using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Entity : NetworkBehaviour
{
    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject loseUI;
    public NetworkVariable<int> Hp = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    protected float Speed;
    protected int Def;
    protected int AtkPower;
    protected Rigidbody rb;
    protected StopHelper helper;
    protected Vector3 currentVelocity;
    protected float SmoothTime;
    public bool isDead = false;
    public bool GameStart = false;
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    protected virtual void Start() { }
    protected virtual void Update() { }
    protected virtual void FixedUpdate() { }

    protected void Atk(GameObject target)
    {
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb == null) return;

        // ผลัก
        Vector3 pushDir = (target.transform.position - transform.position).normalized;
        targetRb.AddForce(pushDir * AtkPower * 2, ForceMode.Impulse);

        // ทำให้หนืดเพื่อจะหยุด
        targetRb.linearDamping = 5f;

        // สร้างคลาสเล็กๆ หรือเก็บ Component ไว้เพื่อ Reset 
        // แต่ถ้าเอาแบบง่ายที่สุดสำหรับมือใหม่ ให้สร้างสคริปต์จิ๋วไปแปะที่ศัตรูครับ
        helper = target.GetComponent<StopHelper>();
        if (helper == null) helper = target.AddComponent<StopHelper>();
        helper.ResetAfter(1f);
    }
    protected virtual void Move() { }
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (GameStart)
        {
            if (isDead) return; // ถ้าตายแล้ว ไม่ต้องทำข้างล่างซ้ำ

            Hp.Value -= damage;

            if (Hp.Value <= 0)
            {
                isDead = true; // ล็อคไว้ทันทีว่าคนนี้ตายแล้วนะ
                Hp.Value = 0;

                if (InGameController.Instance != null)
                {
                    InGameController.Instance.OnPlayerDie(OwnerClientId);
                }
            }
        }
        
    }
    protected void Die()
    {
        if (IsServer)
        {
            // ค้นหา InGameController ในฉากแล้วบอกว่าฉันตายแล้ว
            FindObjectOfType<InGameController>().OnPlayerDie(OwnerClientId);
        }
    }
}
