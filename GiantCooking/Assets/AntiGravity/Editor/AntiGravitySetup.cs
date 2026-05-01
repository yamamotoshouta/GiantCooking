using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace AntiGravity.Editor
{
    public class AntiGravitySetup : EditorWindow
    {
        [MenuItem("AntiGravity/Setup Scene Objects")]
        public static void SetupScene()
        {
            // 1. Setup GameManager
            GameObject gm = GameObject.Find("GameManager");
            if (gm == null)
            {
                gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
                Debug.Log("Created GameManager.");
            }

            // 2. Setup Stage
            GameObject stage = GameObject.Find("Stage");
            if (stage == null)
            {
                stage = GameObject.CreatePrimitive(PrimitiveType.Plane);
                stage.name = "Stage";
                stage.transform.localScale = new Vector3(2, 1, 2);
                stage.AddComponent<FallOutHandler>();
                Debug.Log("Created Stage.");
            }

            // 3. Setup Player Sword
            CreateSword("PlayerSword", new Vector3(0.5f, 1f, 0.5f));

            // 4. Setup Enemy
            GameObject enemy = GameObject.Find("Enemy");
            if (enemy == null)
            {
                enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = "Enemy";
                enemy.tag = "Enemy";
                enemy.transform.position = new Vector3(0, 1, 2);
                enemy.AddComponent<Rigidbody>();
                enemy.AddComponent<FallOutHandler>();
                
                // Add Enemy Sword
                GameObject eSword = CreateSword("EnemySword", new Vector3(0, 0, -0.5f));
                eSword.transform.SetParent(enemy.transform, false);
                eSword.transform.localPosition = new Vector3(0.5f, 0, 0.5f);
                
                Debug.Log("Created Enemy and Enemy Sword.");
            }
            
            Selection.activeObject = gm;
        }

        private static GameObject CreateSword(string name, Vector3 position)
        {
            GameObject sword = GameObject.Find(name);
            if (sword == null)
            {
                sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sword.name = name;
                sword.tag = "Sword";
                sword.transform.position = position;
                sword.transform.localScale = new Vector3(0.05f, 0.05f, 1.2f);

                Rigidbody rb = sword.GetComponent<Rigidbody>();
                if (rb == null) rb = sword.AddComponent<Rigidbody>();
                rb.mass = 1f;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                var grab = sword.AddComponent<XRGrabInteractable>();
                grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
                
                sword.AddComponent<Sword>();
                
                Debug.Log($"Created {name}.");
            }
            return sword;
        }
    }
}
