#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class StartSceneLoader
{
    [RuntimeInitializeOnLoadMethod]
    static void Initialize()
    {
        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            return;
        }
        SceneManager.LoadScene("StartScene");
    }

}
#endif