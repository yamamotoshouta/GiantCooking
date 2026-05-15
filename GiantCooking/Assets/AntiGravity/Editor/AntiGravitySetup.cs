using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Reflection;
using AntiGravity;

namespace AntiGravity.Editor
{
    public class AntiGravitySetup : EditorWindow
    {
        private const string PLAYER_PREFAB_PATH = "Assets/VRTemplateAssets/Prefabs/Setup/Complete XR Origin Set Up Variant.prefab";
        private const string VIGNETTE_PREFAB_PATH = "Assets/Samples/XR Interaction Toolkit/3.2.1/Starter Assets/TunnelingVignette/Tunneling Vignette.prefab";
        private const string KNIGHT_MODEL_PATH = "Assets/Toon_RTS_demo/models/ToonRTS_demo_Knight.FBX";
        private const string CLASH_SFX_PATH = "Assets/Free Pack/Metal impact 5.wav";
        private const string ISSEN_SFX_PATH = "Assets/Free Pack/Magic Spell_Electricity Spell_1.wav";
        private const string GAUGE_MAX_SFX_PATH = "Assets/Free Pack/Magic Spell_Simple Swoosh_6.wav";
        private const string AMBIENT_SFX_PATH = "Assets/Free Pack/Thunder strikes 30 second- Loop.wav";
        private const string TITLE_BGM_PATH = "Assets/EchoFragments/Main Theme - Echo Break/Echo Break.mp3";
        private const string BGM_PATH = "Assets/EchoFragments/Tension - Arrival/Arrival.mp3";
        private const string VICTORY_BGM_PATH = "Assets/Casual Game Sounds U6/CasualGameSounds/DM-CGS-16.wav";
        private const string DEFEAT_BGM_PATH = "Assets/Casual Game Sounds U6/CasualGameSounds/DM-CGS-18.wav";
        private const string FOOTSTEP_SFX_PATH = "Assets/Free Pack/Walking in ChainMail - Loop.wav";
        private const string SPARK_VFX_PATH = "Assets/UnityTechnologies/ParticlePack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts.prefab";
        private const string AURA_VFX_PATH = "Assets/UnityTechnologies/ParticlePack/EffectExamples/Misc Effects/Prefabs/ElectricalSparks.prefab";
        private const string SKYBOX_MAT_PATH = "Assets/VRTemplateAssets/Materials/Skybox/Hub Skybox Blue 2.mat"; // Stable VR skybox
        private const string ISLAND_PREFAB_PATH = "Assets/Low_Poly_Nature_Pack_Lite/Prefabs/Stones/Stone_5_moss.prefab";
        private const string SMALL_ROCK_PREFAB_PATH = "Assets/Low_Poly_Nature_Pack_Lite/Prefabs/Stones/Stone_5_moss.prefab";
        private const string TREE_PREFAB_PATH = "Assets/Low_Poly_Nature_Pack_Lite/Prefabs/Trees/Tree_01_summer.prefab";
        private const string BUSH_PREFAB_PATH = "Assets/Low_Poly_Nature_Pack_Lite/Prefabs/Bushes/Bush_11_summer.prefab";
        private const string ANIM_CONTROLLER_PATH = "Assets/AntiGravity/KnightController.controller";
        
        private const string IDLE_ANIM_PATH = "Assets/Toon_RTS_demo/animations/WK_heavy_infantry_05_combat_idle.FBX";
        private const string WALK_ANIM_PATH = "Assets/Toon_RTS_demo/animations/WK_heavy_infantry_06_combat_walk.FBX";
        private const string ATTACK_ANIM_PATH = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/1H/HumanM@Attack1H01_R.fbx";
        private const string RECOIL_ANIM_PATH = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/HumanM@CombatDamage01.fbx";

        private const string UI_CIRCLE_OUTLINE = "Assets/VRTemplateAssets/Sprites/UI/Circle_60x60 Outline.png";
        private const string UI_CIRCLE_FILL = "Assets/VRTemplateAssets/Sprites/UI/CircleMask.png";

