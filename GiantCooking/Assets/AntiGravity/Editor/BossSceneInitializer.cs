using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using AntiGravity;

namespace AntiGravity.Editor
{
    public static class BossSceneInitializer
    {
        [MenuItem("AntiGravity/Create bossZERO Scene")]
        public static void CreateBossScene()
        {
            // 1. 新しいシーンの作成
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            newScene.name = "bossZERO";

            // 2. JustEvasionSystemの作成
            GameObject evasionObj = new GameObject("JustEvasionSystem");
            evasionObj.AddComponent<JustEvasionSystem>();

            // 3. ボス ZERO の作成
            GameObject bossObj = new GameObject("Boss_ZERO");
            bossObj.tag = "Enemy";
            
            Rigidbody rb = bossObj.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            
            CapsuleCollider col = bossObj.AddComponent<CapsuleCollider>();
            col.height = 2.0f;
            col.center = new Vector3(0, 1, 0);
            
            bossObj.AddComponent<ZeroBossAI>();

            // 4. マップとプレイヤーの配置（ヒント：既存のシーンからコピーするか、プレハブを配置してください）
            Debug.Log("bossZERO Scene Created.");
            Debug.Log("Please copy your Map and Player (XR Origin) to this scene.");
            
            // シーンの保存
            EditorSceneManager.SaveScene(newScene, "Assets/Scenes/bossZERO.unity");
        }
    }
}
