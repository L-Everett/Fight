using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private void OnEnable()
    {
        // 当设置界面打开时，加载当前音量设置
        LoadCurrentVolumeSettings();
    }

    private void LoadCurrentVolumeSettings()
    {
        if (AudioManager.Instance != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.MusicVolume;
            sfxVolumeSlider.value = AudioManager.Instance.SfxVolume;
        }
        else
        {
            // 默认值
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SfxVolume", 0.8f);
        }
    }

    public void SetMusicVolume()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.MusicVolume = musicVolumeSlider.value;
        }
    }

    public void SetSfxVolume()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SfxVolume = sfxVolumeSlider.value;
        }
    }

    public void CloseSetting()
    {
        BattleManager.Instance.Resume();
        gameObject.SetActive(false);
    }
}