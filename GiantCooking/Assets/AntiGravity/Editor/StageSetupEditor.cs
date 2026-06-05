using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using AntiGravity.System;

namespace AntiGravity.Editor
{
    public class StageSetupEditor : EditorWindow
    {
        [MenuItem("AntiGravity/崩壊ステージを自動生成")]
        public static void GenerateStage()
        {
            // すでに存在するかチェック
            GameObject existingStage = GameObject.Find("DestructibleStage");
            if (existingStage != null)
            {
                if (!EditorUtility.DisplayDialog("確認", "すでに DestructibleStage が存在します。新しく作成しますか？", "はい", "いいえ"))
                {
                    return;
                }
            }

            // 1. 親オブジェクトの作成
            GameObject stageRoot = new GameObject("DestructibleStage");
            stageRoot.transform.position = Vector3.zero;

            // 2. マネージャーオブジェクトの作成
            GameObject managerObj = new GameObject("StageCrumbler");
            managerObj.transform.SetParent(stageRoot.transform);
            managerObj.transform.localPosition = Vector3.zero;
            StageCrumbleManager crumbleManager = managerObj.AddComponent<StageCrumbleManager>();
            
            // リストへアクセスするための設定
            SerializedObject serializedManager = new SerializedObject(crumbleManager);
            SerializedProperty chunksProp = serializedManager.FindProperty("stageChunks");
            chunksProp.ClearArray();

            // 3. 円形ステージの生成（キューブを敷き詰める）
            float blockSize = 1.0f; // ブロックの大きさ
            int radiusBlocks = 6;   // 半径（ブロック何個分か）
            
            List<GameObject> generatedBlocks = new List<GameObject>();

            for (int x = -radiusBlocks; x <= radiusBlocks; x++)
            {
                for (int z = -radiusBlocks; z <= radiusBlocks; z++)
                {
                    // 中心からの距離を計算し、円形になるように配置
                    if (new Vector2(x, z).magnitude <= radiusBlocks)
                    {
                        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        block.name = $"StageBlock_{x}_{z}";
                        block.transform.SetParent(stageRoot.transform);
                        // 足場なのでY軸を少し下げて配置
                        block.transform.localPosition = new Vector3(x * blockSize, -0.5f, z * blockSize);
                        block.transform.localScale = new Vector3(blockSize, 1f, blockSize); // 高さは1
                        
                        generatedBlocks.Add(block);
                    }
                }
            }

            // 生成したブロックをマネージャーのリストに登録
            for (int i = 0; i < generatedBlocks.Count; i++)
            {
                chunksProp.InsertArrayElementAtIndex(i);
                chunksProp.GetArrayElementAtIndex(i).objectReferenceValue = generatedBlocks[i];
            }
            
            serializedManager.ApplyModifiedProperties();

            // ユーザーがCtrl+Zで元に戻せるようにする
            Undo.RegisterCreatedObjectUndo(stageRoot, "Generate Destructible Stage");
            Selection.activeGameObject = stageRoot;
            
            Debug.Log("【AntiGravity】崩壊ステージの自動生成と設定が完了しました！");
        }

