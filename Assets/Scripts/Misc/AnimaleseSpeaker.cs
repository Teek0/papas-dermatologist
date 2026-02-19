using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Audio;

public class AnimaleseSpeaker : MonoBehaviour
{
    [Header("Voice Profile")]
    public float pitchShift = 0.0f;
    [Tooltip("How much the pitch bounces randomly. (0.0 to 1.0 recommended)")]
    public float pitchVariation = 0.5f;
    [Tooltip("Volume multiplier.")]
    public float volume = 1.0f;
    public AudioMixerGroup sfxGroup;

    private Dictionary<string, AudioClip> clipLibrary = new Dictionary<string, AudioClip>();
    private AudioSource[] audioSources;

    public float crossFadeDuration = 0.05f;
    private int currentSourceIndex = 0;

    void Start()
    {
        if (audioSources == null || audioSources.Length < 2)
        {
            Debug.LogError("Necesitas asignar al menos 2 AudioSources en el array!");
        }
    }

    void LoadClipsFromResources()
    {
        string[] chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray().Select(c => c.ToString()).ToArray();
        foreach (string c in chars)
        {
            AudioClip clip = Resources.Load<AudioClip>("Animalese/" + c);
            if (clip != null) clipLibrary[c] = clip;
        }
    }

    void Awake()
    {
        audioSources = new AudioSource[2];

        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
            audioSources[i].spatialBlend = 0f;
            audioSources[i].outputAudioMixerGroup = sfxGroup;
        }

        LoadClipsFromResources();
    }

    IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

    public void SpeakKey(string character)
    {
        character = character.ToLower();

        if (!clipLibrary.ContainsKey(character)) return;

        AudioClip clipToPlay = clipLibrary[character];

        int nextSourceIndex = (currentSourceIndex + 1) % audioSources.Length;
        AudioSource outgoingSource = audioSources[currentSourceIndex];
        AudioSource incomingSource = audioSources[nextSourceIndex];

        if (outgoingSource.isPlaying)
        {
            StartCoroutine(FadeOut(outgoingSource, crossFadeDuration));
        }

        float baseCents = pitchShift * 100.0f;
        float randomCents = UnityEngine.Random.Range(-300f, 300f) * pitchVariation;

        float totalCents = baseCents + randomCents;
        float pitchMultiplier = Mathf.Pow(2, totalCents / 1200.0f);

        incomingSource.pitch = pitchMultiplier;
        incomingSource.volume = volume;
        incomingSource.clip = clipToPlay;

        incomingSource.Play();

        currentSourceIndex = nextSourceIndex;
    }
}