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

        foreach (VolumeData data in GetEffectiveVolumeSettings())
        {
            string mixerParameter = GetMixerParameter(data);
            string playerPrefKey = GetPlayerPrefKey(data);
            Slider slider = data.slider != null ? data.slider : FindSlider(playerPrefKey);

            float savedVolume = PlayerPrefs.GetFloat(playerPrefKey, GetDefaultVolume(playerPrefKey));
            ApplyVolume(mixerParameter, playerPrefKey, savedVolume);

            if (slider == null)
                continue;

            slider.onValueChanged.RemoveAllListeners();
            slider.SetValueWithoutNotify(savedVolume);

            slider.onValueChanged.AddListener(value => ApplyVolume(mixerParameter, playerPrefKey, value));
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

    private List<VolumeData> GetEffectiveVolumeSettings()
    {
        List<VolumeData> effectiveSettings = new List<VolumeData>();

        AddOrCompleteSetting(effectiveSettings, GlobalMixerParameter, GlobalVolumeKey);
        AddOrCompleteSetting(effectiveSettings, AmbientMixerParameter, AmbientVolumeKey);
        AddOrCompleteSetting(effectiveSettings, SfxMixerParameter, SfxVolumeKey);
        AddOrCompleteSetting(effectiveSettings, MusicMixerParameter, MusicVolumeKey);

        return effectiveSettings;
    }

    private void AddOrCompleteSetting(List<VolumeData> effectiveSettings, string mixerParameter, string playerPrefKey)
    {
        VolumeData setting = new VolumeData
        {
            mixerParameter = mixerParameter,
            playerPrefKey = playerPrefKey,
            slider = null
        };

        if (volumeSettings != null)
        {
            foreach (VolumeData configuredSetting in volumeSettings)
            {
                if (GetPlayerPrefKey(configuredSetting) != playerPrefKey)
                    continue;

                setting = configuredSetting;
                setting.mixerParameter = string.IsNullOrEmpty(setting.mixerParameter) ? mixerParameter : setting.mixerParameter;
                setting.playerPrefKey = string.IsNullOrEmpty(setting.playerPrefKey) ? playerPrefKey : setting.playerPrefKey;
                break;
            }
        }

        effectiveSettings.Add(setting);
    }

    private string GetMixerParameter(VolumeData data)
    {
        if (!string.IsNullOrEmpty(data.mixerParameter))
            return data.mixerParameter;

        return GetPlayerPrefKey(data) switch
        {
            MusicVolumeKey => MusicMixerParameter,
            SfxVolumeKey => SfxMixerParameter,
            AmbientVolumeKey => AmbientMixerParameter,
            _ => GlobalMixerParameter
        };
    }

    private string GetPlayerPrefKey(VolumeData data)
    {
        return string.IsNullOrEmpty(data.playerPrefKey) ? GlobalVolumeKey : data.playerPrefKey;
    }

    private Slider FindSlider(string playerPrefKey)
    {
        string sliderName = GetSliderName(playerPrefKey);
        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Slider slider in sliders)
        {
            if (slider.gameObject.scene == gameObject.scene && slider.gameObject.name == sliderName)
                return slider;
        }

        return null;
    }

    private string GetSliderName(string playerPrefKey)
    {
        return playerPrefKey switch
        {
            MusicVolumeKey => "sliderMusicVolume",
            SfxVolumeKey => "sliderSFXVolume",
            AmbientVolumeKey => "sliderAmbientVolume",
            _ => "sliderGlobalVolume"
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