        [MenuItem("GameObject/AntiGravity/選択したステージを崩壊させる設定", false, 0)]
        public static void ApplyCrumbleToSelectedStage(MenuCommand menuCommand)
        {
            GameObject selectedObj = menuCommand.context as GameObject;
            if (selectedObj == null)
            {
                EditorUtility.DisplayDialog("エラー", "ステージのまとまり（親オブジェクト）を選択してから実行してください。", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(selectedObj, "Apply Stage Crumble");

            // マネージャーの追加
            StageCrumbleManager manager = selectedObj.GetComponent<StageCrumbleManager>();
            if (manager == null)
            {
                manager = selectedObj.AddComponent<StageCrumbleManager>();
            }

            // 子供のオブジェクトを全て取得してリストに追加
            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty chunksProp = serializedManager.FindProperty("stageChunks");
            chunksProp.ClearArray();

            Transform[] allChildren = selectedObj.GetComponentsInChildren<Transform>();
            List<GameObject> pieces = new List<GameObject>();
            
            foreach (Transform child in allChildren)
            {
                if (child == selectedObj.transform) continue;
                
                // メッシュがある（見た目がある）オブジェクトだけを足場として認識
                if (child.GetComponent<MeshRenderer>() != null)
                {
                    pieces.Add(child.gameObject);
                    
                    // コライダー（当たり判定）が無ければ追加（落下させるため必要）
                    if (child.GetComponent<Collider>() == null)
                    {
                        child.gameObject.AddComponent<MeshCollider>().convex = true; // MeshColliderのconvexかBoxColliderを追加
                    }
                }
            }

            for (int i = 0; i < pieces.Count; i++)
            {
                chunksProp.InsertArrayElementAtIndex(i);
                chunksProp.GetArrayElementAtIndex(i).objectReferenceValue = pieces[i];
            }

            serializedManager.ApplyModifiedProperties();

            Debug.Log($"【AntiGravity】現在作成中のステージから {pieces.Count} 個の床パーツを自動認識し、崩落設定を適用しました！");
        }

        [MenuItem("GameObject/AntiGravity/選択したステージの隙間を埋める（Scale拡大）", false, 1)]
        public static void FillStageGaps(MenuCommand menuCommand)
        {
            GameObject selectedObj = menuCommand.context as GameObject;
            if (selectedObj == null)
            {
                EditorUtility.DisplayDialog("エラー", "ステージの親オブジェクトを選択してから実行してください。", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(selectedObj, "Fill Stage Gaps");

            Transform[] allChildren = selectedObj.GetComponentsInChildren<Transform>();
            int count = 0;

            foreach (Transform child in allChildren)
            {
                if (child == selectedObj.transform) continue;
                
                // メッシュがある（床パーツ）オブジェクトのみスケールを調整
                if (child.GetComponent<MeshRenderer>() != null)
                {
                    Vector3 currentScale = child.localScale;
                    // XとZのスケールを1.1倍にして隙間を埋める
                    child.localScale = new Vector3(currentScale.x * 1.1f, currentScale.y, currentScale.z * 1.1f);
                    count++;
                }
            }

            Debug.Log($"【AntiGravity】{count} 個の床パーツのサイズを1.1倍に拡大し、隙間を埋めました！");
        }

        [MenuItem("AntiGravity/床の隙間を落ちないようにする（見た目はそのまま）")]
        public static void ExpandStageColliders()
        {
            StageCrumbleManager manager = Object.FindAnyObjectByType<StageCrumbleManager>();
            if (manager == null)
            {
                EditorUtility.DisplayDialog("エラー", "StageCrumbleManager が設定されているステージが見つかりません。先に崩壊システムを適用してください。", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(manager.gameObject, "Expand Stage Colliders");

            SerializedObject serializedManager = new SerializedObject(manager);
            SerializedProperty chunksProp = serializedManager.FindProperty("stageChunks");

            int count = 0;
            for (int i = 0; i < chunksProp.arraySize; i++)
            {
                GameObject chunk = chunksProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                if (chunk == null) continue;

                // メッシュコライダーがあれば削除してBoxColliderにする（Boxの方がサイズ調整しやすいため）
                MeshCollider mc = chunk.GetComponent<MeshCollider>();
                if (mc != null)
                {
                    DestroyImmediate(mc);
                }

                BoxCollider bc = chunk.GetComponent<BoxCollider>();
                if (bc == null)
                {
                    bc = chunk.AddComponent<BoxCollider>();
                }

                // 当たり判定の横幅と奥行きを1.3倍（30%増し）にして隙間を完全に覆う
                Vector3 newSize = bc.size;
                newSize.x *= 1.3f;
                newSize.z *= 1.3f;
                bc.size = newSize;

                count++;
            }

            Debug.Log($"【AntiGravity】{count} 個の床パーツの「当たり判定（見えない壁）」だけを拡大しました！見た目はそのままですが、もう隙間には落ちません。");
        }

        [MenuItem("AntiGravity/プレイヤーの目線を高くする（+10cm）")]
        public static void RaisePlayerHeight()
        {
            GameObject playerObj = GameObject.Find("XR Origin (XR Rig)");
            if (playerObj == null) playerObj = GameObject.Find("XR Origin");
            
            if (playerObj != null)
            {
                Undo.RecordObject(playerObj.transform, "Raise Player Height");
                playerObj.transform.position += new Vector3(0, 0.1f, 0);
                Debug.Log($"【AntiGravity】プレイヤーの目線を 10cm 高くしました！ 現在の高さ(Y座標): {playerObj.transform.position.y}m");
            }
            else
            {
                EditorUtility.DisplayDialog("エラー", "プレイヤー（XR Origin）が見つかりません。", "OK");
            }
        }

        [MenuItem("AntiGravity/プレイヤーの目線を低くする（-10cm）")]
        public static void LowerPlayerHeight()
        {
            GameObject playerObj = GameObject.Find("XR Origin (XR Rig)");
            if (playerObj == null) playerObj = GameObject.Find("XR Origin");
            
            if (playerObj != null)
            {
                Undo.RecordObject(playerObj.transform, "Lower Player Height");
                playerObj.transform.position -= new Vector3(0, 0.1f, 0);
                Debug.Log($"【AntiGravity】プレイヤーの目線を 10cm 低くしました！ 現在の高さ(Y座標): {playerObj.transform.position.y}m");
            }
            else
            {
                EditorUtility.DisplayDialog("エラー", "プレイヤー（XR Origin）が見つかりません。", "OK");
            }
        }

        [MenuItem("GameObject/AntiGravity/選択したアイテムを高くする（+10cm）", false, 10)]
        public static void RaiseSelected()
        {
            if (Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("エラー", "高くしたいアイテム（剣やボタン）を選択してください。", "OK");
                return;
            }

            foreach (GameObject obj in Selection.gameObjects)
            {
                Undo.RecordObject(obj.transform, "Raise Object");
                obj.transform.position += new Vector3(0, 0.1f, 0);
            }
            Debug.Log($"【AntiGravity】選択した {Selection.gameObjects.Length} 個のアイテムを 10cm 高くしました！");
        }
        
        [MenuItem("GameObject/AntiGravity/選択したアイテムを低くする（-10cm）", false, 11)]
        public static void LowerSelected()
        {
            if (Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("エラー", "低くしたいアイテム（剣やボタン）を選択してください。", "OK");
                return;
            }

            foreach (GameObject obj in Selection.gameObjects)
            {
                Undo.RecordObject(obj.transform, "Lower Object");
                obj.transform.position -= new Vector3(0, 0.1f, 0);
            }
            Debug.Log($"【AntiGravity】選択した {Selection.gameObjects.Length} 個のアイテムを 10cm 低くしました！");
        }

        [MenuItem("AntiGravity/オーラの色設定などを開く")]
        public static void SelectSystemSettings()
        {
            ScreenFader fader = Object.FindAnyObjectByType<ScreenFader>();
            if (fader == null)
            {
                // シーンに存在しない場合は自動生成する
                GameObject sysObj = new GameObject("AntiGravity_SystemSettings");
                fader = sysObj.AddComponent<ScreenFader>();
                Undo.RegisterCreatedObjectUndo(sysObj, "Create System Settings");
                Debug.Log("【AntiGravity】オーラ表示用のシステムが見つからなかったため、自動で作成しました。");
            }
            
            Selection.activeGameObject = fader.gameObject;
            Debug.Log("【AntiGravity】オーラの色設定画面を開きました！右側の Inspector ウィンドウを確認してください。");
        }
    }
}
