using UnityEngine;
using UnityEngine.Audio;

public class UISoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup outputGroup;
    [SerializeField, Range(0f, 1f)] private float defaultVolume = 1f;

    public void PlayOneShot(AudioClip clip)
    {
        PlayOneShot(clip, defaultVolume);
    }

    public void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        AudioSource source = CreateSource(clip.name);
        source.PlayOneShot(clip, volume);
        Destroy(source.gameObject, clip.length + 0.1f);
    }

    private AudioSource CreateSource(string clipName)
    {
        GameObject audioObject = new($"uiSfx_{clipName}");
        audioObject.transform.SetParent(transform);

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = outputGroup;
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        return source;
    }
}
