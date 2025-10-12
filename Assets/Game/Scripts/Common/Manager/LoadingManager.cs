using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour, IMsgHandler
{
    public GameObject loadingScreen;
    public Image progressFill;

    private void Start()
    {
        MsgManager.Instance.AddMsgListener(Constant.MSG_NOTIFY_SCENE_CHANGE, this);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        MsgManager.Instance.RemoveALLMsgListener(this);
    }

    public void Handle(string msg, object obj)
    {
        if (msg == Constant.MSG_NOTIFY_SCENE_CHANGE)
        {
            loadingScreen.SetActive(true);
            StartCoroutine(LoadSceneAsync((string)obj));
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
                    case "StartScene":
                        break;
                    case "MainScene":
                        bgm = 1;
                        break;
                    default:
                        break;
                }
                AudioManager.Instance.CrossFadeMusic(bgm);

                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }

        progressFill.fillAmount = 0f;
        loadingScreen.SetActive(false);
    }
}
