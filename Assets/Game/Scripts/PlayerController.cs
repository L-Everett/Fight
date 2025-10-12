using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;       // 移动速度
    public float rotationSpeed = 500f; // 旋转速度
    public float jumpForce = 8f;       // 跳跃力度
    public float gravity = -20f;       // 重力强度

    [Header("翻滚设置")]
    public float rollSpeed = 10f;       // 翻滚速度
    public float rollDuration = 0.5f;  // 翻滚持续时间
    public float rollCooldown = 1f;    // 翻滚冷却时间
    public float rollHeightReduction = 0.5f; // 翻滚时角色高度减少量

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGround = true;

    // 翻滚相关变量
    private bool isRolling = false;
    private float rollTimer = 0f;
    private float rollCooldownTimer = 0f;
    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 rollDirection; // 翻滚方向

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // 保存原始控制器尺寸
        originalHeight = controller.height;
        originalCenter = controller.center;
    }

    void Update()
    {
        // 处理翻滚冷却时间
        if (rollCooldownTimer > 0)
            rollCooldownTimer -= Time.deltaTime;

        // 处理翻滚输入（左Shift键）
        if (Input.GetMouseButtonDown(1) && rollCooldownTimer <= 0 && !isRolling)
        {
            StartRoll();
        }

        // 执行翻滚
        if (isRolling)
        {
            HandleRoll();
            return; // 翻滚期间跳过其他移动逻辑
        }

        // 普通移动逻辑
        HandleMovement();

        // 跳跃逻辑
        HandleJump();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
    }

    void HandleJump()
    {
        if (isGround && Input.GetButtonDown("Jump"))
        {
            isGround = false;
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void StartRoll()
    {
        // 获取当前移动方向
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 如果没有输入方向，使用角色朝向
        if (Mathf.Approximately(horizontal, 0) && Mathf.Approximately(vertical, 0))
        {
            rollDirection = transform.forward;
        }
        else
        {
            rollDirection = new Vector3(horizontal, 0, vertical).normalized;
        }

        isRolling = true;
        rollTimer = 0f;
        rollCooldownTimer = rollCooldown;

        // 调整角色控制器尺寸（降低高度）
        controller.height = originalHeight - rollHeightReduction;
        controller.center = new Vector3(
            originalCenter.x,
            originalCenter.y - rollHeightReduction / 2,
            originalCenter.z
        );
    }

    void HandleRoll()
    {
        rollTimer += Time.deltaTime;

        // 翻滚期间保持初始方向
        controller.Move(rollDirection * rollSpeed * Time.deltaTime);

        // 应用重力
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 翻滚结束
        if (rollTimer >= rollDuration)
        {
            EndRoll();
        }
    }

    void EndRoll()
    {
        isRolling = false;

        // 恢复角色控制器尺寸
        controller.height = originalHeight;
        controller.center = originalCenter;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plane"))
        {
            isGround = true;
        }
    }

    // 在编辑器中显示冷却时间（调试用）
    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 200, 30), $"Roll CD: {rollCooldownTimer:F1}", style);
    }
}