        [MenuItem("AntiGravity/Create VR Game Scene")]
        public static void CreateVRGame()
        {
            CreateTags();

            // 0. Reset Environment and Setup Proper Sky
            Material skyboxMat = AssetDatabase.LoadAssetAtPath<Material>(SKYBOX_MAT_PATH);
            if (skyboxMat != null)
            {
                RenderSettings.skybox = skyboxMat;
            }
            else
            {
                RenderSettings.skybox = null; 
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.0f;
            RenderSettings.fog = false;
            
            Light dirLight = GameObject.FindObjectOfType<Light>();
            if (dirLight != null && dirLight.type == LightType.Directional)
            {
                dirLight.color = Color.white;
                dirLight.intensity = 1.0f;
            }

            var allComponents = GameObject.FindObjectsOfType<MonoBehaviour>();
            foreach (var comp in allComponents)
            {
                if (comp == null) continue;
                string typeName = comp.GetType().Name;
                if (typeName == "Volume" || typeName == "FastSky_Sun_Color" || typeName == "FastSky_Ambience")
                {
                    comp.enabled = false;
                }
            }

            // Forcefully stop and destroy any existing looping ambient AudioSources
            AudioSource[] allAudio = GameObject.FindObjectsOfType<AudioSource>();
            foreach (var audio in allAudio)
            {
                if (audio == null) continue;
                
                bool shouldDestroy = audio.loop || (audio.clip != null && (audio.clip.name.Contains("Thunder") || audio.clip.name.Contains("Electricity")));
                if (shouldDestroy)
                {
                    audio.Stop();
                    if (!EditorApplication.isPlaying) Undo.DestroyObjectImmediate(audio.gameObject);
                }
            }

            DynamicGI.UpdateEnvironment();

            GameObject gm = GameObject.Find("AntiGravity_GameManager");
            if (gm != null) Undo.DestroyObjectImmediate(gm); // Force recreate to clear old settings
            
            gm = new GameObject("AntiGravity_GameManager");
            var manager = gm.AddComponent<GameManager>();
            gm.AddComponent<AntiGravity.System.TimeManager>(); 
            
            var source = gm.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;

            var bgmSource = gm.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;
            bgmSource.volume = 0.4f;

            var fAudioSource = typeof(GameManager).GetField("audioSource", BindingFlags.NonPublic | BindingFlags.Instance);
            var fBgmSource = typeof(GameManager).GetField("bgmSource", BindingFlags.NonPublic | BindingFlags.Instance);
            var fMaxClip = typeof(GameManager).GetField("gaugeMaxClip", BindingFlags.NonPublic | BindingFlags.Instance);
            var fIssenClip = typeof(GameManager).GetField("issenActivateClip", BindingFlags.NonPublic | BindingFlags.Instance);
            var fTitleBgm = typeof(GameManager).GetField("titleBGM", BindingFlags.NonPublic | BindingFlags.Instance);
            var fPlayBgm = typeof(GameManager).GetField("playingBGM", BindingFlags.NonPublic | BindingFlags.Instance);
            var fVicBgm = typeof(GameManager).GetField("victoryBGM", BindingFlags.NonPublic | BindingFlags.Instance);
            var fDefBgm = typeof(GameManager).GetField("defeatBGM", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (fAudioSource != null) fAudioSource.SetValue(manager, source);
            if (fBgmSource != null) fBgmSource.SetValue(manager, bgmSource);
            if (fMaxClip != null) fMaxClip.SetValue(manager, AssetDatabase.LoadAssetAtPath<AudioClip>(GAUGE_MAX_SFX_PATH));
            if (fIssenClip != null) fIssenClip.SetValue(manager, AssetDatabase.LoadAssetAtPath<AudioClip>(ISSEN_SFX_PATH));
            if (fTitleBgm != null) fTitleBgm.SetValue(manager, AssetDatabase.LoadAssetAtPath<AudioClip>(TITLE_BGM_PATH));
            if (fPlayBgm != null) fPlayBgm.SetValue(manager, AssetDatabase.LoadAssetAtPath<AudioClip>(BGM_PATH));
            if (fVicBgm != null) fVicBgm.SetValue(manager, AssetDatabase.LoadAssetAtPath<AudioClip>(VICTORY_BGM_PATH));
            if (fDefBgm != null) fDefBgm.SetValue(manager, AssetDatabase.LoadAssetAtPath<AudioClip>(DEFEAT_BGM_PATH));

            Debug.Log("Created GameManager and assigned BGM/Audio.");


            // 1. Setup XR Origin
            GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            if (xrOrigin == null)
            {
                GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PLAYER_PREFAB_PATH);
                if (playerPrefab != null)
                {
                    xrOrigin = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                    xrOrigin.name = "XR Origin (XR Rig)";
                    Debug.Log("Instantiated XR Origin from Template.");
                }
                else
                {
                    Debug.LogWarning("Player Prefab not found.");
                }
            }

            // 2. Setup Tunneling Vignette
            if (xrOrigin != null)
            {
                GameObject vignette = GameObject.Find("Tunneling Vignette");
                if (vignette == null)
                {
                    GameObject vignettePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VIGNETTE_PREFAB_PATH);
                    if (vignettePrefab != null)
                    {
                        vignette = (GameObject)PrefabUtility.InstantiatePrefab(vignettePrefab);
                        vignette.transform.SetParent(xrOrigin.transform.Find("Camera Offset/Main Camera"), false);
                        
                        var controller = gm.GetComponent<VignetteController>();
                        if (controller == null) controller = gm.AddComponent<VignetteController>();
                        
                        Debug.Log("Added Tunneling Vignette and VignetteController.");
                    }
                }

                SetupWristUI(xrOrigin);

                // Setup Player Head Collider for damage detection
                Transform mainCam = xrOrigin.transform.Find("Camera Offset/Main Camera");
                if (mainCam != null)
                {
                    mainCam.gameObject.tag = "MainCamera";
                    var headCol = mainCam.gameObject.GetComponent<SphereCollider>();
                    if (headCol == null) headCol = mainCam.gameObject.AddComponent<SphereCollider>();
                    headCol.radius = 0.15f;
                    headCol.isTrigger = true;

                    var headRb = mainCam.gameObject.GetComponent<Rigidbody>();
                    if (headRb == null) headRb = mainCam.gameObject.AddComponent<Rigidbody>();
                    headRb.isKinematic = true;
                    
                    Debug.Log("Setup Player Head Collider and Rigidbody.");
                }
            }

            // 4. Setup Stage
            GameObject oldStage = GameObject.Find("BattleStage");
            if (oldStage != null) Undo.DestroyObjectImmediate(oldStage);

            GameObject stage = new GameObject("BattleStage");
            stage.transform.position = Vector3.zero;
            
            GameObject islandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ISLAND_PREFAB_PATH);
            GameObject rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SMALL_ROCK_PREFAB_PATH);
            
