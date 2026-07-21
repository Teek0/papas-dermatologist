using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class CreamAudioManager : MonoBehaviour
{
    public AudioSource creamAudioSource;
    [SerializeField] private AudioClip creamLoopClip;
    [SerializeField] private AudioMixerGroup outputGroup;
    [Header("Fade-in AND Fade-out duration")]
    public float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (creamAudioSource == null || creamLoopClip == null)
            return;

        creamAudioSource.playOnAwake = false;
        creamAudioSource.loop = true;
        creamAudioSource.spatialBlend = 0f;
        creamAudioSource.volume = 0f;
        creamAudioSource.outputAudioMixerGroup = outputGroup;

        creamAudioSource.clip = creamLoopClip;
    }

    private void OnEnable()
    {
        if (creamAudioSource == null || creamAudioSource.clip == null)
        {
            Debug.LogWarning("CreamAudioManager: missing AudioSource or cream clip. Brush audio will be disabled.");
            return;
        }

        if (!creamAudioSource.isPlaying)
            creamAudioSource.Play();

        BrushInputWorld.OnBrushValidStateChanged += HandleBrushAudio;
    }

    private void OnDisable()
    {
        BrushInputWorld.OnBrushValidStateChanged -= HandleBrushAudio;
    }
    private void HandleBrushAudio(bool isBrushing)
    {
        if (creamAudioSource == null)
            return;

        float target = isBrushing ? 1f : 0f;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(target));
    }

    private IEnumerator FadeRoutine(float target)
    {
        float startVol = creamAudioSource.volume;
        float elapsed = 0;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            creamAudioSource.volume = Mathf.Lerp(startVol, target, elapsed / fadeDuration);
            yield return null;
        }
        creamAudioSource.volume = target;
    }

}
