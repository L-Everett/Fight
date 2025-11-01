using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private float playTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SaveSystem.LoadGame();
        playTime = Time.time;
    }

    public void SaveGame()
    {
        SaveSystem.SaveGame();
    }

    private void OnApplicationQuit()
    {
        DataModel.Instance.mPlayTime += Time.time - playTime;
        SaveSystem.SaveGame();
    }
}
