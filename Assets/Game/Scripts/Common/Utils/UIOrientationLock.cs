using UnityEngine;

public class UIOrientationLock : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;
    public float cameraXRotation = 65f; // 相机X轴旋转角度

    [Header("UI Settings")]
    public bool lockXRotation = true;
    public bool lockYRotation = true;
    public bool lockZRotation = true;
    public Vector3 targetRotation = Vector3.zero; // 目标旋转角度

    private Quaternion initialRotation;
    private Transform parentTransform;

    private void Start()
    {
        // 获取主相机（如果未指定）
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // 保存初始旋转
        initialRotation = transform.rotation;

        // 获取父物体
        parentTransform = transform.parent;
    }

    private void LateUpdate()
    {
        // 确保相机存在
        if (mainCamera == null) return;

        // 计算目标旋转
        Quaternion targetRot = CalculateTargetRotation();

        // 应用旋转
        transform.rotation = targetRot;
    }

    private Quaternion CalculateTargetRotation()
    {
        // 方法1：完全面向相机
        // return Quaternion.LookRotation(mainCamera.transform.forward, mainCamera.transform.up);

        // 方法2：固定旋转（推荐）
        Quaternion rot = Quaternion.Euler(targetRotation);

        // 应用相机旋转补偿
        rot *= Quaternion.Euler(cameraXRotation, 0, 0);

        // 应用锁定
        Vector3 euler = rot.eulerAngles;
        if (lockXRotation) euler.x = initialRotation.eulerAngles.x;
        if (lockYRotation) euler.y = initialRotation.eulerAngles.y;
        if (lockZRotation) euler.z = initialRotation.eulerAngles.z;

        return Quaternion.Euler(euler);
    }

    // 在编辑器中可视化目标方向
    private void OnDrawGizmosSelected()
    {
        if (mainCamera == null) return;

        Gizmos.color = Color.green;
        Vector3 uiPosition = transform.position;
        Vector3 cameraPosition = mainCamera.transform.position;

        // 绘制相机到UI的线
        Gizmos.DrawLine(cameraPosition, uiPosition);

        // 绘制目标方向
        Quaternion targetRot = CalculateTargetRotation();
        Vector3 forward = targetRot * Vector3.forward;
        Vector3 up = targetRot * Vector3.up;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(uiPosition, forward * 1f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(uiPosition, up * 0.5f);
    }
}