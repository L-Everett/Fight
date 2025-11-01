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
        if (SceneManager.GetActiveScene().name == "StartScene" || SceneManager.GetActiveScene().name == "TestScene")
        {
            return;
        }
        SceneManager.LoadScene("StartScene");
    }

}
#endif