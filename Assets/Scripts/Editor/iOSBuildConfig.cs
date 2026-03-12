using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

namespace RobotTowerDefense.Editor
{
    /// <summary>
    /// iOS build configuration and automation tool.
    /// Provides a comprehensive editor window for configuring iOS builds with all necessary settings.
    /// Ensures proper setup for App Store submission and TestFlight distribution.
    /// </summary>
    public class iOSBuildConfig : EditorWindow
    {
        #region Editor Window Setup

        private static iOSBuildConfig window;
        private Vector2 scrollPosition;

        // Tab selection
        private int selectedTab = 0;
        private string[] tabNames = { "Build Settings", "App Icons", "Splash Screen", "Capabilities", "Quick Actions" };

        [MenuItem("Tools/Robot Tower Defense/iOS Build Configuration")]
        public static void ShowWindow()
        {
            window = GetWindow<iOSBuildConfig>("iOS Build Config");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        #endregion

        #region Build Settings

        // Identity
        private string bundleIdentifier = "com.yourstudio.robottowerdefense";
        private string version = "1.0";
        private string buildNumber = "1";

        // Signing
        private string teamID = "";
        private ProvisioningProfileType provisioningProfile = ProvisioningProfileType.Automatic;
        private bool automaticSigning = true;

        // Optimization
        private bool stripEngineCode = true;
        private ScriptCallOptimizationLevel scriptCallOptimization = ScriptCallOptimizationLevel.SlowAndSafe;
        private bool enableBitcode = false; // Deprecated by Apple

        // Architecture
        private iOSTargetDevice targetDevice = iOSTargetDevice.iPhoneAndiPad;
        private string targetOSVersion = "13.0";

        // Graphics
        private bool metalEditorSupport = true;
        private bool metalAPIValidation = false; // For debugging only

        // Orientation
        private UIOrientation defaultOrientation = UIOrientation.LandscapeLeft;
        private bool allowLandscapeLeft = true;
        private bool allowLandscapeRight = true;
        private bool allowPortrait = false;
        private bool allowPortraitUpsideDown = false;

        // Performance
        private int targetFrameRate = 60;
        private bool accelerometerFrequency = false;

        // Privacy
        private string cameraUsageDescription = "This app does not use the camera.";
        private string microphoneUsageDescription = "This app does not use the microphone.";
        private string locationUsageDescription = "This app does not use location services.";

        #endregion

        #region GUI Drawing

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();

            EditorGUILayout.Space(10);

            // Tab selection
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

            EditorGUILayout.Space(10);

            switch (selectedTab)
            {
                case 0: DrawBuildSettingsTab(); break;
                case 1: DrawAppIconsTab(); break;
                case 2: DrawSplashScreenTab(); break;
                case 3: DrawCapabilitiesTab(); break;
                case 4: DrawQuickActionsTab(); break;
            }

            EditorGUILayout.Space(20);

            DrawActionButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("iOS Build Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure all iOS build settings for App Store submission. " +
                "Ensure all required settings are filled before building.",
                MessageType.Info);
        }

