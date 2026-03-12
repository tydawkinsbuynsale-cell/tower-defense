#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace RobotTD.Editor
{
    /// <summary>
    /// Tools > Robot TD > Build Game Scene
    ///
    /// Creates the complete GameObject hierarchy required for a gameplay scene,
    /// attaching the correct components to each object. Run this once per new
    /// map scene you create.
    ///
    /// Also provides "Build Main Menu Scene" for the MainMenu scene.
    /// </summary>
    public class SceneHierarchyBuilder : EditorWindow
    {
        [MenuItem("Tools/Robot TD/Build Game Scene", priority = 20)]
        public static void ShowWindow()
        {
            GetWindow<SceneHierarchyBuilder>("Scene Builder");
        }

        // ── GUI ───────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Robot TD — Scene Hierarchy Builder", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            if (GUILayout.Button("Build Gameplay Scene", GUILayout.Height(40)))
                BuildGameplayScene();

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Build Main Menu Scene", GUILayout.Height(40)))
                BuildMainMenuScene();

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Each button creates the full hierarchy in the ACTIVE scene.\n" +
                "Run in a freshly created empty scene.", MessageType.Info);
        }

        // ══════════════════════════════════════════════════════════════════════
        // GAMEPLAY SCENE
        // ══════════════════════════════════════════════════════════════════════

        private static void BuildGameplayScene()
        {
            Undo.SetCurrentGroupName("Build Gameplay Scene");
            int group = Undo.GetCurrentGroup();

            // ── [_BOOTSTRAPPER] ───────────────────────────────────────────────
            var bootstrapper = CreateGO("_Bootstrapper");
            AddComp<Core.SceneBootstrapper>(bootstrapper);

            // ── [_MANAGERS] ───────────────────────────────────────────────────
            var managers = CreateGO("_Managers");

            var gmGO = CreateChild(managers, "GameManager");
            AddComp<Core.GameManager>(gmGO);

            var wmGO = CreateChild(managers, "WaveManager");
            AddComp<Core.WaveManager>(wmGO);

            var poolGO = CreateChild(managers, "ObjectPooler");
            AddComp<Core.ObjectPooler>(poolGO);

            var smGO = CreateChild(managers, "SaveManager");
            AddComp<Core.SaveManager>(smGO);

            var giGO = CreateChild(managers, "GameIntegrator");
            AddComp<Core.GameIntegrator>(giGO);

            var pmGO = CreateChild(managers, "PerformanceManager");
            AddComp<Core.PerformanceManager>(pmGO);

            var tplGO = CreateChild(managers, "TowerPlacementManager");
            AddComp<Towers.TowerPlacementManager>(tplGO);

            var endlessGO = CreateChild(managers, "EndlessMode");
            AddComp<Core.EndlessMode>(endlessGO);

            var tutGO = CreateChild(managers, "TutorialManager");
            AddComp<Tutorial.TutorialManager>(tutGO);

            // ── [_AUDIO] ──────────────────────────────────────────────────────
            var audioGO = CreateGO("AudioManager");
            AddComp<Audio.AudioManager>(audioGO);

            // ── [_VFX] ────────────────────────────────────────────────────────
            var vfxGO = CreateGO("VFXManager");
            AddComp<VFX.VFXManager>(vfxGO);

            // ── [_PROGRESSION] ────────────────────────────────────────────────
            var progGO = CreateGO("_Progression");
            var ttGO = CreateChild(progGO, "TechTree");
            AddComp<Progression.TechTree>(ttGO);
            var achGO = CreateChild(progGO, "AchievementManager");
            AddComp<Progression.AchievementManager>(achGO);

            // ── [MAP] ─────────────────────────────────────────────────────────
            var mapRoot = CreateGO("Map");
            var mmGO = CreateChild(mapRoot, "MapManager");
            AddComp<Map.MapManager>(mmGO);
            CreateChild(mapRoot, "Tiles");
            CreateChild(mapRoot, "Path_Waypoints");
            CreateChild(mapRoot, "SpawnPoint");

            // ── [TOWERS] ──────────────────────────────────────────────────────
            var towersRoot = CreateGO("Towers");

            // ── [ENEMIES] ─────────────────────────────────────────────────────
            var enemiesRoot = CreateGO("Enemies");

            // ── [PROJECTILES] ─────────────────────────────────────────────────
            var projRoot = CreateGO("Projectiles");

            // ── [CAMERA] ──────────────────────────────────────────────────────
            var camGO = CreateGO("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            AddComp<Map.CameraController>(camGO);
            AddComp<Map.InputManager>(camGO);
            AddComp<AudioListener>(camGO);

            // ── [UI] ──────────────────────────────────────────────────────────
            BuildGameplayUI();

            // ── [LIGHTING] ───────────────────────────────────────────────────
            var lightGO = CreateGO("Directional Light");
            var dl = lightGO.AddComponent<Light>();
            dl.type = LightType.Directional;
            dl.intensity = 1.0f;
            lightGO.transform.eulerAngles = new Vector3(50, -30, 0);

            Undo.CollapseUndoOperations(group);
            Debug.Log("[SceneHierarchyBuilder] Gameplay scene hierarchy built.");
            EditorUtility.DisplayDialog("Done!", "Gameplay scene hierarchy created.\nAssign ScriptableObject references in the Inspector.", "OK");
        }

        private static void BuildGameplayUI()
        {
            // Canvas
            var canvasGO = CreateGO("UI_Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            // HUD
            var hudGO = CreateChild(canvasGO, "GameHUD");
            AddComp<UI.GameHUD>(hudGO);

            // Pause Menu
            var pauseGO = CreateChild(canvasGO, "PauseMenu");
            AddComp<UI.PauseMenuUI>(pauseGO);
            pauseGO.SetActive(false);

            // Wave Result
            var waveResultGO = CreateChild(canvasGO, "WaveResultUI");
            AddComp<UI.WaveResultUI>(waveResultGO);
            waveResultGO.SetActive(false);

            // Tower Info
            var towerInfoGO = CreateChild(canvasGO, "TowerInfoUI");
            AddComp<UI.TowerInfoUI>(towerInfoGO);
            towerInfoGO.SetActive(false);

            // Tutorial Overlay
            var tutOverlay = CreateChild(canvasGO, "TutorialOverlay");
            tutOverlay.SetActive(false);
            // TutorialManager references are assigned manually in Inspector

            // Event System
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var esGO = CreateGO("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // MAIN MENU SCENE
        // ══════════════════════════════════════════════════════════════════════

        private static void BuildMainMenuScene()
        {
            Undo.SetCurrentGroupName("Build Main Menu Scene");
            int group = Undo.GetCurrentGroup();

            // Persistent managers (same subset needed in main menu)
            var managers = CreateGO("_Managers");
            AddComp<Core.SaveManager>(CreateChild(managers, "SaveManager"));
            AddComp<Core.PerformanceManager>(CreateChild(managers, "PerformanceManager"));
            AddComp<Progression.TechTree>(CreateChild(managers, "TechTree"));
            AddComp<Progression.AchievementManager>(CreateChild(managers, "AchievementManager"));

            var audioGO = CreateGO("AudioManager");
            AddComp<Audio.AudioManager>(audioGO);

            // Camera
            var camGO = CreateGO("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.04f, 0.12f);
            camGO.AddComponent<AudioListener>();

            // UI Canvas
            var canvasGO = CreateGO("UI_Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            var menuGO = CreateChild(canvasGO, "MainMenuUI");
            AddComp<UI.MainMenuUI>(menuGO);

            var settingsGO = CreateChild(canvasGO, "SettingsUI");
            AddComp<UI.SettingsUI>(settingsGO);
            settingsGO.SetActive(false);

            // Event System
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var esGO = CreateGO("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }

            // Bootstrapper
            AddComp<Core.SceneBootstrapper>(CreateGO("_Bootstrapper"));

            Undo.CollapseUndoOperations(group);
            Debug.Log("[SceneHierarchyBuilder] Main menu hierarchy built.");
            EditorUtility.DisplayDialog("Done!", "Main menu hierarchy created.\nAssign UI references in the Inspector.", "OK");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static GameObject CreateGO(string name)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            return go;
        }

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static T AddComp<T>(GameObject go) where T : Component
        {
            return Undo.AddComponent<T>(go);
        }

        private static GameObject FindFirstObjectByType<T>() where T : Component
        {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<T>()?.gameObject;
#else
            return UnityEngine.Object.FindObjectOfType<T>()?.gameObject;
#endif
        }
    }
}
#endif
