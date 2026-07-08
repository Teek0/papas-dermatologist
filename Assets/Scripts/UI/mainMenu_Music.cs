using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
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
    private Coroutine musicRoutine;
    private bool musicStarted;

    private void Awake()
    {
        if (loopClip ==  null)
        {
            Debug.LogWarning("You have not assigned a music loop.");
            // return;
        }

        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        RefreshPlaybackForActiveScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
    }

    private void HandleActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        RefreshPlaybackForActiveScene(newScene);
    }

    private void RefreshPlaybackForActiveScene(Scene activeScene)
    {
        if (ShouldPlayMusicForActiveScene(activeScene.name))
            StartMusic();
        else
            StopMusic();
    }

    private bool ShouldPlayMusicForActiveScene(string activeSceneName)
    {
        string ownerSceneName = gameObject.scene.name;

        return ownerSceneName == SceneNames.MainMenu && activeSceneName == SceneNames.MainMenu;
    }

    private void StartMusic()
    {
        if (musicStarted || introClip == null)
            return;

        musicStarted = true;

        if (SceneManager.GetActiveScene().name == SceneNames.MainMenu)
        {
            mainMixer.SetFloat("musicPitch", 1.06f);
        }
        else mainMixer.SetFloat("musicPitch", 1f);

        musicRoutine = StartCoroutine(PlayMusicRoutine());
    }

    private IEnumerator PlayMusicRoutine()
    {
        AudioSource musicSource = CreateSource("musicSource", false);
        musicSource.clip = introClip;
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.loop = false;
        musicSource.Play();

        yield return StartCoroutine(FadeInVolume(musicSource, 0f, musicVolume, fadeInDuration));

        yield return new WaitUntil(() => !musicSource.isPlaying);

        musicSource.clip = loopClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void StopMusic()
    {
        if (!musicStarted)
            return;

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i] != null)
                audioSources[i].Stop();
        }

        musicStarted = false;
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
