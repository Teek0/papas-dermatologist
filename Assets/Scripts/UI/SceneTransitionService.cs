using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public static class SceneTransitionService
{
    private const string MasterMixerParameter = "masterMix";
    private const float MinLinearVolume = 0.0001f;

    public static IEnumerator FadeOutAndLoadScene(
        string sceneName,
        CanvasGroup sceneGroup,
        AudioMixer mixer,
        float duration)
    {
        yield return FadeOutAndLoadScene(sceneName, sceneGroup, 0f, false, mixer, duration);
    }

    public static IEnumerator FadeOutAndLoadScene(
        string sceneName,
        CanvasGroup sceneGroup,
        float targetAlpha,
        bool interactable,
        AudioMixer mixer,
        float duration)
    {
        yield return FadeOut(sceneGroup, targetAlpha, interactable, mixer, duration);
        LoadSceneAndResetMixer(sceneName, mixer);
    }

    public static IEnumerator FadeOut(
        CanvasGroup sceneGroup,
        float targetAlpha,
        bool interactable,
        AudioMixer mixer,
        float duration)
    {
        bool hasSceneGroup = sceneGroup != null;
        bool hasMixer = mixer != null;

        if (!hasSceneGroup && !hasMixer)
            yield break;

        float startAlpha = hasSceneGroup ? sceneGroup.alpha : 0f;
        float startLinearVolume = 1f;

        if (hasMixer && mixer.GetFloat(MasterMixerParameter, out float startVolumeDb))
            startLinearVolume = Mathf.Pow(10f, startVolumeDb / 20f);

        if (duration <= 0f)
        {
            ApplySceneGroupState(sceneGroup, targetAlpha, interactable);
            SetMixerLinearVolume(mixer, 0f);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float percentage = Mathf.Clamp01(elapsed / duration);

            if (hasSceneGroup)
                sceneGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, percentage);

            if (hasMixer)
            {
                float currentLinear = Mathf.Lerp(startLinearVolume, 0f, percentage);
                SetMixerLinearVolume(mixer, currentLinear);
            }

            yield return null;
        }

        ApplySceneGroupState(sceneGroup, targetAlpha, interactable);
    }

    public static void ResetMasterMixerVolume(AudioMixer mixer)
    {
        if (mixer != null)
            mixer.SetFloat(MasterMixerParameter, 0f);
    }

    private static void LoadSceneAndResetMixer(string sceneName, AudioMixer mixer)
    {
        if (mixer != null)
        {
            void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                ResetMasterMixerVolume(mixer);
            }

            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        SceneManager.LoadScene(sceneName);
    }

    private static void ApplySceneGroupState(CanvasGroup sceneGroup, float alpha, bool interactable)
    {
        if (sceneGroup == null)
            return;

        sceneGroup.alpha = alpha;
        sceneGroup.blocksRaycasts = interactable;
        sceneGroup.interactable = interactable;
    }

    private static void SetMixerLinearVolume(AudioMixer mixer, float linearVolume)
    {
        if (mixer == null)
            return;

        float targetDb = Mathf.Log10(Mathf.Clamp(linearVolume, MinLinearVolume, 1f)) * 20f;
        mixer.SetFloat(MasterMixerParameter, targetDb);
    }
}
