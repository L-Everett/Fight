using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour, IMsgHandler
{
    private static bool isCreated = false;
    public GameObject loadingScreen;
    public Image progressFill;
    private bool isLoading = false;
    private Queue<string> loadingQ = new Queue<string>();

    private void Awake()
    {
        if (isCreated)
        {
            Destroy(gameObject);
            return;
        }
        isCreated = true;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        MsgManager.Instance.AddMsgListener(Constant.MSG_NOTIFY_SCENE_CHANGE, this);
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if(loadingQ.Count > 0 && !isLoading)
        {
            isLoading = true;
            SaveSystem.SaveGame();
            loadingScreen.SetActive(true);
            StartCoroutine(LoadSceneAsync(loadingQ.Dequeue()));
        }
    }

    private void OnDestroy()
    {
        MsgManager.Instance.RemoveALLMsgListener(this);
    }

    public void Handle(string msg, object obj)
    {
        if (msg == Constant.MSG_NOTIFY_SCENE_CHANGE)
        {
            loadingQ.Enqueue((string)obj);
        }
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // 重置进度条
        progressFill.fillAmount = 0f;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float progress = 0f;

        while (!asyncLoad.isDone)
        {
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // 更新UI
            progressFill.fillAmount = progress;

            // 加载完成但未激活
            if (asyncLoad.progress >= 0.9f)
            {
                // 激活场景
                asyncLoad.allowSceneActivation = true;

                int bgm = 0;
                switch (sceneName)
                {
                    case "MainScene":
                        bgm = 1;
                        break;
                }
                AudioManager.Instance.CrossFadeMusic(bgm);
                break;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);
        isLoading = false;
        loadingScreen.SetActive(false);
    }
}
