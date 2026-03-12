#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System.IO;

namespace RobotTD.Editor
{
    /// <summary>
    /// Editor window to configure Android build settings for Play Store submission.
    /// Open via: Tools > Robot TD > Android Build Config
    /// </summary>
    public class AndroidBuildConfig : EditorWindow
    {
        // App identity
        private string bundleId     = "com.yourstudio.robottowerdefense";
        private string productName  = "Robot Tower Defense";
        private string version      = "1.0.0";
        private int    bundleVersion = 1;

        // Build
        private bool   developmentBuild = false;
        private bool   armv7 = true;
        private bool   arm64 = true;
        private bool   splitAPKs = false;
        private bool   buildAAB  = true;   // Required for Play Store
        private ScriptingImplementation backend = ScriptingImplementation.IL2CPP;
        private AndroidSdkVersions minSDK = AndroidSdkVersions.AndroidApiLevel24; // Android 7
        private AndroidSdkVersions targetSDK = AndroidSdkVersions.AndroidApiLevel34;

        // Graphics
        private bool   openGLES3 = true;
        private bool   vulkan    = true;
        private bool   gpuSkinning = true;

        // Optimization
        private bool   stripEngineCode = true;
        private ManagedStrippingLevel strippingLevel = ManagedStrippingLevel.Medium;
        private bool   optimizeMesh = true;

        private Vector2 scroll;
        private GUIStyle headerStyle;
        private bool stylesInitialized;

        [MenuItem("Tools/Robot TD/Android Build Config")]
        public static void ShowWindow()
        {
            var win = GetWindow<AndroidBuildConfig>("Android Build Config");
            win.minSize = new Vector2(420, 600);
            win.LoadCurrentSettings();
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                margin = new RectOffset(0, 0, 8, 4)
            };
            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();
            scroll = EditorGUILayout.BeginScrollView(scroll);

            EditorGUILayout.Space(6);
            GUILayout.Label("Robot Tower Defense — Android Config", headerStyle);
            EditorGUILayout.HelpBox("Configure all Android build settings, then click Apply.", MessageType.Info);
            EditorGUILayout.Space(6);

            // ── App Identity ────────────────────────────────────────────────
            GUILayout.Label("App Identity", headerStyle);
            bundleId    = EditorGUILayout.TextField("Bundle ID",     bundleId);
            productName = EditorGUILayout.TextField("Product Name",  productName);
            version     = EditorGUILayout.TextField("Version",       version);
            bundleVersion = EditorGUILayout.IntField("Bundle Version Code", bundleVersion);

            EditorGUILayout.Space(6);

            // ── Build Options ────────────────────────────────────────────────
            GUILayout.Label("Build Options", headerStyle);
            developmentBuild = EditorGUILayout.Toggle("Development Build", developmentBuild);
            buildAAB  = EditorGUILayout.Toggle("Build AAB (Play Store)", buildAAB);
            splitAPKs = EditorGUILayout.Toggle("Split APKs by ABI",      splitAPKs);
            backend   = (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", backend);

            EditorGUILayout.Space(6);

            // ── Platform ──────────────────────────────────────────────────────
            GUILayout.Label("Platform", headerStyle);
            armv7     = EditorGUILayout.Toggle("ARMv7",   armv7);
            arm64     = EditorGUILayout.Toggle("ARM64",   arm64);
            minSDK    = (AndroidSdkVersions)EditorGUILayout.EnumPopup("Min SDK",    minSDK);
            targetSDK = (AndroidSdkVersions)EditorGUILayout.EnumPopup("Target SDK", targetSDK);

            EditorGUILayout.Space(6);

            // ── Graphics ──────────────────────────────────────────────────────
            GUILayout.Label("Graphics APIs", headerStyle);
            openGLES3  = EditorGUILayout.Toggle("OpenGL ES 3",    openGLES3);
            vulkan     = EditorGUILayout.Toggle("Vulkan",         vulkan);
            gpuSkinning = EditorGUILayout.Toggle("GPU Skinning",  gpuSkinning);

            EditorGUILayout.Space(6);

            // ── Optimization ─────────────────────────────────────────────────
            GUILayout.Label("Optimization", headerStyle);
            stripEngineCode = EditorGUILayout.Toggle("Strip Engine Code", stripEngineCode);
            strippingLevel  = (ManagedStrippingLevel)EditorGUILayout.EnumPopup("Managed Stripping", strippingLevel);
            optimizeMesh    = EditorGUILayout.Toggle("Optimize Mesh Data", optimizeMesh);

            EditorGUILayout.Space(10);

            // ── Actions ───────────────────────────────────────────────────────
            if (GUILayout.Button("Apply Settings", GUILayout.Height(36)))
                ApplySettings();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build APK", GUILayout.Height(30)))
                BuildAndroid(false);
            if (GUILayout.Button("Build AAB", GUILayout.Height(30)))
                BuildAndroid(true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Generate icons placeholder info", GUILayout.Height(28)))
                ShowIconInfo();

            EditorGUILayout.Space(6);
            EditorGUILayout.EndScrollView();
        }

        // ── Apply ────────────────────────────────────────────────────────────

        private void ApplySettings()
        {
            PlayerSettings.applicationIdentifier = bundleId;
            PlayerSettings.productName           = productName;
            PlayerSettings.bundleVersion         = version;
            PlayerSettings.Android.bundleVersionCode = bundleVersion;

            // Scripting backend & IL2CPP
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, backend);

            // ARM targets
            AndroidArchitecture arch = AndroidArchitecture.None;
            if (armv7) arch |= AndroidArchitecture.ARMv7;
            if (arm64) arch |= AndroidArchitecture.ARM64;
            PlayerSettings.Android.targetArchitectures = arch;

            // SDK versions
            PlayerSettings.Android.minSdkVersion    = minSDK;
            PlayerSettings.Android.targetSdkVersion = targetSDK;

            // Graphics APIs
            var apis = new System.Collections.Generic.List<UnityEngine.Rendering.GraphicsDeviceType>();
            if (vulkan)    apis.Add(UnityEngine.Rendering.GraphicsDeviceType.Vulkan);
            if (openGLES3) apis.Add(UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, apis.ToArray());
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);

