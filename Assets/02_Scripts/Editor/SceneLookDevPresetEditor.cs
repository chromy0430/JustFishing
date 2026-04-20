using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SceneLookDevPresetEditor : EditorWindow
{
    [MenuItem("Tools/Environment/Apply LookDev Preset")]
    public static void ShowWindow()
    {
        GetWindow<SceneLookDevPresetEditor>("LookDev Preset");
    }

    private void OnGUI()
    {
        GUILayout.Label("씬 환경 원클릭 세팅", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("적용하기 (Light + GI + PostProcessing)", GUILayout.Height(40)))
        {
            ApplyDirectionalLightSettings();
            ApplyGISettings();
            ApplyPostProcessingSettings();

            // 변경 사항을 에디터에 알림 (저장 가능 상태로 전환)
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("LookDev 프리셋 적용이 완료되었습니다.");
        }
    }

    private void ApplyDirectionalLightSettings()
    {
        Light dirLight = FindAnyObjectByType<Light>(FindObjectsInactive.Exclude);

        if (dirLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            dirLight = lightObj.AddComponent<Light>();
            dirLight.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        if (dirLight.type == LightType.Directional)
        {
            dirLight.shadows = LightShadows.Soft;
            dirLight.shadowStrength = 0.6f;
            Debug.Log("[Light] Directional Light 세팅 완료 (Shadow Strength: 0.6, Soft Shadows)");
        }
    }

    private void ApplyGISettings()
    {
        // Baked GI 및 Lightmap 세팅
        Lightmapping.bakedGI = true;
        Lightmapping.realtimeGI = false;

        // Environment Lighting
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.5f; // Indirect Intensity ↑ (1.2 ~ 2.0 사이의 중간값 적용)

        Debug.Log("[GI] Baked GI 활성화 및 Indirect Intensity 1.5 적용 완료");
    }

    private void ApplyPostProcessingSettings()
    {
        Volume globalVolume = FindAnyObjectByType<Volume>(FindObjectsInactive.Exclude);

        if (globalVolume == null)
        {
            GameObject volumeObj = new GameObject("Global PostProcessing Volume");
            globalVolume = volumeObj.AddComponent<Volume>();
            globalVolume.isGlobal = true;
        }

        // 새 프로파일 생성 및 할당 (기존 프로파일 덮어쓰기 방지를 위해 새로 인스턴스화하거나 기존 것에 오버라이드)
        VolumeProfile profile = globalVolume.HasInstantiatedProfile() ? globalVolume.profile : ScriptableObject.CreateInstance<VolumeProfile>();
        globalVolume.profile = profile;

        // 1. Tonemapping (ACES)
        if (!profile.TryGet(out Tonemapping tonemapping))
        {
            tonemapping = profile.Add<Tonemapping>();
        }
        tonemapping.mode.Override(TonemappingMode.ACES);

        // 2. Color Adjustments (Saturation ↑, Contrast 약간 ↑)
        if (!profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            colorAdjustments = profile.Add<ColorAdjustments>();
        }
        colorAdjustments.saturation.Override(15f); // Saturation 증가
        colorAdjustments.contrast.Override(10f);   // Contrast 증가

        // 3. Bloom (약하게)
        if (!profile.TryGet(out Bloom bloom))
        {
            bloom = profile.Add<Bloom>();
        }
        bloom.intensity.Override(0.5f); // 약한 Bloom
        bloom.threshold.Override(1.0f);

        Debug.Log("[PostProcessing] ACES, Saturation, Contrast, Bloom 세팅 완료");
    }
}