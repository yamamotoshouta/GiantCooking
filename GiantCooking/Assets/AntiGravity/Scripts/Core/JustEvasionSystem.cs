using UnityEngine;
using UnityEngine.Events;
using AntiGravity.System;

namespace AntiGravity
{
    public class JustEvasionSystem : MonoBehaviour
    {
        public static JustEvasionSystem Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float evasionSpeedThreshold = 1.5f; // 回避とみなす最小速度
        [SerializeField] private float detectionWindow = 0.5f;     // ジャスト判定の猶予時間
        [SerializeField] private float slowMoDuration = 2.0f;
        [SerializeField] private float slowMoScale = 0.1f;

        [Header("References")]
        [SerializeField] private Transform playerHead; // XR OriginのCamera

        public UnityEvent OnJustEvasionSuccess;

        private Vector3 lastPosition;
        private float currentSpeed;
        private bool isCurrentlyEvading;
        private float lastAttackTime;
        private bool isAttackActive;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            if (playerHead == null)
            {
                playerHead = Camera.main.transform;
            }
        }

        private void Start()
        {
            lastPosition = playerHead.position;
        }

        private void Update()
        {
            // プレイヤーの移動速度を計算（簡易的な回避検知）
            Vector3 delta = playerHead.position - lastPosition;
            currentSpeed = delta.magnitude / Time.deltaTime;
            lastPosition = playerHead.position;

            isCurrentlyEvading = currentSpeed > evasionSpeedThreshold;

            // ボスが攻撃中かつプレイヤーが高速移動（回避）した場合
            if (isAttackActive && isCurrentlyEvading)
            {
                TriggerJustEvasion();
            }
        }

        // ボス側から攻撃開始時に呼ばれる
        public void NotifyAttackStart()
        {
            isAttackActive = true;
            CancelInvoke(nameof(EndAttackWindow));
            Invoke(nameof(EndAttackWindow), detectionWindow);
        }

        private void EndAttackWindow()
        {
            isAttackActive = false;
        }

        private void TriggerJustEvasion()
        {
            if (!isAttackActive) return;

            isAttackActive = false;
            Debug.Log("<color=cyan>JUST EVASION!</color>");

            // スローモーション発動
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.DoSlowMotion(slowMoDuration, slowMoScale);
            }

            OnJustEvasionSuccess?.Invoke();
        }
    }
}