            // GPU skinning
            PlayerSettings.gpuSkinning = gpuSkinning;

            // Stripping
            PlayerSettings.stripEngineCode = stripEngineCode;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, strippingLevel);

            // Mesh optimisation
            PlayerSettings.optimizeMeshData = optimizeMesh;

            // AAB
            EditorUserBuildSettings.buildAppBundle = buildAAB;
            EditorUserBuildSettings.androidCreateSymbols = developmentBuild
                ? AndroidCreateSymbols.Debugging
                : AndroidCreateSymbols.Disabled;

            AssetDatabase.SaveAssets();
            Debug.Log("[AndroidBuildConfig] Settings applied.");
            EditorUtility.DisplayDialog("Done", "Android build settings applied!", "OK");
        }

        private void LoadCurrentSettings()
        {
            bundleId      = PlayerSettings.applicationIdentifier;
            productName   = PlayerSettings.productName;
            version       = PlayerSettings.bundleVersion;
            bundleVersion = PlayerSettings.Android.bundleVersionCode;
            backend       = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            minSDK        = PlayerSettings.Android.minSdkVersion;
            targetSDK     = PlayerSettings.Android.targetSdkVersion;
            gpuSkinning   = PlayerSettings.gpuSkinning;
            stripEngineCode = PlayerSettings.stripEngineCode;
            buildAAB      = EditorUserBuildSettings.buildAppBundle;
        }

        private void BuildAndroid(bool aab)
        {
            ApplySettings();
            EditorUserBuildSettings.buildAppBundle = aab;

            string ext = aab ? ".aab" : ".apk";
            string path = EditorUtility.SaveFilePanel("Build Location", "", productName, ext.TrimStart('.'));
            if (string.IsNullOrEmpty(path)) return;

            var options = new BuildPlayerOptions
            {
                scenes = GetSceneList(),
                locationPathName = path,
                target = BuildTarget.Android,
                options = developmentBuild
                    ? BuildOptions.Development | BuildOptions.AllowDebugging
                    : BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[AndroidBuildConfig] Build result: {report.summary.result}");
        }

        private string[] GetSceneList()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (var s in EditorBuildSettings.scenes)
                if (s.enabled) scenes.Add(s.path);
            return scenes.ToArray();
        }

        private void ShowIconInfo()
        {
            EditorUtility.DisplayDialog("App Icon Sizes Required",
                "Android adaptive icons:\n" +
                "• Foreground: 432×432 px\n" +
                "• Background: 432×432 px\n\n" +
                "Legacy icons:\n" +
                "• xxxhdpi: 192×192\n" +
                "• xxhdpi:  144×144\n" +
                "• xhdpi:   96×96\n" +
                "• hdpi:    72×72\n" +
                "• mdpi:    48×48\n\n" +
                "Place in: Assets/Art/Icons/",
                "OK");
        }
    }
}
#endif