        private void DrawBuildSettingsTab()
        {
            EditorGUILayout.LabelField("App Identity", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            bundleIdentifier = EditorGUILayout.TextField("Bundle Identifier", bundleIdentifier);
            version = EditorGUILayout.TextField("Version", version);
            buildNumber = EditorGUILayout.TextField("Build Number", buildNumber);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Code Signing", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            automaticSigning = EditorGUILayout.Toggle("Automatic Signing", automaticSigning);
            if (!automaticSigning)
            {
                teamID = EditorGUILayout.TextField("Team ID", teamID);
                provisioningProfile = (ProvisioningProfileType)EditorGUILayout.EnumPopup("Provisioning Profile", provisioningProfile);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Device Compatibility", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            targetDevice = (iOSTargetDevice)EditorGUILayout.EnumPopup("Target Device", targetDevice);
            targetOSVersion = EditorGUILayout.TextField("Min iOS Version", targetOSVersion);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Orientation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            defaultOrientation = (UIOrientation)EditorGUILayout.EnumPopup("Default Orientation", defaultOrientation);
            EditorGUILayout.LabelField("Allowed Orientations:");
            EditorGUI.indentLevel++;
            allowLandscapeLeft = EditorGUILayout.Toggle("Landscape Left", allowLandscapeLeft);
            allowLandscapeRight = EditorGUILayout.Toggle("Landscape Right", allowLandscapeRight);
            allowPortrait = EditorGUILayout.Toggle("Portrait", allowPortrait);
            allowPortraitUpsideDown = EditorGUILayout.Toggle("Portrait Upside Down", allowPortraitUpsideDown);
            EditorGUI.indentLevel -= 2;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Optimization", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            stripEngineCode = EditorGUILayout.Toggle("Strip Engine Code", stripEngineCode);
            scriptCallOptimization = (ScriptCallOptimizationLevel)EditorGUILayout.EnumPopup("Script Call Optimization", scriptCallOptimization);
            metalEditorSupport = EditorGUILayout.Toggle("Metal Editor Support", metalEditorSupport);
            metalAPIValidation = EditorGUILayout.Toggle("Metal API Validation", metalAPIValidation);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Performance", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            targetFrameRate = EditorGUILayout.IntSlider("Target Frame Rate", targetFrameRate, 30, 120);
            EditorGUI.indentLevel--;
        }

        private void DrawAppIconsTab()
        {
            EditorGUILayout.LabelField("App Icons", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "iOS requires multiple icon sizes:\n" +
                "• 180x180 (iPhone)\n" +
                "• 120x120 (iPhone)\n" +
                "• 167x167 (iPad Pro)\n" +
                "• 152x152 (iPad)\n" +
                "• 1024x1024 (App Store)\n\n" +
                "Configure icons in: Player Settings > iOS > Icon",
                MessageType.Info);

            if (GUILayout.Button("Open Player Settings - Icon"))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Icon Validation", EditorStyles.boldLabel);
            if (GUILayout.Button("Validate Icon Sizes"))
            {
                ValidateAppIcons();
            }
        }

        private void DrawSplashScreenTab()
        {
            EditorGUILayout.LabelField("Splash Screen", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure splash screen images for different devices:\n" +
                "• Launch Storyboard (recommended for iOS 13+)\n" +
                "• Or static launch images for older devices\n\n" +
                "Configure in: Player Settings > iOS > Splash Image",
                MessageType.Info);

            if (GUILayout.Button("Open Player Settings - Splash"))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }
        }

        private void DrawCapabilitiesTab()
        {
            EditorGUILayout.LabelField("iOS Capabilities", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Required capabilities for Robot Tower Defense:\n" +
                "• Game Center (leaderboards, achievements)\n" +
                "• In-App Purchase\n" +
                "• Push Notifications (optional)\n\n" +
                "Configure in Xcode after build.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Privacy Descriptions", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("Required if you use these features. Leave default if not used.", MessageType.None);
            cameraUsageDescription = EditorGUILayout.TextField("Camera Usage", cameraUsageDescription);
            microphoneUsageDescription = EditorGUILayout.TextField("Microphone Usage", microphoneUsageDescription);
            locationUsageDescription = EditorGUILayout.TextField("Location Usage", locationUsageDescription);
            EditorGUI.indentLevel--;
        }

        private void DrawQuickActionsTab()
        {
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Apply Recommended Settings"))
            {
                ApplyRecommendedSettings();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Increment Build Number"))
            {
                IncrementBuildNumber();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Validate All Settings"))
            {
                ValidateAllSettings();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Export Build Settings JSON"))
            {
                ExportBuildSettingsJSON();
            }
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply Settings", GUILayout.Height(40)))
            {
                ApplySettings();
            }

            if (GUILayout.Button("Build iOS Xcode Project", GUILayout.Height(40)))
            {
                ApplySettings();
                BuildiOSProject();
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Apply Settings

        private void ApplySettings()
        {
            // Identity
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundleIdentifier);
            PlayerSettings.bundleVersion = version;
            PlayerSettings.iOS.buildNumber = buildNumber;

            // Architecture
            PlayerSettings.iOS.targetDevice = targetDevice;
            PlayerSettings.iOS.targetOSVersionString = targetOSVersion;

            // Optimization
            PlayerSettings.stripEngineCode = stripEngineCode;
            PlayerSettings.iOS.scriptCallOptimization = scriptCallOptimization;

            // Graphics
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new UnityEngine.Rendering.GraphicsDeviceType[] {
                UnityEngine.Rendering.GraphicsDeviceType.Metal
            });
            PlayerSettings.iOS.forceHardShadowsOnMetal = false;

            // Orientation
            PlayerSettings.defaultInterfaceOrientation = defaultOrientation;
            PlayerSettings.allowedAutorotateToLandscapeLeft = allowLandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeRight = allowLandscapeRight;
            PlayerSettings.allowedAutorotateToPortrait = allowPortrait;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = allowPortraitUpsideDown;

            // Performance
            Application.targetFrameRate = targetFrameRate;

            // Signing
            PlayerSettings.iOS.appleEnableAutomaticSigning = automaticSigning;
            if (!automaticSigning)
            {
                PlayerSettings.iOS.appleDeveloperTeamID = teamID;
                PlayerSettings.iOS.iOSManualProvisioningProfileType = provisioningProfile;
            }

            // Capabilities - Camera/Microphone usage descriptions
            PlayerSettings.iOS.cameraUsageDescription = cameraUsageDescription;

            Debug.Log("✅ iOS build settings applied successfully!");
            EditorUtility.DisplayDialog("Success", "iOS build settings applied successfully!", "OK");
        }

        #endregion

        #region Build Functions

        private void BuildiOSProject()
        {
            string buildPath = EditorUtility.SaveFolderPanel("Choose iOS Build Location", "", "");
            if (string.IsNullOrEmpty(buildPath))
            {
                Debug.LogWarning("Build cancelled.");
                return;
            }

            string[] scenes = GetEnabledScenes();

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            };

            Debug.Log($"🚀 Starting iOS Xcode project build to: {buildPath}");

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"✅ iOS build succeeded! Size: {summary.totalSize / (1024 * 1024)} MB");
                EditorUtility.DisplayDialog("Build Successful", 
                    $"iOS Xcode project created successfully!\n\nLocation: {buildPath}\n\nOpen in Xcode to build for device or simulator.", 
                    "OK");
            }
            else
            {
                Debug.LogError($"❌ iOS build failed! Result: {summary.result}");
                EditorUtility.DisplayDialog("Build Failed", $"iOS build failed!\n\nResult: {summary.result}\n\nCheck Console for errors.", "OK");
            }
        }

        private string[] GetEnabledScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }
            return scenes.ToArray();
        }