            if (islandPrefab != null)
            {
                for (int x = -2; x <= 2; x++)
                {
                    for (int z = -2; z <= 2; z++)
                    {
                        GameObject stone = (GameObject)PrefabUtility.InstantiatePrefab(islandPrefab);
                        stone.transform.SetParent(stage.transform, false);
                        float offX = (x * 4.5f) + Random.Range(-0.5f, 0.5f);
                        float offZ = (z * 4.5f) + Random.Range(-0.5f, 0.5f);
                        stone.transform.localPosition = new Vector3(offX, -0.8f, offZ);
                        stone.transform.localScale = new Vector3(6, 1.5f, 6);
                        stone.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        AddCollidersRecursively(stone);
                    }
                }
                
                if (rockPrefab != null)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = i * 30 * Mathf.Deg2Rad;
                        Vector3 pos = new Vector3(Mathf.Cos(angle) * 12, -0.5f, Mathf.Sin(angle) * 12);
                        GameObject pillar = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
                        pillar.transform.SetParent(stage.transform, false);
                        pillar.transform.localPosition = pos;
                        pillar.transform.localScale = new Vector3(2, Random.Range(5, 10), 2);
                        AddCollidersRecursively(pillar);
                    }
                }
            }
            else
            {
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.SetParent(stage.transform, false);
                floor.transform.localPosition = new Vector3(0, -0.1f, 0);
                floor.transform.localScale = new Vector3(20, 0.2f, 20);
                floor.GetComponent<Renderer>().sharedMaterial.color = new Color(0.3f, 0.3f, 0.3f);
            }

            if (rockPrefab != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 pos = new Vector3(Random.Range(-20, 20), Random.Range(-10, 10), Random.Range(15, 30));
                    GameObject bgIsland = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
                    bgIsland.transform.SetParent(stage.transform, false);
                    bgIsland.transform.localPosition = pos;
                    bgIsland.transform.localScale = Vector3.one * Random.Range(3, 8);
                    
                    GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TREE_PREFAB_PATH);
                    if (treePrefab != null)
                    {
                        GameObject bgTree = (GameObject)PrefabUtility.InstantiatePrefab(treePrefab);
                        bgTree.transform.SetParent(bgIsland.transform, false);
                        bgTree.transform.localPosition = new Vector3(0, 0.5f, 0);
                        bgTree.transform.localScale = Vector3.one * 0.2f;
                    }
                }
            }

            GameObject mainTreePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TREE_PREFAB_PATH);
            GameObject mainBushPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BUSH_PREFAB_PATH);
            if (mainTreePrefab != null)
            {
                CreateDecoration(mainTreePrefab, stage.transform, new Vector3(7, 0, 7), Vector3.one);
                CreateDecoration(mainTreePrefab, stage.transform, new Vector3(-7, 0, -7), Vector3.one);
            }
            if (mainBushPrefab != null)
            {
                CreateDecoration(mainBushPrefab, stage.transform, new Vector3(0, 0, 8), Vector3.one * 1.5f);
                CreateDecoration(mainBushPrefab, stage.transform, new Vector3(0, 0, -8), Vector3.one * 1.5f);
            }

            stage.AddComponent<FallOutHandler>();

            // Add Ambient Wind/Thunder Sound to Stage
            var stageAudio = stage.AddComponent<AudioSource>();
            stageAudio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AMBIENT_SFX_PATH);
            stageAudio.playOnAwake = true;
            stageAudio.loop = true;
            stageAudio.spatialBlend = 0.5f; // Semi-spatial
            stageAudio.volume = 0.05f; // Much lower to prioritize BGM
            // stageAudio.Play(); // Removed to prevent playing in Unity Editor

            // 5. Setup Sword for Player
            GameObject sword = SetupSword("VR_Sword_Player", new Vector3(0.3f, 1f, 0.3f));
            
            // 6. Setup Enemy
            GameObject[] oldEnemies = GameObject.FindObjectsOfType<GameObject>();
            foreach (var go in oldEnemies)
            {
                if (go.name == "Training_Dummy" || go.name == "Enemy_Knight" || go.tag == "Enemy")
                {
                    Undo.DestroyObjectImmediate(go);
                }
            }

            GameObject knightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(KNIGHT_MODEL_PATH);
            GameObject enemy;
            if (knightPrefab != null)
            {
                enemy = (GameObject)PrefabUtility.InstantiatePrefab(knightPrefab);
                enemy.name = "Enemy_Knight";
            }
            else
            {
                enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = "Enemy_Knight";
                enemy.transform.localPosition = new Vector3(0, 1, 0);
            }

            enemy.tag = "Enemy";
            enemy.transform.position = new Vector3(0, 0.5f, 3); 
            enemy.transform.localScale = Vector3.one * 1.8f;

            var rb = enemy.GetComponent<Rigidbody>();
            if (rb == null) rb = enemy.AddComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.mass = 10f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            var charCtrl = enemy.GetComponent<CharacterController>();
            if (charCtrl != null) Object.DestroyImmediate(charCtrl);

            CapsuleCollider enemyCol = enemy.GetComponent<CapsuleCollider>();
            if (enemyCol == null)
            {
                enemyCol = enemy.AddComponent<CapsuleCollider>();
                enemyCol.center = new Vector3(0, 1, 0);
                enemyCol.height = 2f;
                enemyCol.radius = 0.3f;
            }

            enemy.AddComponent<FallOutHandler>();
            enemy.AddComponent<EnemyAI>();
            
            // Footstep Sound (Chainmail)
            var footstepSource = enemy.AddComponent<AudioSource>();
            footstepSource.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(FOOTSTEP_SFX_PATH);
            footstepSource.loop = true;
            footstepSource.playOnAwake = true;
            footstepSource.volume = 0.4f;
            footstepSource.spatialBlend = 1.0f;
            footstepSource.spatialize = true;
            footstepSource.minDistance = 1f;
            footstepSource.maxDistance = 10f;
            // footstepSource.Play(); // Disabled to ensure quiet environment

            
            var animator = enemy.GetComponent<Animator>();
            if (animator == null) animator = enemy.AddComponent<Animator>();
            animator.runtimeAnimatorController = SetupAnimatorController();
            
            // Find the existing sword in the model hierarchy
            Transform existingSword = enemy.transform.FindRecursive("Sword");
            if (existingSword == null) existingSword = enemy.transform.FindRecursive("weapon");
            
            if (existingSword != null)
            {
                existingSword.gameObject.name = "Enemy_Sword"; // Rename to match EnemyAI's search
                existingSword.gameObject.tag = "Sword";
                
                // Add Sword script if not present
                var swordComp = existingSword.gameObject.GetComponent<Sword>();
                if (swordComp == null) swordComp = existingSword.gameObject.AddComponent<Sword>();

                // Add Trigger Collider for safety (prevents flying bug)
                var boxCol = existingSword.gameObject.GetComponent<BoxCollider>();
                if (boxCol == null) boxCol = existingSword.gameObject.AddComponent<BoxCollider>();
                boxCol.isTrigger = true; 

                var sRb = existingSword.gameObject.GetComponent<Rigidbody>();
                if (sRb == null) sRb = existingSword.gameObject.AddComponent<Rigidbody>();
                sRb.isKinematic = true; // Follow animation but allow trigger detection

                // Setup Sword references (Audio, etc.)
                var bladeRenderer = existingSword.GetComponent<Renderer>();
                var swordSource = existingSword.gameObject.GetComponent<AudioSource>();
                if (swordSource == null) swordSource = existingSword.gameObject.AddComponent<AudioSource>();
                swordSource.playOnAwake = false;
                swordSource.spatialBlend = 1.0f;

                var fSource = typeof(Sword).GetField("audioSource", BindingFlags.NonPublic | BindingFlags.Instance);
                var fClash = typeof(Sword).GetField("clashClip", BindingFlags.NonPublic | BindingFlags.Instance);
                var fRenderer = typeof(Sword).GetField("swordRenderer", BindingFlags.NonPublic | BindingFlags.Instance);

                if (fSource != null) fSource.SetValue(swordComp, swordSource);
                if (fClash != null) fClash.SetValue(swordComp, AssetDatabase.LoadAssetAtPath<AudioClip>(CLASH_SFX_PATH));
                if (fRenderer != null) fRenderer.SetValue(swordComp, bladeRenderer);

                Debug.Log("Attached Sword logic to existing model: " + existingSword.name);
            }
            else
            {
                Debug.LogWarning("Existing sword not found in enemy model.");
            }

            // 6. Setup UI
            SetupGameUI();

            Debug.Log("AntiGravity VR Game Setup Complete!");
        }

        private static void CreateTags()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            string[] tagsToCreate = { "Sword", "Enemy" };
            foreach (string tag in tagsToCreate)
            {
                bool exists = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    tagsProp.InsertArrayElementAtIndex(0);
                    tagsProp.GetArrayElementAtIndex(0).stringValue = tag;
                }
            }
            tagManager.ApplyModifiedProperties();
        }

        private static void SetupGameUI()
        {
            GameObject uiRoot = GameObject.Find("AntiGravity_UI");
            if (uiRoot != null) Undo.DestroyObjectImmediate(uiRoot);

            uiRoot = new GameObject("AntiGravity_UI");
            uiRoot.transform.position = new Vector3(0, 1.5f, 2.0f);
            
            var menuManager = uiRoot.AddComponent<AntiGravity.UI.MenuUIManager>();

            GameObject startPanel = CreateUIPanel("StartPanel", uiRoot.transform, "ANTI-GRAVITY", "START GAME");
            GameObject victoryPanel = CreateUIPanel("VictoryPanel", uiRoot.transform, "VICTORY!", "PLAY AGAIN");
            victoryPanel.SetActive(false);
            GameObject defeatPanel = CreateUIPanel("DefeatPanel", uiRoot.transform, "DEFEAT...", "TRY AGAIN");
            defeatPanel.SetActive(false);

            var fStart = typeof(AntiGravity.UI.MenuUIManager).GetField("startPanel", BindingFlags.NonPublic | BindingFlags.Instance);
            var fVictory = typeof(AntiGravity.UI.MenuUIManager).GetField("victoryPanel", BindingFlags.NonPublic | BindingFlags.Instance);
            var fDefeat = typeof(AntiGravity.UI.MenuUIManager).GetField("defeatPanel", BindingFlags.NonPublic | BindingFlags.Instance);
            var fBtnStart = typeof(AntiGravity.UI.MenuUIManager).GetField("startButton", BindingFlags.NonPublic | BindingFlags.Instance);
            var fBtnRestart = typeof(AntiGravity.UI.MenuUIManager).GetField("restartButton", BindingFlags.NonPublic | BindingFlags.Instance);
            var fBtnTryAgain = typeof(AntiGravity.UI.MenuUIManager).GetField("tryAgainButton", BindingFlags.NonPublic | BindingFlags.Instance);

            if (fStart != null) fStart.SetValue(menuManager, startPanel);
            if (fVictory != null) fVictory.SetValue(menuManager, victoryPanel);
            if (fDefeat != null) fDefeat.SetValue(menuManager, defeatPanel);
            if (fBtnStart != null) fBtnStart.SetValue(menuManager, startPanel.GetComponentInChildren<Button>());
            if (fBtnRestart != null) fBtnRestart.SetValue(menuManager, victoryPanel.GetComponentInChildren<Button>());
            if (fBtnTryAgain != null) fBtnTryAgain.SetValue(menuManager, defeatPanel.GetComponentInChildren<Button>());
        }

        private static GameObject CreateUIPanel(string name, Transform parent, string titleText, string buttonText)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            Canvas canvas = panel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            panel.AddComponent<CanvasScaler>();
            panel.AddComponent<GraphicRaycaster>();
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 300);
            panel.transform.localScale = Vector3.one * 0.005f;

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(panel.transform, false);
            var img = bg.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.8f);
            bg.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 300);

            GameObject title = new GameObject("Title");
            title.transform.SetParent(panel.transform, false);
            title.transform.localPosition = new Vector3(0, 80, 0);
            var tmpTitle = title.AddComponent<TextMeshProUGUI>();
            tmpTitle.text = titleText;
            tmpTitle.fontSize = 48;
            tmpTitle.alignment = TextAlignmentOptions.Center;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 100);

            GameObject btnObj = new GameObject("Button");
            btnObj.transform.SetParent(panel.transform, false);
            btnObj.transform.localPosition = new Vector3(0, -50, 0);
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = Color.white;
            var btn = btnObj.AddComponent<Button>();
            btnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 60);

            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            var tmpBtn = btnTextObj.AddComponent<TextMeshProUGUI>();
            tmpBtn.text = buttonText;
            tmpBtn.color = Color.black;
            tmpBtn.fontSize = 24;
            tmpBtn.alignment = TextAlignmentOptions.Center;
            btnTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 60);

            return panel;
        }

        private static GameObject SetupSword(string name, Vector3 pos)
        {
            GameObject sword = GameObject.Find(name);
            if (sword != null) Undo.DestroyObjectImmediate(sword);

            sword = new GameObject(name);
            sword.tag = "Sword";
            sword.transform.position = pos;
            
            var rb = sword.AddComponent<Rigidbody>();
            rb.mass = 2f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var grab = sword.AddComponent<XRGrabInteractable>();
            grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            
            var swordComp = sword.AddComponent<Sword>();

            GameObject hilt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hilt.name = "Hilt";
            hilt.transform.SetParent(sword.transform, false);
            hilt.transform.localPosition = new Vector3(0, 0, -0.4f);
            hilt.transform.localScale = new Vector3(0.03f, 0.15f, 0.03f);
            hilt.transform.localRotation = Quaternion.Euler(90, 0, 0);
            hilt.GetComponent<Renderer>().sharedMaterial.color = new Color(0.2f, 0.2f, 0.2f);
            Object.DestroyImmediate(hilt.GetComponent<Collider>());

            GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            guard.name = "Guard";
            guard.transform.SetParent(sword.transform, false);
            guard.transform.localPosition = new Vector3(0, 0, -0.2f);
            guard.transform.localScale = new Vector3(0.15f, 0.03f, 0.05f);
            guard.GetComponent<Renderer>().sharedMaterial.color = new Color(0.3f, 0.3f, 0.3f);
            Object.DestroyImmediate(guard.GetComponent<Collider>());

            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade";
            blade.transform.SetParent(sword.transform, false);
            blade.transform.localPosition = new Vector3(0, 0, 0.3f);
            blade.transform.localScale = new Vector3(0.08f, 0.01f, 1.0f);
            var bladeRenderer = blade.GetComponent<Renderer>();
            bladeRenderer.sharedMaterial.color = new Color(0.8f, 0.8f, 0.9f);
            
            var col = blade.GetComponent<BoxCollider>();
            if (col == null) col = blade.AddComponent<BoxCollider>();

            var trail = blade.AddComponent<TrailRenderer>();
            trail.time = 0.2f;
            trail.startWidth = 0.08f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = Color.yellow;
            trail.enabled = false;

            GameObject auraObj = null;
            ParticleSystem auraPs = null;
            GameObject auraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AURA_VFX_PATH);
            if (auraPrefab != null)
            {
                auraObj = (GameObject)PrefabUtility.InstantiatePrefab(auraPrefab);
                auraObj.transform.SetParent(hilt.transform, false);
                auraObj.transform.localPosition = Vector3.zero;
                auraPs = auraObj.GetComponent<ParticleSystem>();
                if (auraPs == null) auraPs = auraObj.GetComponentInChildren<ParticleSystem>();
                if (auraPs != null)
                {
                    var main = auraPs.main;
                    main.playOnAwake = false;
                    auraPs.Stop();
                }
            }

            var source = sword.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1.0f;
            source.spatialize = true; // Meta Quest spatial audio

            var fSource = typeof(Sword).GetField("audioSource", BindingFlags.NonPublic | BindingFlags.Instance);
            var fClash = typeof(Sword).GetField("clashClip", BindingFlags.NonPublic | BindingFlags.Instance);
            var fIssen = typeof(Sword).GetField("issenClip", BindingFlags.NonPublic | BindingFlags.Instance);
            var fSpark = typeof(Sword).GetField("sparkPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
            var fRenderer = typeof(Sword).GetField("swordRenderer", BindingFlags.NonPublic | BindingFlags.Instance);
            var fTrail = typeof(Sword).GetField("swordTrail", BindingFlags.NonPublic | BindingFlags.Instance);
            var fAura = typeof(Sword).GetField("auraParticles", BindingFlags.NonPublic | BindingFlags.Instance);

            if (fSource != null) fSource.SetValue(swordComp, source);
            if (fClash != null) fClash.SetValue(swordComp, AssetDatabase.LoadAssetAtPath<AudioClip>(CLASH_SFX_PATH));
            if (fIssen != null) fIssen.SetValue(swordComp, AssetDatabase.LoadAssetAtPath<AudioClip>(ISSEN_SFX_PATH));
            if (fSpark != null) fSpark.SetValue(swordComp, AssetDatabase.LoadAssetAtPath<GameObject>(SPARK_VFX_PATH));
            if (fRenderer != null) fRenderer.SetValue(swordComp, bladeRenderer);
            if (fTrail != null) fTrail.SetValue(swordComp, trail);
            if (fAura != null) fAura.SetValue(swordComp, auraPs);

            return sword;
        }

        private static void CreateDecoration(GameObject prefab, Transform parent, Vector3 pos, Vector3 scale)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = pos;
            instance.transform.localScale = scale;
            instance.transform.localRotation = Quaternion.Euler(Random.Range(-10, 10), Random.Range(0, 360), Random.Range(-10, 10));
            AddCollidersRecursively(instance);
        }

        private static void SetupWristUI(GameObject xrOrigin)
        {
            Transform leftHand = xrOrigin.transform.Find("Camera Offset/Left Controller");
            if (leftHand == null) return;

            GameObject canvasObj = new GameObject("WristCanvas");
            canvasObj.transform.SetParent(leftHand, false);
            canvasObj.transform.localPosition = new Vector3(0, 0.05f, 0.1f);
            canvasObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            canvasObj.transform.localScale = Vector3.one * 0.0006f; // Slightly smaller to fit two

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject bgObj = new GameObject("Kiwami_BG");
            bgObj.transform.SetParent(canvasObj.transform, false);
            bgObj.transform.localPosition = new Vector3(-60, 0, 0);
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UI_CIRCLE_OUTLINE);
            bgImg.color = new Color(1, 1, 1, 0.5f);
            bgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

            GameObject fillObj = new GameObject("Kiwami_Fill");
            fillObj.transform.SetParent(bgObj.transform, false);
            var fillImg = fillObj.AddComponent<Image>();
            fillImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UI_CIRCLE_FILL);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Radial360;
            fillImg.fillOrigin = (int)Image.Origin360.Top;
            fillImg.fillAmount = 0f;
            fillObj.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 90);

            // HP UI
            GameObject hpBgObj = new GameObject("HP_BG");
            hpBgObj.transform.SetParent(canvasObj.transform, false);
            hpBgObj.transform.localPosition = new Vector3(60, 0, 0);
            var hpBgImg = hpBgObj.AddComponent<Image>();
            hpBgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UI_CIRCLE_OUTLINE);
            hpBgImg.color = new Color(1, 0.2f, 0.2f, 0.5f);
            hpBgObj.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);

            GameObject hpFillObj = new GameObject("HP_Fill");
            hpFillObj.transform.SetParent(hpBgObj.transform, false);
            var hpFillImg = hpFillObj.AddComponent<Image>();
            hpFillImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UI_CIRCLE_FILL);
            hpFillImg.color = Color.red;
            hpFillImg.type = Image.Type.Filled;
            hpFillImg.fillMethod = Image.FillMethod.Radial360;
            hpFillImg.fillOrigin = (int)Image.Origin360.Top;
            hpFillImg.fillAmount = 1f;
            hpFillObj.GetComponent<RectTransform>().sizeDelta = new Vector2(90, 90);

            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(canvasObj.transform, false);
            textObj.transform.localPosition = new Vector3(0, -60, 0);
            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "READY";
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            textObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);

            var uiComp = canvasObj.AddComponent<GaugeUI>();
            var fFill = typeof(GaugeUI).GetField("gaugeFill", BindingFlags.NonPublic | BindingFlags.Instance);
            var fHpFill = typeof(GaugeUI).GetField("hpFill", BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (fFill != null) fFill.SetValue(uiComp, fillImg);
            if (fHpFill != null) fHpFill.SetValue(uiComp, hpFillImg);
        }

        private static RuntimeAnimatorController SetupAnimatorController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ANIM_CONTROLLER_PATH);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ANIM_CONTROLLER_PATH);
                controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
                controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("Recoil", AnimatorControllerParameterType.Trigger);

                var rootStateMachine = controller.layers[0].stateMachine;

                AnimationClip idleClip = GetClipFromFBX(IDLE_ANIM_PATH);
                AnimationClip walkClip = GetClipFromFBX(WALK_ANIM_PATH);
                AnimationClip attackClip = GetClipFromFBX(ATTACK_ANIM_PATH);
                AnimationClip recoilClip = GetClipFromFBX(RECOIL_ANIM_PATH);

                var idleState = rootStateMachine.AddState("Idle");
                idleState.motion = idleClip;

                var walkState = rootStateMachine.AddState("Walk");
                walkState.motion = walkClip;

                var attackState = rootStateMachine.AddState("Attack");
                attackState.motion = attackClip;
                
                var recoilState = rootStateMachine.AddState("Recoil");
                recoilState.motion = recoilClip;

                idleState.AddTransition(walkState).AddCondition(AnimatorConditionMode.If, 0, "IsWalking");
                walkState.AddTransition(idleState).AddCondition(AnimatorConditionMode.IfNot, 0, "IsWalking");
                idleState.AddTransition(attackState).AddCondition(AnimatorConditionMode.If, 0, "Attack");
                walkState.AddTransition(attackState).AddCondition(AnimatorConditionMode.If, 0, "Attack");
                attackState.AddTransition(idleState).hasExitTime = true;

                var toRecoil = rootStateMachine.AddAnyStateTransition(recoilState);
                toRecoil.AddCondition(AnimatorConditionMode.If, 0, "Recoil");
                recoilState.AddTransition(idleState).hasExitTime = true;
            }
            return controller;
        }

        private static AnimationClip GetClipFromFBX(string path)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in assets)
            {
                if (asset is AnimationClip && !asset.name.Contains("__preview__")) return (AnimationClip)asset;
            }
            return null;
        }

        private static void AddCollidersRecursively(GameObject target)
        {
            foreach (var meshFilter in target.GetComponentsInChildren<MeshFilter>())
            {
                GameObject go = meshFilter.gameObject;
                if (go.GetComponent<Collider>() == null)
                {
                    var mc = go.AddComponent<MeshCollider>();
                    mc.convex = true;
                }
            }
        }
    }

    public static class TransformExtensions
    {
        public static Transform FindRecursive(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name.Contains(name)) return child;
                Transform result = child.FindRecursive(name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
