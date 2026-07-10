using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    private const float MinLinearVolume = 0.0001f;
    private const string GlobalVolumeKey = "globalVolume";
    private const string MusicVolumeKey = "musicVolume";
    private const string SfxVolumeKey = "sfxVolume";
    private const string AmbientVolumeKey = "ambientVolume";
    private const string GlobalMixerParameter = "masterMix";
    private const string MusicMixerParameter = "musicMix";
    private const string SfxMixerParameter = "sfxMix";
    private const string AmbientMixerParameter = "ambientMix";

    [System.Serializable]
    public struct VolumeData
    {
        public string mixerParameter;
        public string playerPrefKey;
        public Slider slider;
    }

    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private List<VolumeData> volumeSettings = new List<VolumeData>();

    private bool initialized;

    public void Configure(AudioMixer mixer, List<VolumeData> settings)
    {
        mainMixer = mixer;
        volumeSettings = settings;
        initialized = false;
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        if (mainMixer == null)
        {
            Debug.LogWarning("AudioSettingsController: mainMixer is null. Volume settings will be skipped.");
            return;
        }

        if (volumeSettings == null)
            return;

        foreach (VolumeData data in volumeSettings)
        {
            float savedVolume = PlayerPrefs.GetFloat(data.playerPrefKey, GetDefaultVolume(data.playerPrefKey));
            ApplyVolume(data.mixerParameter, data.playerPrefKey, savedVolume);

            if (data.slider == null)
                continue;

            data.slider.SetValueWithoutNotify(savedVolume);

            string mixerParameter = data.mixerParameter;
            string playerPrefKey = data.playerPrefKey;
            data.slider.onValueChanged.AddListener(value => ApplyVolume(mixerParameter, playerPrefKey, value));
        }
    }

    public void SetGlobalVolume(float value)
    {
        ApplyVolume(GlobalMixerParameter, GlobalVolumeKey, value);
    }

    public void SetMusicVolume(float value)
    {
        ApplyVolume(MusicMixerParameter, MusicVolumeKey, value);
    }

    public void SetSfxVolume(float value)
    {
        ApplyVolume(SfxMixerParameter, SfxVolumeKey, value);
    }

    public void SetAmbientVolume(float value)
    {
        ApplyVolume(AmbientMixerParameter, AmbientVolumeKey, value);
    }

    private float GetDefaultVolume(string playerPrefKey)
    {
        return playerPrefKey switch
        {
            MusicVolumeKey => 0.55f,
            AmbientVolumeKey => 0.75f,
            _ => 1f
        };
    }

    private void ApplyVolume(string mixerParameter, string playerPrefKey, float linearValue)
    {
        if (mainMixer == null || string.IsNullOrEmpty(mixerParameter))
            return;

        float dB = Mathf.Log10(Mathf.Clamp(linearValue, MinLinearVolume, 1f)) * 20f;
        mainMixer.SetFloat(mixerParameter, dB);

        if (!string.IsNullOrEmpty(playerPrefKey))
            PlayerPrefs.SetFloat(playerPrefKey, linearValue);
    }
}