        #endregion

        #region Validation

        private void ValidateAppIcons()
        {
            bool allValid = true;
            string report = "App Icon Validation:\n\n";

            // Check if icons are assigned
            var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.iOS, IconKind.Application);
            if (icons == null || icons.Length == 0)
            {
                report += "❌ No app icons assigned!\n";
                allValid = false;
            }
            else
            {
                report += $"✅ {icons.Length} app icons assigned\n";
            }

            if (allValid)
            {
                report += "\n✅ All icon requirements met!";
                EditorUtility.DisplayDialog("Icon Validation", report, "OK");
            }
            else
            {
                report += "\n⚠️ Please assign app icons in Player Settings > iOS > Icon";
                EditorUtility.DisplayDialog("Icon Validation", report, "OK");
            }

            Debug.Log(report);
        }

        private void ValidateAllSettings()
        {
            bool allValid = true;
            string report = "iOS Settings Validation:\n\n";

            // Bundle identifier
            if (string.IsNullOrEmpty(bundleIdentifier) || bundleIdentifier == "com.yourstudio.robottowerdefense")
            {
                report += "⚠️ Set a unique Bundle Identifier\n";
                allValid = false;
            }
            else
            {
                report += "✅ Bundle Identifier: " + bundleIdentifier + "\n";
            }

            // Version
            if (string.IsNullOrEmpty(version))
            {
                report += "❌ Version is required\n";
                allValid = false;
            }
            else
            {
                report += "✅ Version: " + version + "\n";
            }

            // Build number
            if (string.IsNullOrEmpty(buildNumber))
            {
                report += "❌ Build Number is required\n";
                allValid = false;
            }
            else
            {
                report += "✅ Build Number: " + buildNumber + "\n";
            }

            // Team ID (if manual signing)
            if (!automaticSigning && string.IsNullOrEmpty(teamID))
            {
                report += "⚠️ Team ID required for manual signing\n";
                allValid = false;
            }

            // Orientation
            if (!allowLandscapeLeft && !allowLandscapeRight && !allowPortrait && !allowPortraitUpsideDown)
            {
                report += "❌ At least one orientation must be allowed\n";
                allValid = false;
            }
            else
            {
                report += "✅ Orientation settings configured\n";
            }

            // Target OS version
            if (string.IsNullOrEmpty(targetOSVersion))
            {
                report += "❌ Target iOS version required\n";
                allValid = false;
            }
            else
            {
                report += "✅ Minimum iOS Version: " + targetOSVersion + "\n";
            }

            if (allValid)
            {
                report += "\n✅ All settings validated! Ready to build.";
            }
            else
            {
                report += "\n⚠️ Please fix the issues above before building.";
            }

            EditorUtility.DisplayDialog("Settings Validation", report, "OK");
            Debug.Log(report);
        }

