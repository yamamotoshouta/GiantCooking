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

        [MenuItem("AntiGravity/Create VR Game Scene")]
        public static void CreateVRGame()
        {
            GameObject gm = GameObject.Find("AntiGravity_GameManager");
            if (gm == null)
            {
                gm = new GameObject("AntiGravity_GameManager");
                var manager = gm.AddComponent<GameManager>();
                
                // Assign Audio
                var source = gm.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f; // 2D for UI-like sounds
                
                // Use reflection or just direct assignment if we are in the same assembly
                // But since GameManager is in a different asmdef, we need to be careful.
                // However, the setup script usually has access to all.
                
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
                        
                        // Use reflection or Find to set the field if it's private, 
                        // but here we'll just let the user assign it or try to find it.
                        Debug.Log("Added Tunneling Vignette and VignetteController.");
                    }
                }

                // Setup Wrist UI
                SetupWristUI(xrOrigin);
            }

            // 3. GameManager is already setup at the beginning.

            // 4. Setup Stage (The Floating Island)
            GameObject stage = GameObject.Find("BattleStage");
            if (stage == null)
            {
                stage = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stage.name = "BattleStage";
                stage.transform.position = new Vector3(0, -0.05f, 0);
                stage.transform.localScale = new Vector3(5, 0.1f, 5);
                stage.AddComponent<FallOutHandler>();
                
                // Add a simple grid material if possible
                Renderer rend = stage.GetComponent<Renderer>();
                rend.material.color = new Color(0.2f, 0.2f, 0.2f);
            }

            // 5. Setup Sword for Player
            GameObject sword = SetupSword("VR_Sword_Player", new Vector3(0.3f, 1f, 0.3f));
            
            // 6. Setup Enemy
            GameObject enemy = GameObject.Find("Training_Dummy");
            if (enemy == null)
            {
                enemy = new GameObject("Training_Dummy");
                enemy.tag = "Enemy";
                enemy.transform.position = new Vector3(0, 0, 3);
                
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
                eSword.transform.localPosition = new Vector3(0.5f, 1f, 0.5f);
                eSword.transform.localRotation = Quaternion.Euler(0, 0, 90);
            }

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
    }
}
