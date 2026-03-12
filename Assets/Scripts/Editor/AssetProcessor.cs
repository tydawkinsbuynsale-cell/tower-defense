#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace RobotTD.Editor
{
    /// <summary>
    /// Batch asset processing tool for setting up sprites, textures, and icons
    /// with proper import settings for mobile tower defense game.
    /// 
    /// Opens via: Tools > Robot TD > Asset Processor
    /// </summary>
    public class AssetProcessor : EditorWindow
    {
        private Vector2 scrollPosition;
        private string spriteFolderPath = "Assets/Sprites";
        private string iconFolderPath = "Assets/UI/Icons";
        private string particleFolderPath = "Assets/VFX/Textures";

        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;

        // Sprite import settings
        private int spritePixelsPerUnit = 100;
        private FilterMode spriteFilterMode = FilterMode.Bilinear;
        private TextureImporterCompression spriteCompression = TextureImporterCompression.Compressed;
        private int spriteMaxSize = 2048;

        // Icon import settings
        private int iconMaxSize = 512;
        private TextureImporterCompression iconCompression = TextureImporterCompression.Compressed;

        // Particle texture settings
        private int particleMaxSize = 1024;
        private bool particleAlphaIsTransparency = true;

        [MenuItem("Tools/Robot TD/Asset Processor", priority = 60)]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetProcessor>("Asset Processor");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            InitStyles();

            EditorGUILayout.Space(8);
            GUILayout.Label("Robot TD - Asset Processor", headerStyle);
            EditorGUILayout.Space(4);

            EditorGUILayout.HelpBox(
                "Batch process textures and sprites with optimized import settings for mobile.\n" +
                "Configure settings below and click Process to apply to all assets in the folder.",
                MessageType.Info);

            EditorGUILayout.Space(8);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawSpriteSection();
            EditorGUILayout.Space(12);
            DrawIconSection();
            EditorGUILayout.Space(12);
            DrawParticleSection();
            EditorGUILayout.Space(12);
            DrawUtilitySection();

            EditorGUILayout.EndScrollView();
        }

        private void InitStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    margin = new RectOffset(10, 10, 5, 5)
                };
            }

            if (sectionStyle == null)
            {
                sectionStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    padding = new RectOffset(5, 5, 5, 5)
                };
            }
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SPRITE PROCESSING
        // ═══════════════════════════════════════════════════════════════════════

        private void DrawSpriteSection()
        {
            GUILayout.Label("━━━ Game Sprites (Towers, Enemies, Projectiles) ━━━", sectionStyle);

            EditorGUILayout.LabelField("Folder Path:");
            spriteFolderPath = EditorGUILayout.TextField(spriteFolderPath);

            EditorGUILayout.Space(4);

            spritePixelsPerUnit = EditorGUILayout.IntField("Pixels Per Unit:", spritePixelsPerUnit);
            spriteFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode:", spriteFilterMode);
            spriteCompression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Compression:", spriteCompression);
            spriteMaxSize = EditorGUILayout.IntField("Max Size:", spriteMaxSize);

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Process All Sprites", GUILayout.Height(30)))
            {
                ProcessSprites();
            }

            EditorGUILayout.HelpBox(
                "Recommended: PPU=100, Bilinear, Compressed, 2048\n" +
                "Use this for in-game sprites (towers, enemies, projectiles).",
                MessageType.None);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ICON PROCESSING
        // ═══════════════════════════════════════════════════════════════════════

        private void DrawIconSection()
        {
            GUILayout.Label("━━━ UI Icons (Buttons, Achievements, etc.) ━━━", sectionStyle);

            EditorGUILayout.LabelField("Folder Path:");
            iconFolderPath = EditorGUILayout.TextField(iconFolderPath);

            EditorGUILayout.Space(4);

            iconMaxSize = EditorGUILayout.IntField("Max Size:", iconMaxSize);
            iconCompression = (TextureImporterCompression)EditorGUILayout.EnumPopup("Compression:", iconCompression);

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Process All UI Icons", GUILayout.Height(30)))
            {
                ProcessIcons();
            }

            EditorGUILayout.HelpBox(
                "Recommended: 512px, Compressed\n" +
                "UI icons should be sharp and smaller for memory efficiency.",
                MessageType.None);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // PARTICLE TEXTURE PROCESSING
        // ═══════════════════════════════════════════════════════════════════════

        private void DrawParticleSection()
        {
            GUILayout.Label("━━━ Particle/VFX Textures ━━━", sectionStyle);

            EditorGUILayout.LabelField("Folder Path:");
            particleFolderPath = EditorGUILayout.TextField(particleFolderPath);

            EditorGUILayout.Space(4);

            particleMaxSize = EditorGUILayout.IntField("Max Size:", particleMaxSize);
            particleAlphaIsTransparency = EditorGUILayout.Toggle("Alpha Is Transparency:", particleAlphaIsTransparency);

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Process All Particle Textures", GUILayout.Height(30)))
            {
                ProcessParticles();
            }

            EditorGUILayout.HelpBox(
                "Recommended: 1024px, Alpha transparency enabled\n" +
                "For particle systems and VFX effects.",
                MessageType.None);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // UTILITY FUNCTIONS
        // ═══════════════════════════════════════════════════════════════════════

        private void DrawUtilitySection()
        {
            GUILayout.Label("━━━ Utilities ━━━", sectionStyle);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Standard Folders", GUILayout.Height(25)))
            {
                CreateStandardFolders();
            }
            if (GUILayout.Button("Generate Missing Meta Files", GUILayout.Height(25)))
            {
                AssetDatabase.Refresh();
                Debug.Log("[AssetProcessor] Refreshed AssetDatabase and regenerated meta files.");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set All Textures to Read/Write OFF", GUILayout.Height(25)))
            {
                DisableReadWriteOnAllTextures();
            }
            if (GUILayout.Button("Compress All Audio", GUILayout.Height(25)))
            {
                CompressAllAudio();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            EditorGUILayout.HelpBox(
                "Utilities:\n" +
                "• Create Folders: Sets up recommended asset folder structure\n" +
                "• Read/Write OFF: Saves memory (do this before building)\n" +
                "• Compress Audio: Optimizes audio files for mobile",
                MessageType.None);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // PROCESSING METHODS
        // ═══════════════════════════════════════════════════════════════════════

        private void ProcessSprites()
        {
            if (!AssetDatabase.IsValidFolder(spriteFolderPath))
            {
                EditorUtility.DisplayDialog("Error", $"Folder not found: {spriteFolderPath}", "OK");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { spriteFolderPath });
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = spritePixelsPerUnit;
                    importer.filterMode = spriteFilterMode;
                    importer.textureCompression = spriteCompression;
                    importer.maxTextureSize = spriteMaxSize;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = true;
                    importer.npotScale = TextureImporterNPOTScale.None;

                    // Android-specific settings
                    var androidSettings = importer.GetPlatformTextureSettings("Android");
                    androidSettings.overridden = true;
                    androidSettings.maxTextureSize = spriteMaxSize;
                    androidSettings.format = TextureImporterFormat.ASTC_6x6;
                    importer.SetPlatformTextureSettings(androidSettings);

                    // iOS-specific settings
                    var iosSettings = importer.GetPlatformTextureSettings("iPhone");
                    iosSettings.overridden = true;
                    iosSettings.maxTextureSize = spriteMaxSize;
                    iosSettings.format = TextureImporterFormat.ASTC_6x6;
                    importer.SetPlatformTextureSettings(iosSettings);

                    importer.SaveAndReimport();
                    count++;
                }
            }

            EditorUtility.DisplayDialog("Complete", $"Processed {count} sprites in {spriteFolderPath}", "OK");
            Debug.Log($"[AssetProcessor] Processed {count} sprites");
        }

        private void ProcessIcons()
        {
            if (!AssetDatabase.IsValidFolder(iconFolderPath))
            {
                EditorUtility.DisplayDialog("Error", $"Folder not found: {iconFolderPath}", "OK");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { iconFolderPath });
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 100;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = iconCompression;
                    importer.maxTextureSize = iconMaxSize;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = true;
                    importer.npotScale = TextureImporterNPOTScale.None;

                    // Mobile settings - higher quality for UI
                    var androidSettings = importer.GetPlatformTextureSettings("Android");
                    androidSettings.overridden = true;
                    androidSettings.maxTextureSize = iconMaxSize;
                    androidSettings.format = TextureImporterFormat.RGBA32;
                    importer.SetPlatformTextureSettings(androidSettings);

                    var iosSettings = importer.GetPlatformTextureSettings("iPhone");
                    iosSettings.overridden = true;
                    iosSettings.maxTextureSize = iconMaxSize;
                    iosSettings.format = TextureImporterFormat.RGBA32;
                    importer.SetPlatformTextureSettings(iosSettings);

                    importer.SaveAndReimport();
                    count++;
                }
            }

            EditorUtility.DisplayDialog("Complete", $"Processed {count} icons in {iconFolderPath}", "OK");
            Debug.Log($"[AssetProcessor] Processed {count} UI icons");
        }

        private void ProcessParticles()
        {
            if (!AssetDatabase.IsValidFolder(particleFolderPath))
            {
                EditorUtility.DisplayDialog("Error", $"Folder not found: {particleFolderPath}", "OK");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { particleFolderPath });
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    importer.maxTextureSize = particleMaxSize;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = particleAlphaIsTransparency;
                    importer.npotScale = TextureImporterNPOTScale.None;

                    var androidSettings = importer.GetPlatformTextureSettings("Android");
                    androidSettings.overridden = true;
                    androidSettings.maxTextureSize = particleMaxSize;
                    androidSettings.format = TextureImporterFormat.ASTC_6x6;
                    importer.SetPlatformTextureSettings(androidSettings);

                    var iosSettings = importer.GetPlatformTextureSettings("iPhone");
                    iosSettings.overridden = true;
                    iosSettings.maxTextureSize = particleMaxSize;
                    iosSettings.format = TextureImporterFormat.ASTC_6x6;
                    importer.SetPlatformTextureSettings(iosSettings);

                    importer.SaveAndReimport();
                    count++;
                }
            }

            EditorUtility.DisplayDialog("Complete", $"Processed {count} particle textures in {particleFolderPath}", "OK");
            Debug.Log($"[AssetProcessor] Processed {count} particle textures");
        }

        private void CreateStandardFolders()
        {
            string[] folders = new string[]
            {
                "Assets/Sprites",
                "Assets/Sprites/Towers",
                "Assets/Sprites/Enemies",
                "Assets/Sprites/Projectiles",
                "Assets/UI",
                "Assets/UI/Icons",
                "Assets/UI/Backgrounds",
                "Assets/VFX",
                "Assets/VFX/Textures",
                "Assets/VFX/Prefabs",
                "Assets/Audio",
                "Assets/Audio/Music",
                "Assets/Audio/SFX",
                "Assets/Prefabs",
                "Assets/Prefabs/Towers",
                "Assets/Prefabs/Enemies",
                "Assets/Prefabs/Projectiles",
                "Assets/Prefabs/UI",
                "Assets/Data",
                "Assets/Data/Towers",
                "Assets/Data/Enemies",
                "Assets/Data/Maps",
                "Assets/Data/Waves",
                "Assets/Resources",
                "Assets/Resources/Prefabs"
            };

            int created = 0;
            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
                    string name = Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parent, name);
                    created++;
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Complete", $"Created {created} new folders.\nTotal standard folders: {folders.Length}", "OK");
            Debug.Log($"[AssetProcessor] Created {created} new folders");
        }

        private void DisableReadWriteOnAllTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null && importer.isReadable)
                {
                    importer.isReadable = false;
                    importer.SaveAndReimport();
                    count++;
                }
            }

            EditorUtility.DisplayDialog("Complete", $"Disabled Read/Write on {count} textures.\nThis saves memory at runtime.", "OK");
            Debug.Log($"[AssetProcessor] Disabled Read/Write on {count} textures");
        }

        private void CompressAllAudio()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;

                if (importer != null)
                {
                    AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                    settings.loadType = AudioClipLoadType.CompressedInMemory;
                    settings.compressionFormat = AudioCompressionFormat.Vorbis;
                    settings.quality = 0.7f; // 70% quality for mobile
                    importer.defaultSampleSettings = settings;

                    // Android-specific
                    AudioImporterSampleSettings androidSettings = importer.GetOverrideSampleSettings("Android");
                    androidSettings.loadType = AudioClipLoadType.CompressedInMemory;
                    androidSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                    androidSettings.quality = 0.7f;
                    importer.SetOverrideSampleSettings("Android", androidSettings);

                    // iOS-specific
                    AudioImporterSampleSettings iosSettings = importer.GetOverrideSampleSettings("iOS");
                    iosSettings.loadType = AudioClipLoadType.CompressedInMemory;
                    iosSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                    iosSettings.quality = 0.7f;
                    importer.SetOverrideSampleSettings("iOS", iosSettings);

                    importer.SaveAndReimport();
                    count++;
                }
            }

            EditorUtility.DisplayDialog("Complete", $"Compressed {count} audio files.\nSet to Vorbis 70% quality for mobile.", "OK");
            Debug.Log($"[AssetProcessor] Compressed {count} audio files");
        }
    }
}
#endif
