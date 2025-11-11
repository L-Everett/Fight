using System;
using UnityEngine;

public class S002 : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("移动速度 (单位/秒)")]
    public float moveSpeed = 8f;

    [Tooltip("到达目标点的距离阈值")]
    public float arrivalThreshold = 0.1f;

    [Tooltip("旋转速度 (度/秒)")]
    public float rotationSpeed = 720f;

    [Header("Timing Settings")]
    [Tooltip("到达目标后等待的时间 (秒)")]
    public float waitDuration = 1.5f;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool hasArrived = false;
    private float waitTimer = 0f;

    private Action callback;
    private BattleManager battleManager;

    private void Start()
    {
        battleManager = BattleManager.Instance;
    }

    // 初始化剑的位置并设置目标点
    public void Init(Vector3 startPosition, Vector3 target)
    {
        // 重置状态
        isMoving = true;
        hasArrived = false;
        waitTimer = 0f;

        // 在以startPosition为圆心，半径30的圆内随机位置
        float randomAngle = UnityEngine.Random.Range(0f, 2f * Mathf.PI); // 随机角度
        float randomRadius = UnityEngine.Random.Range(10f, 30f); // 随机半径 (10-30之间)

        // 计算随机位置 (保持Y轴不变)
        Vector3 randomOffset = new Vector3(
            Mathf.Cos(randomAngle) * randomRadius,
            0f,
            Mathf.Sin(randomAngle) * randomRadius
        );

        // 设置位置和目标
        transform.position = startPosition + randomOffset;
        targetPosition = target;

        // 立即朝向目标点
        LookAtTargetImmediate();

        // 确保剑是激活状态
        gameObject.SetActive(true);
    }

    public void SetCallback(Action callback)
    {
        this.callback = callback;
    }

    void Update()
    {
        if (!isMoving) return;

        if (!hasArrived)
        {
            // 平滑旋转朝向目标
            RotateTowardsTarget();

            // 向目标移动
            MoveTowardsTarget();
        }
        else
        {
            HandleWaitingPeriod();
        }
    }

    // 立即朝向目标点
    private void LookAtTargetImmediate()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    // 平滑旋转朝向目标
    private void RotateTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime * battleManager.currentTimeSpeed
            );
        }
    }

    // 向目标点移动
    private void MoveTowardsTarget()
    {
        if (battleManager.currentStage != BattleStage.Battle) DeactivateSword();
        // 计算移动方向
        Vector3 direction = (targetPosition - transform.position).normalized;

        // 计算移动距离（确保不会超过目标点）
        float moveDistance = moveSpeed * Time.deltaTime * battleManager.currentTimeSpeed;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (moveDistance > distanceToTarget)
        {
            moveDistance = distanceToTarget;
        }

        // 执行移动
        transform.position += direction * moveDistance;

        // 检查是否到达目标
        if (distanceToTarget <= arrivalThreshold)
        {
            hasArrived = true;
            OnArrival();
        }
    }

    // 到达目标点时的处理
    private void OnArrival()
    {
        callback?.Invoke();
    }

    // 处理等待期
    private void HandleWaitingPeriod()
    {
        waitTimer += Time.deltaTime * battleManager.currentTimeSpeed;

        if (waitTimer >= waitDuration)
        {
            DeactivateSword();
        }
    }

    // 禁用
    private void DeactivateSword()
    {
        isMoving = false;
        gameObject.SetActive(false);
    }
}