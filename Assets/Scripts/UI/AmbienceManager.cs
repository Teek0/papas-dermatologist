using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AmbienceManager : MonoBehaviour
{
    [Header("Audio configuration")]
    public AudioMixerGroup ambGroup;
    public AudioClip ambLoopClip;
    private AudioSource ambSource;
    [Range(0f, 1f)] public float ambVolume = 1f;
    public float fadeInDuration = 2.5f;

    private void Awake()
    {
        Time.timeScale = 1f;
        ConfigureAmbience();
        PlayAmbience();
    }
    public void ConfigureAmbience()
    {
        if (ambSource == null)
        {
            ambSource = gameObject.AddComponent<AudioSource>();
        }

        ambSource.clip = ambLoopClip;
        ambSource.volume = 0f;
        ambSource.loop = true;
        ambSource.outputAudioMixerGroup = ambGroup;
        ambSource.spatialBlend = 0f;
        ambSource.ignoreListenerPause = true;
    }

    public void PlayAmbience()
    {
        ambSource.Play();
        StartCoroutine(FadeInVolume(ambSource, 0f, ambVolume, fadeInDuration));
    }

    private IEnumerator FadeInVolume(AudioSource source, float from, float to, float duration)
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
}
