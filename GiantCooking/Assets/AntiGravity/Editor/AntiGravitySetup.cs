using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace AntiGravity.Editor
{
    public class AntiGravitySetup : EditorWindow
    {
        private const string PLAYER_PREFAB_PATH = "Assets/VRTemplateAssets/Prefabs/Setup/Complete XR Origin Set Up Variant.prefab";
        private const string VIGNETTE_PREFAB_PATH = "Assets/Samples/XR Interaction Toolkit/3.2.1/Starter Assets/TunnelingVignette/Tunneling Vignette.prefab";
        private const string KNIGHT_MODEL_PATH = "Assets/Toon_RTS_demo/models/ToonRTS_demo_Knight.FBX";
        private const string CLASH_SFX_PATH = "Assets/Casual Game Sounds U6/CasualGameSounds/DM-CGS-21.wav";
        private const string ISSEN_SFX_PATH = "Assets/Casual Game Sounds U6/CasualGameSounds/DM-CGS-28.wav";
        private const string GAUGE_MAX_SFX_PATH = "Assets/Casual Game Sounds U6/CasualGameSounds/DM-CGS-45.wav";
        private const string SPARK_VFX_PATH = "Assets/UnityTechnologies/ParticlePack/EffectExamples/Weapon Effects/Prefabs/MetalImpacts.prefab";
        private const string SKYBOX_MAT_PATH = "Assets/VRTemplateAssets/Materials/Skybox/Hub Skybox Blue 2.mat"; // Stable VR skybox
        private const string ISLAND_PREFAB_PATH = "Assets/Low_Poly_Nature_Pack_Lite/Prefabs/Stones/Stone_5_moss.prefab";
        private const string SMALL_ROCK_PREFAB_PATH = "Assets/Low_Poly_Nature_Pack_Lite/Prefabs/Stones/Stone_5_moss.prefab";
        private const string TREE_PREFAB_PATH = "Assets/Low_Poly_Nature_Pack_Lite/Prefabs/Trees/Tree_01_summer.prefab";
        private const string BUSH_PREFAB_PATH = "Assets/Low_Poly_Nature_Pack_Lite/Prefabs/Bushes/Bush_11_summer.prefab";

        [MenuItem("AntiGravity/Create VR Game Scene")]
        public static void CreateVRGame()
        {
            // 0. Reset Environment and Setup Proper Sky
            Material skyboxMat = AssetDatabase.LoadAssetAtPath<Material>(SKYBOX_MAT_PATH);
            if (skyboxMat != null)
            {
                RenderSettings.skybox = skyboxMat;
            }
            else
            {
                RenderSettings.skybox = null; // Revert to Unity default if not found
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.0f;
            RenderSettings.fog = false;
            
            // Ensure Directional Light is clean and white
            Light dirLight = GameObject.FindObjectOfType<Light>();
            if (dirLight != null && dirLight.type == LightType.Directional)
            {
                dirLight.color = Color.white;
                dirLight.intensity = 1.0f;
            }

            // Disable any scripts that might force yellow colors (FastSky remnants)
            var allComponents = GameObject.FindObjectsOfType<MonoBehaviour>();
            foreach (var comp in allComponents)
            {
                string typeName = comp.GetType().Name;
                if (typeName == "Volume" || typeName == "FastSky_Sun_Color")
                {
                    comp.enabled = false;
                }
            }

            DynamicGI.UpdateEnvironment();

            GameObject gm = GameObject.Find("AntiGravity_GameManager");
            if (gm == null)
            {
                gm = new GameObject("AntiGravity_GameManager");
                var manager = gm.AddComponent<GameManager>();
                
                // Assign Audio
                var source = gm.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f; // 2D for UI-like sounds
                
                var propSource = typeof(GameManager).GetField("audioSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var propClip = typeof(GameManager).GetField("gaugeMaxClip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (propSource != null) propSource.SetValue(manager, source);
                if (propClip != null) propClip.SetValue(manager, AssetDatabase.LoadAssetAtPath<AudioClip>(GAUGE_MAX_SFX_PATH));
                
                Debug.Log("Created GameManager and assigned Audio.");
            }

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
                    Debug.LogWarning("Player Prefab not found. Make sure VR Template Assets are installed.");
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
                        
                        // Setup VignetteController
                        var controller = gm.GetComponent<VignetteController>();
                        if (controller == null) controller = gm.AddComponent<VignetteController>();
                        
                        Debug.Log("Added Tunneling Vignette and VignetteController.");
                    }
                }

                // Setup Wrist UI
                SetupWristUI(xrOrigin);
            }

            // 4. Setup Stage (The Floating Ruins Map)
            GameObject oldStage = GameObject.Find("BattleStage");
            if (oldStage != null) Undo.DestroyObjectImmediate(oldStage);

            GameObject stage = new GameObject("BattleStage");
            stage.transform.position = Vector3.zero;
            
            // 1. Create a "Proper Map" Floor (9 stones arranged in a flat-ish area)
            GameObject islandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ISLAND_PREFAB_PATH);
            GameObject rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SMALL_ROCK_PREFAB_PATH);
            
            if (islandPrefab != null)
            {
                // Create a tiled floor with many stones to look like a proper ruined arena
                for (int x = -2; x <= 2; x++)
                {
                    for (int z = -2; z <= 2; z++)
                    {
                        GameObject stone = (GameObject)PrefabUtility.InstantiatePrefab(islandPrefab);
                        stone.transform.SetParent(stage.transform, false);
                        // Randomize position slightly for "natural" look
                        float offX = (x * 4.5f) + Random.Range(-0.5f, 0.5f);
                        float offZ = (z * 4.5f) + Random.Range(-0.5f, 0.5f);
                        stone.transform.localPosition = new Vector3(offX, -0.8f, offZ);
                        stone.transform.localScale = new Vector3(6, 1.5f, 6);
                        stone.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        AddCollidersRecursively(stone);
                    }
                }
                
                // Add surrounding structures (columns)
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
                // Absolute fallback (Better than metal cylinder)
                var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.transform.SetParent(stage.transform, false);
                floor.transform.localPosition = new Vector3(0, -0.1f, 0);
                floor.transform.localScale = new Vector3(20, 0.2f, 20);
                floor.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
            }

            // 2. Add Background Decoration Islands (Non-playable but adds map feel)
            if (rockPrefab != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector3 pos = new Vector3(Random.Range(-20, 20), Random.Range(-10, 10), Random.Range(15, 30));
                    GameObject bgIsland = (GameObject)PrefabUtility.InstantiatePrefab(rockPrefab);
                    bgIsland.transform.SetParent(stage.transform, false);
                    bgIsland.transform.localPosition = pos;
                    bgIsland.transform.localScale = Vector3.one * Random.Range(3, 8);
                    
                    // Add a tree to background islands
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

            // 3. Add Flora to Main Stage
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

            // 5. Setup Sword for Player
            GameObject sword = SetupSword("VR_Sword_Player", new Vector3(0.3f, 1f, 0.3f));
            
            // 6. Setup Enemy
            GameObject oldEnemy = GameObject.Find("Training_Dummy");
            if (oldEnemy != null) Undo.DestroyObjectImmediate(oldEnemy);

            GameObject enemy = new GameObject("Training_Dummy");
            enemy.tag = "Enemy";
            enemy.transform.position = new Vector3(0, 0, 3);
            enemy.transform.localScale = Vector3.one * 1.8f; // Make enemy larger to match player
                
            GameObject knightModel = AssetDatabase.LoadAssetAtPath<GameObject>(KNIGHT_MODEL_PATH);
            if (knightModel != null)
            {
                GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(knightModel);
                modelInstance.transform.SetParent(enemy.transform, false);
            }
            else
            {
                // Fallback to capsule
                GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsule.transform.SetParent(enemy.transform, false);
                capsule.transform.localPosition = new Vector3(0, 1, 0);
            }

            enemy.AddComponent<Rigidbody>();
            enemy.AddComponent<FallOutHandler>();
            enemy.AddComponent<EnemyAI>();
            
            GameObject eSword = SetupSword("Enemy_Sword", new Vector3(0, 0, 0));
            eSword.transform.SetParent(enemy.transform, false);
            eSword.transform.localPosition = new Vector3(0.5f, 0.8f, 0.5f); // Adjusted height for larger enemy
            eSword.transform.localRotation = Quaternion.Euler(0, 0, 90);
            eSword.transform.localScale = Vector3.one * 0.6f; // Sword scale relative to enemy

            Debug.Log("AntiGravity VR Game Setup Complete!");
        }

        private static GameObject SetupSword(string name, Vector3 pos)
        {
            GameObject sword = GameObject.Find(name);
            if (sword == null)
            {
                sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sword.name = name;
                sword.tag = "Sword";
                sword.transform.position = pos;
                sword.transform.localScale = new Vector3(0.05f, 0.05f, 1.0f);
                
                var rb = sword.GetComponent<Rigidbody>();
                if (rb == null) rb = sword.AddComponent<Rigidbody>();
                rb.mass = 2f;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                var grab = sword.AddComponent<XRGrabInteractable>();
                grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                
                var swordComp = sword.AddComponent<Sword>();
                sword.AddComponent<SwordVisuals>();

                // Setup Audio
                var source = sword.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 1.0f; // 3D Sound

                var fSource = typeof(Sword).GetField("audioSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var fClash = typeof(Sword).GetField("clashClip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var fIssen = typeof(Sword).GetField("issenClip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var fSpark = typeof(Sword).GetField("sparkPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (fSource != null) fSource.SetValue(swordComp, source);
                if (fClash != null) fClash.SetValue(swordComp, AssetDatabase.LoadAssetAtPath<AudioClip>(CLASH_SFX_PATH));
                if (fIssen != null) fIssen.SetValue(swordComp, AssetDatabase.LoadAssetAtPath<AudioClip>(ISSEN_SFX_PATH));
                if (fSpark != null) fSpark.SetValue(swordComp, AssetDatabase.LoadAssetAtPath<GameObject>(SPARK_VFX_PATH));
                
                // Add a simple visual cue for the blade
                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = "BladeVisual";
                blade.transform.SetParent(sword.transform, false);
                blade.transform.localScale = new Vector3(0.8f, 0.2f, 1.0f);
                Object.DestroyImmediate(blade.GetComponent<BoxCollider>());
            }
            return sword;
        }

        private static void SetupWristUI(GameObject xrOrigin)
        {
            Transform leftHand = xrOrigin.transform.Find("Camera Offset/Left Controller");
            if (leftHand == null) return;

            GameObject wristCanvas = GameObject.Find("WristGaugeCanvas");
            if (wristCanvas == null)
            {
                wristCanvas = new GameObject("WristGaugeCanvas");
                wristCanvas.transform.SetParent(leftHand, false);
                wristCanvas.transform.localPosition = new Vector3(0, 0.05f, 0.1f);
                wristCanvas.transform.localRotation = Quaternion.Euler(90, 0, 0);
                wristCanvas.transform.localScale = Vector3.one * 0.001f;

                Canvas canvas = wristCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                wristCanvas.AddComponent<UnityEngine.UI.CanvasScaler>();
                
                GameObject bg = new GameObject("Background");
                bg.transform.SetParent(wristCanvas.transform, false);
                var bgImg = bg.AddComponent<UnityEngine.UI.Image>();
                bgImg.color = new Color(0, 0, 0, 0.5f);
                bgImg.rectTransform.sizeDelta = new Vector2(200, 50);

                GameObject fill = new GameObject("Fill");
                fill.transform.SetParent(wristCanvas.transform, false);
                var fillImg = fill.AddComponent<UnityEngine.UI.Image>();
                fillImg.color = Color.cyan;
                fillImg.rectTransform.sizeDelta = new Vector2(200, 50);
                fillImg.type = UnityEngine.UI.Image.Type.Filled;
                fillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;

                var wristUI = wristCanvas.AddComponent<UI.WristGaugeUI>();
                
                // Use reflection to assign fields to WristGaugeUI
                var fFill = typeof(UI.WristGaugeUI).GetField("fillImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (fFill != null) fFill.SetValue(wristUI, fillImg);
            }
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
}
