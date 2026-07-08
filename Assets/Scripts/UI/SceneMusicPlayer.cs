using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SceneMusicPlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip loopClip;

    [Header("Playback")]
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private AudioSource source;
    private Coroutine playbackRoutine;
    private Coroutine fadeRoutine;
    private bool isPlaying;

    private void Awake()
    {
        ConfigureSource();
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
    }

    private void Start()
    {
        RefreshForActiveScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
    }

    private void HandleActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        RefreshForActiveScene(newScene);
    }

    private void RefreshForActiveScene(Scene activeScene)
    {
        if (gameObject.scene == activeScene || gameObject.scene.name == activeScene.name)
            Play();
        else
            Stop();
    }

    private void ConfigureSource()
    {
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = 0f;
        source.outputAudioMixerGroup = musicGroup;
    }

    public void Play()
    {
        if (isPlaying || (introClip == null && loopClip == null))
            return;

        isPlaying = true;
        playbackRoutine = StartCoroutine(PlaybackRoutine());
    }

    public void Stop()
    {
        if (!isPlaying || source == null)
            return;

        if (playbackRoutine != null)
            StopCoroutine(playbackRoutine);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOutAndStop());
        isPlaying = false;
    }

    private IEnumerator PlaybackRoutine()
    {
        AudioClip firstClip = introClip != null ? introClip : loopClip;
        source.clip = firstClip;
        source.loop = false;
        source.Play();

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeVolume(source.volume, volume, fadeInDuration));
        yield return fadeRoutine;

        if (introClip != null && loopClip != null)
        {
            yield return new WaitUntil(() => source == null || !source.isPlaying);

            if (source == null || !isPlaying)
                yield break;

            source.clip = loopClip;
            source.loop = true;
            source.volume = volume;
            source.Play();
        }
        else if (loopClip != null)
        {
            source.loop = true;
        }
    }

    private IEnumerator FadeOutAndStop()
    {
        yield return FadeVolume(source.volume, 0f, fadeOutDuration);
        source.Stop();
        source.clip = null;
    }

    private IEnumerator FadeVolume(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            source.volume = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        source.volume = to;
    }
}
