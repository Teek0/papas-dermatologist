using System.Collections;
using UnityEngine;

public class CreamAudioManager : MonoBehaviour
{
    public AudioSource creamAudioSource;
    [Header("Fade-in AND Fade-out duration")]
    public float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    private void OnEnable()
    {
        BrushInputWorld.OnBrushValidStateChanged += HandleBrushAudio;
    }

    private void OnDisable()
    {
        BrushInputWorld.OnBrushValidStateChanged -= HandleBrushAudio;
    }
    private void HandleBrushAudio(bool isBrushing)
    {
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
