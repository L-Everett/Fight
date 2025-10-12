using DG.Tweening;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private readonly float[] leftBound = {-57, -27 };
    private readonly float[] rightBound = {37, 27 };
    private readonly float[] topBound = {15, 13.5f };
    private readonly float[] bottomBound = {-8, -4 };
    public Transform mPlayerRoot;
    public Transform mFollowTrans;
    [Header("天气系统")]
    public GameObject[] mWeatherList;

    [HideInInspector] public float mLeft;
    [HideInInspector] public float mRight;
    [HideInInspector] public float mTop;
    [HideInInspector] public float mBottom;
    [Header("死区设置")]
    public float deadZoneWidth = 2f;   // 水平死区宽度
    public float deadZoneHeight = 0.5f; // 垂直死区高度
    [Header("节点")]
    public Transform mInitRoot;

    private Camera mCamera;
    private float mInitOrthographicSize;
    private float mCameraHalfWidth;
    private float mCameraHalfHeight;
    private Vector3 mCurrentVelocity;
    private float mFollowOffset = 6f;

    private bool mIsFollow;
    private Sequence mSequence;

    void Start()
    {
        mCamera = GetComponent<Camera>();
        mInitOrthographicSize = mCamera.orthographicSize;
        mCameraHalfHeight = mInitOrthographicSize;
        mCameraHalfWidth = mCameraHalfHeight * mCamera.aspect;
        mIsFollow = false;
        mSequence = DOTween.Sequence();
    }

    private void LateUpdate()
    {
        FollowMove();
    }

    private void OnDestroy()
    {
        if (mSequence != null)
        {
            mSequence.Kill();
            mSequence = null;
        }
    }

    #region 跟随移动
    private void FollowMove()
    {
        if (!mIsFollow) return;
        // 计算目标位置
        Vector3 targetPos = new Vector3(
            mFollowTrans.position.x,
            mFollowTrans.position.y + mFollowOffset,
            transform.position.z
        );

        targetPos = ApplyDeadZoon(targetPos);

        // 平滑移动镜头
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref mCurrentVelocity,
            0.6f
        );

        // 边界限制
        ApplyBoundaryRestrictions();
    }

    private Vector3 ApplyDeadZoon(Vector3 targetPos)
    {
        // 死区判断：仅在玩家超出死区时更新目标位置
        Vector3 cameraCenter = transform.position;
        float deltaX = Mathf.Abs(targetPos.x - cameraCenter.x);
        float deltaY = Mathf.Abs(targetPos.y - cameraCenter.y - mFollowOffset);

        // 如果玩家未超出死区，保持镜头原位
        if (deltaX < deadZoneWidth / 2)
        {
            targetPos.x = transform.position.x;
        }
        if(deltaY < deadZoneHeight / 2)
        {
            targetPos.y = transform.position.y;
        }
        return targetPos;
    }

    private void ApplyBoundaryRestrictions()
    {
        float clampedX = Mathf.Clamp(transform.position.x, mLeft + mCameraHalfWidth, mRight - mCameraHalfWidth);
        float clampedY = Mathf.Clamp(transform.position.y, mBottom + mCameraHalfHeight, mTop - mCameraHalfHeight);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
    #endregion
}