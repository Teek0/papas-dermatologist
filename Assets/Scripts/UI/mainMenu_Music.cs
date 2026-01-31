using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;
using RangeAttribute = UnityEngine.RangeAttribute;

public class mainMenu_Music : MonoBehaviour
{
    [Header("Audio Mixer setup")]
    public AudioMixer mainMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup ambGroup;
    public AudioMixerGroup sfxGroup;

    [Header("Music files")]
    public AudioClip introClip;
    public AudioClip loopClip;

    [Header("Music settings")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float startButtonVolume = 0.3f;
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 1.8f;

    private List<AudioSource> audioSources = new List<AudioSource>();

    [System.Serializable]
    public struct VolumeData
    {
        public string mixerParameter; 
        public string playerPrefKey;  
        public Slider slider;         
    }

    public List<VolumeData> volumeSettings;

    private void Awake()
    {
        if (loopClip ==  null)
        {
            Debug.LogWarning("You have not assigned a music loop.");
            // return;
        }

        if (introClip != null)
        {
            AudioSource musicSource = CreateSource("musicSource", false);
            musicSource.clip = introClip;
            musicSource.outputAudioMixerGroup = musicGroup;

            mainMixer.SetFloat("musicPitch", 1.06f);
            StartCoroutine(PlayMusic());

            IEnumerator PlayMusic()
            {
                musicSource.clip = introClip;
                musicSource.loop = false;
                musicSource.Play();

                // Fade-in
                yield return StartCoroutine(FadeInVolume(musicSource, 0f, musicVolume, fadeInDuration));

                yield return new WaitUntil(() =>
                !musicSource.isPlaying && musicSource.timeSamples == 0);

                musicSource.clip = loopClip;
                musicSource.loop = true;
                musicSource.Play();
            }
        }
    }

    private void Start()
    {
        foreach (var data in volumeSettings)
        {
            float savedVolume = PlayerPrefs.GetFloat(data.playerPrefKey, 1f);
            if (data.slider != null)
            {
                data.slider.value = savedVolume;
            }

            data.slider.onValueChanged.AddListener((val) => {
                UpdateVolume(data.mixerParameter, data.playerPrefKey, val);
            });

            UpdateVolume(data.mixerParameter, data.playerPrefKey, savedVolume);
        }
    }
    private void SetMixerVolume(string parameter, float linearValue)
    {
        float dB = Mathf.Log10(Mathf.Clamp(linearValue, 0.0001f, 1f)) * 20f;
        mainMixer.SetFloat(parameter, dB);
    }
    private void UpdateVolume(string mixerParam, string prefKey, float value)
    {
        SetMixerVolume(mixerParam, value);
        PlayerPrefs.SetFloat(prefKey, value);
    }

    IEnumerator FadeInVolume(AudioSource source, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        source.volume = to;
    }

    AudioSource CreateSource(string name, bool loop)
    {
        GameObject audioObject = new GameObject(name);
        audioObject.transform.SetParent(this.transform);
        AudioSource source = audioObject.AddComponent<AudioSource>();

        source.volume = musicVolume;  
        source.loop = loop;           
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        audioSources.Add(source);
        return source;
    }

    public void playStartSound(AudioClip btnStartClip)
    {
        // Play start sound when start button is clicked.
        AudioSource startBtnSource = CreateSource("startBtnSource", false);
        startBtnSource.outputAudioMixerGroup = sfxGroup;
        if (btnStartClip != null)
        {
            startBtnSource.PlayOneShot(btnStartClip, startButtonVolume);
        }
        
    }

    public IEnumerator FadeOutRoutine(string sceneIndex)
    {
        float timer = 0;
        float startVolume = musicVolume;

        if (fadeOutDuration > 0)
        {
            while (timer < fadeOutDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / fadeOutDuration;
                foreach (var source in audioSources)
                {
                    source.volume = Mathf.Lerp(startVolume, 0f, progress);
                }
                yield return null;
            }
        }
        else yield return null;

        SceneManager.LoadScene(sceneIndex);

    }
}
