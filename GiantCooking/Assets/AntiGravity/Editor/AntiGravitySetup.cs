using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace AntiGravity.Editor
{
    public class AntiGravitySetup : EditorWindow
    {
        private const string PLAYER_PREFAB_PATH = "Assets/VRTemplateAssets/Prefabs/Setup/Complete XR Origin Set Up Variant.prefab";
        private const string VIGNETTE_PREFAB_PATH = "Assets/Samples/XR Interaction Toolkit/3.2.1/Starter Assets/TunnelingVignette/Tunneling Vignette.prefab";

        [MenuItem("AntiGravity/Create VR Game Scene")]
        public static void CreateVRGame()
        {
            GameObject gm = GameObject.Find("AntiGravity_GameManager");
            if (gm == null)
            {
                gm = new GameObject("AntiGravity_GameManager");
                gm.AddComponent<GameManager>();
                Debug.Log("Created GameManager.");
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
                enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = "Training_Dummy";
                enemy.tag = "Enemy";
                enemy.transform.position = new Vector3(0, 1, 3);
                enemy.AddComponent<Rigidbody>();
                enemy.AddComponent<FallOutHandler>();
                
                GameObject eSword = SetupSword("Enemy_Sword", new Vector3(0, 0, 0));
                eSword.transform.SetParent(enemy.transform, false);
                eSword.transform.localPosition = new Vector3(0.5f, 0, 0.5f);
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
                
                sword.AddComponent<Sword>();
                sword.AddComponent<SwordVisuals>();
                
                // Add a simple visual cue for the blade
                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.name = "BladeVisual";
                blade.transform.SetParent(sword.transform, false);
                blade.transform.localScale = new Vector3(0.8f, 0.2f, 1.0f);
                Object.DestroyImmediate(blade.GetComponent<BoxCollider>());
            }
            return sword;
        }
    }
}
