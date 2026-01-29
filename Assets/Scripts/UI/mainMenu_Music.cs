using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;
using RangeAttribute = UnityEngine.RangeAttribute;

public class mainMenu_Music : MonoBehaviour
{
    [Header("Audio Mixer setup")]
    public AudioMixer mainMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup ambGroup;
    public AudioMixerGroup sfxGroup;
    [SerializeField] private string musicMix = "musicMix";
    [SerializeField] private string ambMix = "ambientMix";
    [SerializeField] private string sfxMix = "sfxMix";
    [SerializeField] private string masterMix = "masterMix";

    [Header("Music files")]
    public AudioClip introClip;
    public AudioClip loopClip;

    [Header("Music settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float startButtonVolume = 0.3f;
    public float fadeInDuration = 2f;
    public float fadeOutDuration = 1.8f;

    private List<AudioSource> audioSources = new List<AudioSource>();

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

            mainMixer.SetFloat("musicPitch", 2f); //This does NOT WORK????????? t_t
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

    public void setMusicVolume(float sliderValue)
    {
        // Sets music volume.
        // sliderMusicVolume -> musicMix in (Audio Mixer) MainMixer
        // this line converts porcentage to decibels :)
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;
        mainMixer.SetFloat(musicMix, dB);
    }
    public void setSFXVolume(float sliderValue)
    {
        // Sets SFX volume.
        // sliderSFXVolume -> sfxMix in (Audio Mixer) MainMixer
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;
        mainMixer.SetFloat(sfxMix, dB);
    }

    public void setGlobalVolume(float sliderValue)
    {
        //Sets Master Volume
        float dB = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;
        mainMixer.SetFloat(masterMix, dB);
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
