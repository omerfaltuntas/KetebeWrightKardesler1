using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

public class KetebeUIManager : MonoBehaviour
{
    [Header("Settings and Paused Panel")]
    [SerializeField] private GameObject settingsBG;
    [SerializeField] private Button settingsButton;
    private bool isSettings;

    [SerializeField] private Button homeButton;
    [SerializeField] private Button backButton;

    [Header("Sound System")]
    [SerializeField] private Button soundButton;
    public Sprite[] soundSprites;
    private bool isSound = true;

    [Header("Sound Effect System")]
    [SerializeField] private Button soundEffectButton;
    public Sprite[] soundEffectSprites;
    private bool isSoundEffect = true;

    [Header("Reading System")]
    [SerializeField] private Button readingButton;
    public Sprite[] readingSprites;
    private bool isReadingSound = true;

    private AudioSource musicController;
    private List<AudioSource> soundEffects;
    private AudioSource readingSounds;

    private int isSoundData;
    private int isMusicData;
    private int isReadingData;

    private void Start()
    {
        isMusicData = PlayerPrefs.GetInt("MusicData");
        isSoundData = PlayerPrefs.GetInt("SoundData");
        isReadingData = PlayerPrefs.GetInt("isReadingData");

        SaveAndLoadSystem();

        SettingsPanelActive();
        HomeController();
        BackController();
        SoundEffectFindAudioSystem();

        SoundSystem();
        SoundEffectSystem();
        ReadingSystem();

        musicController = AudioManager.instance._bgAudioSource;
        readingSounds = AudioManager.instance._storytellingSource;

        settingsBG.SetActive(false);
    }

    private void Update()
    {
        // 🔴 SES (sadece bg ve single)
        if (isSound)
        {
            soundButton.GetComponent<Image>().sprite = soundSprites[1];
            AudioManager.instance._bgAudioSource.mute = true;
            AudioManager.instance._singleAudioSource.mute = true;
            PlayerPrefs.SetInt("MusicData", 1);
        }
        else
        {
            soundButton.GetComponent<Image>().sprite = soundSprites[0];
            AudioManager.instance._bgAudioSource.mute = false;
            AudioManager.instance._singleAudioSource.mute = false;
            PlayerPrefs.SetInt("MusicData", 0);
        }

        // 🟡 EFEKT (sadece loop sesler + buttonClick)
        if (isSoundEffect)
        {
            soundEffectButton.GetComponent<Image>().sprite = soundEffectSprites[1];
            AudioManager.instance.MuteAllLoopingSounds(true);
            PlayerPrefs.SetInt("SoundData", 1);
        }
        else
        {
            soundEffectButton.GetComponent<Image>().sprite = soundEffectSprites[0];
            AudioManager.instance.MuteAllLoopingSounds(false);
            PlayerPrefs.SetInt("SoundData", 0);
        }

        // 🟢 OKUMA (ayrı kontrol edilir)
        if (isReadingSound)
        {
            readingButton.GetComponent<Image>().sprite = readingSprites[1];
            AudioManager.instance._storytellingSource.mute = true;
            PlayerPrefs.SetInt("ReadingData", 1);
        }
        else
        {
            readingButton.GetComponent<Image>().sprite = readingSprites[0];
            AudioManager.instance._storytellingSource.mute = false;
            PlayerPrefs.SetInt("ReadingData", 0);
        }
    }


    private void SettingsPanelActive()
    {
        settingsButton.onClick.AddListener(() =>
        {
            isSettings = !isSettings;
            ClickSound();
            settingsBG.SetActive(isSettings);
        });
    }

    private void HomeController()
    {
        homeButton.onClick.AddListener(() =>
        {
            ClickSound();
            StartCoroutine(BackToHomeWait());
        });
    }

    private void BackController()
    {
        backButton.onClick.AddListener(() =>
        {
            ClickSound();
            StartCoroutine(BackToHomeWait());
        });
    }

    IEnumerator BackToHomeWait()
    {
        yield return new WaitForSeconds(0.8f);
        SceneManager.LoadScene("MainMenu");
    }

    private void ClickSound()
    {
        AudioManager.instance.buttonClick.Play();
    }

    private void SoundSystem()
    {
        soundButton.onClick.AddListener(() =>
        {
            ClickSound();
            isSound = !isSound;
        });
    }

    private void SoundEffectSystem()
    {
        soundEffectButton.onClick.AddListener(() =>
        {
            ClickSound();
            isSoundEffect = !isSoundEffect;
        });
    }

    private void ReadingSystem()
    {
        readingButton.onClick.AddListener(() =>
        {
            ClickSound();
            isReadingSound = !isReadingSound;
        });
    }

    private void SoundEffectFindAudioSystem()
    {
        soundEffects = new List<AudioSource>();

        var allSources = FindObjectsOfType<AudioSource>(true);
        foreach (var source in allSources)
        {
            soundEffects.Add(source);
        }

        soundEffects.Remove(AudioManager.instance._bgAudioSource);
        soundEffects.Remove(AudioManager.instance._storytellingSource);
    }

    private void SoundEffectMuteActive()
    {
        foreach (AudioSource source in soundEffects)
        {
            source.mute = true;
        }
    }

    private void SoundEffectMutePassive()
    {
        foreach (AudioSource source in soundEffects)
        {
            source.mute = false;
        }
    }

    private void SaveAndLoadSystem()
    {
        isSoundEffect = isSoundData == 1;
        isSound = isMusicData == 1;
        isReadingSound = isReadingData == 1;
    }
}