        #endregion

        #region Helper Functions

        private void ApplyRecommendedSettings()
        {
            // Tower Defense game recommended settings
            targetDevice = iOSTargetDevice.iPhoneAndiPad;
            targetOSVersion = "13.0"; // iOS 13+ for modern features
            defaultOrientation = UIOrientation.LandscapeLeft;
            allowLandscapeLeft = true;
            allowLandscapeRight = true;
            allowPortrait = false;
            allowPortraitUpsideDown = false;
            stripEngineCode = true;
            scriptCallOptimization = ScriptCallOptimizationLevel.FastButNoExceptions;
            metalEditorSupport = true;
            metalAPIValidation = false;
            targetFrameRate = 60;
            automaticSigning = true;

            Debug.Log("✅ Applied recommended settings for tower defense game");
            EditorUtility.DisplayDialog("Recommended Settings", "Applied recommended iOS settings for tower defense game.", "OK");
        }

        private void IncrementBuildNumber()
        {
            if (int.TryParse(buildNumber, out int currentBuild))
            {
                buildNumber = (currentBuild + 1).ToString();
                PlayerSettings.iOS.buildNumber = buildNumber;
                Debug.Log($"✅ Build number incremented to {buildNumber}");
            }
            else
            {
                Debug.LogError("Build number must be a valid integer!");
            }
        }

        private void ExportBuildSettingsJSON()
        {
            string json = JsonUtility.ToJson(new BuildSettings
            {
                bundleIdentifier = this.bundleIdentifier,
                version = this.version,
                buildNumber = this.buildNumber,
                teamID = this.teamID,
                targetOSVersion = this.targetOSVersion,
                targetDevice = this.targetDevice.ToString(),
                automaticSigning = this.automaticSigning
            }, true);

            string path = EditorUtility.SaveFilePanel("Export Build Settings", "", "iOS_BuildSettings", "json");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, json);
                Debug.Log($"✅ Build settings exported to {path}");
                EditorUtility.DisplayDialog("Export Successful", $"Build settings exported to:\n{path}", "OK");
            }
        }

        #endregion

        #region Data Classes

        [System.Serializable]
        private class BuildSettings
        {
            public string bundleIdentifier;
            public string version;
            public string buildNumber;
            public string teamID;
            public string targetOSVersion;
            public string targetDevice;
            public bool automaticSigning;
        }

        #endregion
    }
}
