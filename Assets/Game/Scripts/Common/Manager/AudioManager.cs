using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [HideInInspector] public AudioSource musicSource;
    [HideInInspector] public AudioSource sfxSource;
    public AudioClip[] backgroundMusics;
    public AudioClip btnHover;
    public AudioClip btnClick;
    public AudioClip drawCard;
    public AudioClip playCard;
    public AudioClip win;
    public AudioClip lose;
    public int mIsPlayBg = -1;

    // 添加音量属性
    private float _musicVolume = 0.5f;
    private float _sfxVolume = 0.5f;

    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = Mathf.Clamp01(value);
            musicSource.volume = _musicVolume;
            //PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = Mathf.Clamp01(value);
            sfxSource.volume = _sfxVolume;
            //PlayerPrefs.SetFloat("SfxVolume", _sfxVolume);
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;

        // 加载保存的音量设置
        LoadVolumeSettings();
    }

    private void Start()
    {
        PlayMusic(0);
    }

    // 加载音量设置
    private void LoadVolumeSettings()
    {
        musicSource.volume = MusicVolume;
        sfxSource.volume = SfxVolume;
    }

    private void PlayMusic(int musicIndex)
    {
        if (musicIndex < 0 || musicIndex >= backgroundMusics.Length)
        {
            return;
        }

        AudioClip musicToPlay = backgroundMusics[musicIndex];

        if (musicSource.clip == musicToPlay && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = musicToPlay;
        musicSource.Play();
        mIsPlayBg = musicIndex;
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlayHover()
    {
        sfxSource.PlayOneShot(btnHover);
    }

    public void PlayWin()
    {
        sfxSource.PlayOneShot(win);
    }

    public void PlayLose()
    {
        sfxSource.PlayOneShot(lose);
    }

    public void PlayBtnClick()
    {
        sfxSource.PlayOneShot(btnClick);
    }

    public void PlayDrawCard()
    {
        sfxSource.PlayOneShot(drawCard);
    }

    public void PlayPlayCard()
    {
        sfxSource.PlayOneShot(playCard);
    }

    public void CrossFadeMusic(int newMusicIndex, float fadeDuration = 1.0f)
    {
        StartCoroutine(CrossFadeRoutine(newMusicIndex, fadeDuration));
    }

    private IEnumerator CrossFadeRoutine(int newMusicIndex, float fadeDuration)
    {
        if (newMusicIndex < 0 || newMusicIndex >= backgroundMusics.Length) yield break;

        AudioClip newMusic = backgroundMusics[newMusicIndex];
        float startVolume = musicSource.volume;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newMusic;
        musicSource.Play();

        mIsPlayBg = newMusicIndex;

        while (musicSource.volume < startVolume)
        {
            musicSource.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        musicSource.volume = startVolume;
    }
}