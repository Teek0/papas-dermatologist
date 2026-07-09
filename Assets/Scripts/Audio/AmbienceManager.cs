using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AmbienceManager : MonoBehaviour
{
    [Header("Audio configuration")]
    public AudioMixerGroup ambGroup;
    public AudioClip ambLoopClip;
    private AudioSource ambSource;
    [Range(0f, 1f)] public float ambVolume = 1f;
    public float fadeInDuration = 2.5f;
    public float fadeOutDuration = 0.5f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        Time.timeScale = 1f;
        ConfigureAmbience();
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        RefreshPlaybackForActiveScene(SceneManager.GetActiveScene());
    }

    private void Start()
    {
        RefreshPlaybackForActiveScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        RefreshPlaybackForActiveScene(newScene);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshPlaybackForActiveScene(SceneManager.GetActiveScene());
    }

    private void RefreshPlaybackForActiveScene(Scene activeScene)
    {
        if (gameObject.scene == activeScene || gameObject.scene.name == activeScene.name)
            PlayAmbience();
        else
            StopAmbience();
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
        if (ambSource == null || ambSource.clip == null)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (ambSource.isPlaying && ambSource.volume >= ambVolume)
            return;

        ambSource.Play();
        fadeCoroutine = StartCoroutine(FadeVolume(ambSource, ambSource.volume, ambVolume, fadeInDuration, false));
    }

    public void StopAmbience()
    {
        if (ambSource == null || !ambSource.isPlaying)
            return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeVolume(ambSource, ambSource.volume, 0f, fadeOutDuration, true));
    }

    private IEnumerator FadeVolume(AudioSource source, float from, float to, float duration, bool stopWhenDone)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        source.volume = to;
        if (stopWhenDone)
            source.Stop();

        fadeCoroutine = null;
    }
}
