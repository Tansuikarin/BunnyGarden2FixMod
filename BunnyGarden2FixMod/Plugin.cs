using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
#if BIE6
using BepInEx.Unity.Mono;
#endif
using BunnyGarden2FixMod.Controllers;
using BunnyGarden2FixMod.Utils;
using GB;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BunnyGarden2FixMod;

public enum AntiAliasingType
{
    Off,
    FXAA,
    TAA,
    MSAA2x,
    MSAA4x,
    MSAA8x,
}

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static ConfigEntry<int> ConfigWidth;
    public static ConfigEntry<int> ConfigHeight;
    public static ConfigEntry<int> ConfigFrameRate;
    public static ConfigEntry<AntiAliasingType> ConfigAntiAliasing;
    public static ConfigEntry<float> ConfigSensitivity;
    public static ConfigEntry<float> ConfigSpeed;
    public static ConfigEntry<float> ConfigFastSpeed;
    public static ConfigEntry<float> ConfigSlowSpeed;
    public static ConfigEntry<bool> ConfigCheatEnabled;
    public static ConfigEntry<bool> ConfigDisableStockings;

    // --- 实时控制变量 ---
    public static bool isCheatActive = false;
    public static bool isDisableStockingsActive = false;

    private GameObject freeCamObject;
    private Camera freeCam;
    private Camera originalCam;
    private FreeCameraController controller;
    public static bool isFreeCamActive = false;
    public static bool isFixedFreeCam = false;

    internal new static ManualLogSource Logger;

    private void Awake()
    {
        ConfigWidth = Config.Bind(
            "Resolution",
            "Width",
            1920,
            "解像度の幅（横）を指定します");

        ConfigHeight = Config.Bind(
            "Resolution",
            "Height",
            1080,
            "解像度の高さ（縦）を指定します");

        ConfigFrameRate = Config.Bind(
            "Resolution",
            "FrameRate",
            60,
            "フレームレート上限を指定します。-1にすると上限を撤廃します。");

        ConfigAntiAliasing = Config.Bind(
            "AntiAliasing",
            "AntiAliasingType",
            AntiAliasingType.MSAA8x,
            "アンチエイリアシングの種類を指定します。右の方ほど画質が良くなりますが、動作が重くなります。Off / FXAA / TAA / MSAA2x / MSAA4x / MSAA8x");

        ConfigSensitivity = Config.Bind(
            "Camera",
            "Sensitivity",
            2f,
            "フリーカメラのマウス感度");

        ConfigSpeed = Config.Bind(
            "Camera",
            "Speed",
            10f,
            "フリーカメラの移動速度");

        ConfigFastSpeed = Config.Bind(
            "Camera",
            "FastSpeed",
            30f,
            "フリーカメラの高速移動速度（Shift）");

        ConfigSlowSpeed = Config.Bind(
            "Camera",
            "SlowSpeed",
            2.5f,
            "フリーカメラの低速移動速度（Ctrl）");

        ConfigDisableStockings = Config.Bind(
            "Appearance",
            "DisableStockings",
            false,
            "true にするとキャストのストッキングを非表示にします。");

        ConfigCheatEnabled = Config.Bind(
            "Cheat",
            "Enabled",
            false,
            "true にすると会話選択肢・ドリンク・フードの正解をゲーム内に表示します。");

        // 初始化实时变量
        isCheatActive = ConfigCheatEnabled.Value;
        isDisableStockingsActive = ConfigDisableStockings.Value;

        Logger = base.Logger;
        PatchLogger.Initialize(Logger);
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
        
        Patches.CameraZoomPatch.Initialize(gameObject);
        
        PatchLogger.LogInfo($"プラグイン起動: {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION}");
    }

    private void OnGUI()
    {
        // F5: 自由视角
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F5)
            ToggleFreeCam();

        // F6: 固定视角
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F6)
            ToggleFixedFreeCam();

        // F7: 实时开关作弊
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F7)
        {
            isCheatActive = !isCheatActive;
            PatchLogger.LogInfo($"正解表示チート: {(isCheatActive ? "ON" : "OFF")}");
        }

        // F8: 实时开关丝袜移除
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F8)
        {
            isDisableStockingsActive = !isDisableStockingsActive;
            PatchLogger.LogInfo($"ストッキング無効化: {(isDisableStockingsActive ? "ON" : "OFF")}");
        }

        // 界面状态绘制
        if (isFreeCamActive)
        {
            if (isFixedFreeCam)
            {
                GUI.color = Color.yellow;
                GUI.Label(new Rect(10, 40, 500, 30), "Fixed Free Camera Mode: ON (F6=TOGGLE)");
            }
            GUI.color = Color.green;
            GUI.Label(new Rect(10, 10, 500, 30), "Free Camera: ON (F5=OFF, Arrow/WASD=Move, E/Q=UpDown)");
        }

        if (isCheatActive)
        {
            GUI.color = Color.cyan;
            GUI.Label(new Rect(10, 70, 500, 30), "Cheat Display: ON (F7=TOGGLE)");
        }

        if (isDisableStockingsActive)
        {
            GUI.color = Color.magenta;
            GUI.Label(new Rect(10, 100, 500, 30), "Disable Stockings: ON (F8=TOGGLE)");
        }

        GUI.color = Color.white;
    }

    private void ToggleFreeCam()
    {
        isFreeCamActive = !isFreeCamActive;
        if (isFreeCamActive)
            CreateFreeCam();
        else
        {
            DestroyFreeCam();
            isFixedFreeCam = false;
        }
        PatchLogger.LogInfo($"フリーカメラ: {(isFreeCamActive ? "ON" : "OFF")}");
    }

    private void ToggleFixedFreeCam()
    {
        if (isFreeCamActive)
        {
            isFixedFreeCam = !isFixedFreeCam;
            PatchLogger.LogInfo($"フリーカメラ固定モード: {(isFixedFreeCam ? "ON" : "OFF")}");
        }
    }

    private void CreateFreeCam()
    {
        var allCameras = Camera.allCameras;
        originalCam = Camera.main;
        if (originalCam == null)
        {
            foreach (var cam in allCameras)
            {
                if (originalCam == null || cam.depth > originalCam.depth)
                    originalCam = cam;
            }
            if (originalCam == null) return;
        }

        freeCamObject = new GameObject("BG2FreeCam");
        freeCam = freeCamObject.AddComponent<Camera>();
        freeCam.CopyFrom(originalCam);
        freeCamObject.transform.SetPositionAndRotation(
            originalCam.transform.position,
            originalCam.transform.rotation);

        CopyUrpCameraData(originalCam, freeCam);

        controller = freeCamObject.AddComponent<FreeCameraController>();
        freeCamObject.AddComponent<AudioListener>();

        originalCam.enabled = false;
        var originalListener = originalCam.GetComponent<AudioListener>();
        if (originalListener != null)
            originalListener.enabled = false;
    }

    private static void CopyUrpCameraData(Camera src, Camera dst)
    {
        var srcData = src.GetUniversalAdditionalCameraData();
        var dstData = dst.GetUniversalAdditionalCameraData();
        if (srcData == null || dstData == null) return;

        dstData.renderPostProcessing  = srcData.renderPostProcessing;
        dstData.antialiasing          = srcData.antialiasing;
        dstData.antialiasingQuality   = srcData.antialiasingQuality;
        dstData.stopNaN               = srcData.stopNaN;
        dstData.dithering             = srcData.dithering;
        dstData.renderShadows         = srcData.renderShadows;
        dstData.volumeLayerMask       = srcData.volumeLayerMask;
        dstData.volumeTrigger         = srcData.volumeTrigger;
    }

    private void DestroyFreeCam()
    {
        if (freeCamObject != null)
        {
            Destroy(freeCamObject);
            freeCamObject = null;
            freeCam = null;
            controller = null;
        }

        if (originalCam != null)
        {
            originalCam.enabled = true;
            var originalListener = originalCam.GetComponent<AudioListener>();
            if (originalListener != null)
                originalListener.enabled = true;
        }
    }
}

[HarmonyPatch(typeof(GBSystem), "IsInputDisabled")]
public class FreeCamInputDisablePatch
{
    private static void Postfix(ref bool __result)
    {
        if (Plugin.isFreeCamActive && !Plugin.isFixedFreeCam)
            __result = true;
    }
}
