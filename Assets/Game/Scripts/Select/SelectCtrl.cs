using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SelectCtrl : MonoBehaviour
{
    [Header("角色")]
    public GameObject[] characters; // 索引0: A, 1: B, 2: C
    public Camera playerCamera;
    public Button leftButton;
    public Button rightButton;
    public float moveDuration = 1.0f;

    private GameObject selectedCharacter;
    private bool isMoving = false;
    private Vector3 centerPoint; // 三角形中心点（局部坐标）

    public TextMeshProUGUI characterNameText;

    [Header("难度")]
    public TextMeshProUGUI diffText;
    public TextMeshProUGUI diffDesText;
    public Button leftDiffButton;
    public Button rightDiffButton;
    public Color[] diffColor;
    private string[] diffs = { "简单", "普通", "困难", "地狱" };
    private string[] diffDes = { "你将面对非常简单的敌人，最大回合数为<color=#FF0000>10</color>。",
                                 "你将面对普通的敌人，最大回合数为<color=#FF0000>30</color>。",
                                 "你将面对较为强力的敌人，最大回合数为<color=#FF0000>50</color>。",
                                 "你将面对非常棘手的敌人，最大回合数为<color=#FF0000>99</color>。"};
    private int[] rounds = { 10, 30, 50, 99 };
    private int curDiff = 0;

    [Header("界面")]
    public GameObject uiRoot;

    private void OnEnable()
    {
        InitializeCharacterPositions();
        UpdateSelectedCharacter();
        curDiff = 0;
        FreshDiffText();
    }

    void Start()
    {
        
        leftButton.onClick.AddListener(() => RotateCharacters(true));
        rightButton.onClick.AddListener(() => RotateCharacters(false));
        leftDiffButton.onClick.AddListener(() => LastDiff());
        rightDiffButton.onClick.AddListener(() => NextDiff());
    }

    #region 角色选择
    private void InitializeCharacterPositions()
    {
        // 使用localPosition设置初始位置
        characters[0].transform.localPosition = new Vector3(-4, 0, 0);
        characters[1].transform.localPosition = new Vector3(4, 0, 0);

        // 计算C的位置（局部坐标）
        float sideLength = Vector3.Distance(
            characters[0].transform.localPosition,
            characters[1].transform.localPosition
        );

        float height = -(Mathf.Sqrt(3) / 2) * sideLength;
        characters[2].transform.localPosition = new Vector3(0, 0, height);

        // 计算初始中心点（局部坐标）
        UpdateCenterPoint();
    }

    // 更新三角形中心点（局部坐标）
    private void UpdateCenterPoint()
    {
        centerPoint = Vector3.zero;
        foreach (GameObject character in characters)
        {
            centerPoint += character.transform.localPosition;
        }
        centerPoint /= characters.Length;
    }

    private void UpdateSelectedCharacter()
    {
        float minDistance = Mathf.Infinity;
        GameObject closest = null;

        foreach (GameObject character in characters)
        {
            // 使用世界坐标计算距离
            float dist = Vector3.Distance(
                character.transform.position,
                playerCamera.transform.position
            );

            if (dist < minDistance)
            {
                minDistance = dist;
                closest = character;
            }
        }

        selectedCharacter = closest;
        RunningManager.Instance.mCurrentCharacter = selectedCharacter.name;
        characterNameText.text = StaticDataInterface.Character.GetCharacter(selectedCharacter.name).Name;
    }

    private void RotateCharacters(bool clockwise)
    {
        if (isMoving) return;
        isMoving = true;
        float angle = clockwise ? 120f : -120f;
        StartCoroutine(RotateAroundCenter(angle));
    }

    private IEnumerator RotateAroundCenter(float angle)
    {
        float elapsed = 0;
        Quaternion[] startRotations = new Quaternion[3];
        Vector3[] startLocalPositions = new Vector3[3];

        // 存储初始局部位置和旋转
        for (int i = 0; i < 3; i++)
        {
            startRotations[i] = characters[i].transform.rotation;
            startLocalPositions[i] = characters[i].transform.localPosition;
        }

        // 计算目标位置（围绕中心点旋转，局部坐标）
        Vector3[] targetLocalPositions = new Vector3[3];
        for (int i = 0; i < 3; i++)
        {
            // 计算从中心点到角色的向量（局部坐标）
            Vector3 toCharacter = startLocalPositions[i] - centerPoint;

            // 旋转这个向量（在父节点的局部空间中）
            Vector3 rotatedVector = Quaternion.Euler(0, angle, 0) * toCharacter;

            // 新位置 = 中心点 + 旋转后的向量
            targetLocalPositions[i] = centerPoint + rotatedVector;
        }

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            for (int i = 0; i < 3; i++)
            {
                // 插值局部位置
                characters[i].transform.localPosition = Vector3.Lerp(
                    startLocalPositions[i],
                    targetLocalPositions[i],
                    t
                );

                // 保持角色朝向
                characters[i].transform.rotation = startRotations[i];
            }
            yield return null;
        }

        // 确保精确到达目标位置
        for (int i = 0; i < 3; i++)
        {
            characters[i].transform.localPosition = targetLocalPositions[i];
        }

        // 更新中心点（位置改变后中心点可能变化）
        UpdateCenterPoint();

        isMoving = false;
        UpdateSelectedCharacter();
    }
    #endregion

    #region 难度选择
    void NextDiff()
    {
        if (curDiff == diffs.Length - 1) return;
        curDiff++;
        FreshDiffText();
    }
    void LastDiff()
    {
        if (curDiff == 0) return;
        curDiff--;
        FreshDiffText();
    }
    void FreshDiffText()
    {
        diffText.text = diffs[curDiff];
        diffText.color = diffColor[curDiff];
        diffDesText.text = diffDes[curDiff];
        RunningManager.Instance.mCurrentDiff = curDiff;
        RunningManager.Instance.mMaxRound = rounds[curDiff];
    }
    #endregion

    public void Return()
    {
        gameObject.SetActive(false);
        uiRoot.SetActive(true);
    }

    public void Next()
    {
        if (RunningManager.Instance.mCurrentCharacter != "C001")
        {
            MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_TIP_SHOW, "该角色暂未开放！");
            return;
        }
        MsgManager.Instance.EmitMsg(Constant.MSG_NOTIFY_SCENE_CHANGE, "MainScene");
    }
}