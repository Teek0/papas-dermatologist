using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class AnimaleseSpeaker : MonoBehaviour
{
    [Header("Voice Profile")]
    public float pitchShift = -4.0f; //in semitones.
    [Tooltip("How much the pitch bounces randomly. (0.0 to 1.0 recommended)")]
    public float pitchVariation = 0.5f;
    [Tooltip("Volume multiplier.")]
    public float volume = 1.0f;

    private Dictionary<string, AudioClip> clipLibrary = new Dictionary<string, AudioClip>();
    private AudioSource[] audioSources;

    public float crossFadeDuration = 0.05f; // in seconds i guess
    private int currentSourceIndex = 0;

    // Se asegura de que ya existen 2 AudioSources.
    void Start()
    {
        if (audioSources == null || audioSources.Length < 2)
        {
            Debug.LogError("Necesitas asignar al menos 2 AudioSources en el array!");
        }
    }

    /// Loads 'a' through 'z' from Resources/Animalese
    void LoadClipsFromResources()
    {
        string[] chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray().Select(c => c.ToString()).ToArray();
        foreach (string c in chars)
        {
            // Assumes files are named "a", "b", etc. inside "Assets/Resources/Animalese/"
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
        }

        LoadClipsFromResources();
    }

    // Fade-out and stop worker.
    IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // Fade to 0
            source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }

    /// Main function.

    public void SpeakKey(string character)
    {
        character = character.ToLower();

        if (!clipLibrary.ContainsKey(character)) return;

        // Determine the audio path
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
        // Sets audio source clip.
        incomingSource.clip = clipToPlay;

        // Play
        incomingSource.Play();

        currentSourceIndex = nextSourceIndex;
    }
}