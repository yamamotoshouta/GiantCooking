using UnityEngine;
using System.Collections;
using AntiGravity.System;

namespace AntiGravity
{
    public class ZeroBossAI : MonoBehaviour
    {
        public enum BossPhase { Phase1_Observation, Phase2_Distortion, Phase3_ZERO }
        public enum BossState { Idle, Approaching, Attacking, Down, Teleporting }

        [Header("Status")]
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private float currentHealth;
        [SerializeField] private BossPhase currentPhase = BossPhase.Phase1_Observation;
        [SerializeField] private BossState currentState = BossState.Idle;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2.0f;
        [SerializeField] private float attackRange = 3.5f;
        [SerializeField] private float teleportDistance = 5.0f;
        
        [Header("Effects & Prefabs")]
        [SerializeField] private GameObject redSkyVolume;
        [SerializeField] private GameObject darknessVolume;
        [SerializeField] private GameObject shockwavePrefab;
        [SerializeField] private GameObject teleportEffectPrefab;
        [SerializeField] private AudioSource bossAudio;
        [SerializeField] private AudioClip phase2SE;
        [SerializeField] private AudioClip phase3SE;

        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform attackPoint;

        private Rigidbody rb;
        private bool isDown = false;
        private float nextActionTime;
        
        // Animation Hashes
        private static readonly int AnimState = Animator.StringToHash("State"); // 0:Idle, 1:Walk, 2:Attack, 3:Down, 4:Teleport
        private static readonly int AnimAttackType = Animator.StringToHash("AttackType");
        private static readonly int AnimDie = Animator.StringToHash("Die");

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            currentHealth = maxHealth;
        }

        private void Start()
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.Find("XR Origin (XR Rig)");
                if (playerObj != null) player = playerObj.transform;
            }

            if (JustEvasionSystem.Instance != null)
            {
                JustEvasionSystem.Instance.OnJustEvasionSuccess.AddListener(OnJustEvasionReceived);
            }
        }

        private void Update()
        {
            if (isDown || currentState == BossState.Teleporting) return;

            UpdatePhase();
            HandleAI();
        }

        private void UpdatePhase()
        {
            float hpRatio = currentHealth / maxHealth;

            if (hpRatio < 0.3f && currentPhase != BossPhase.Phase3_ZERO)
            {
                TransitionToPhase(BossPhase.Phase3_ZERO);
            }
            else if (hpRatio < 0.7f && currentPhase == BossPhase.Phase1_Observation)
            {
                TransitionToPhase(BossPhase.Phase2_Distortion);
            }
        }

        private void TransitionToPhase(BossPhase next)
        {
            currentPhase = next;
            if (next == BossPhase.Phase2_Distortion)
            {
                if (BossVisualEffectManager.Instance != null) BossVisualEffectManager.Instance.ApplyPhase2Effects();
                if (bossAudio != null && phase2SE != null) bossAudio.PlayOneShot(phase2SE);
                moveSpeed *= 1.2f;
            }
            else if (next == BossPhase.Phase3_ZERO)
            {
                if (BossVisualEffectManager.Instance != null) BossVisualEffectManager.Instance.ApplyPhase3Effects();
                if (bossAudio != null && phase3SE != null) bossAudio.PlayOneShot(phase3SE);
                moveSpeed *= 1.5f;
            }
            Debug.Log($"<color=red>ZERO Phase: {next}</color>");
        }

        private void HandleAI()
        {
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            if (Time.time < nextActionTime) return;

            if (distance > attackRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                DecideAction();
            }
        }

        private void MoveTowardsPlayer()
        {
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;
            rb.linearVelocity = dir * moveSpeed;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.1f);
            
            if (animator != null) animator.SetInteger(AnimState, 1);
        }

        private void DecideAction()
        {
            rb.linearVelocity = Vector3.zero;
            
            // Phase 2以降は一定確率でテレポート
            if (currentPhase >= BossPhase.Phase2_Distortion && Random.value < 0.3f)
            {
                StartCoroutine(TeleportRoutine());
                return;
            }

            StartAttack();
        }

        private void StartAttack()
        {
            currentState = BossState.Attacking;
            
            int attackType = 0;
            if (currentPhase == BossPhase.Phase2_Distortion) attackType = Random.Range(0, 2);
            if (currentPhase == BossPhase.Phase3_ZERO) attackType = Random.Range(0, 4);

            if (animator != null)
            {
                animator.SetInteger(AnimAttackType, attackType);
                animator.SetInteger(AnimState, 2);
            }

            if (JustEvasionSystem.Instance != null)
            {
                JustEvasionSystem.Instance.NotifyAttackStart();
            }

            StartCoroutine(AttackRoutine(attackType));
        }

        private IEnumerator AttackRoutine(int type)
        {
            // 攻撃発生までのディレイ（アニメーションに合わせる）
            yield return new WaitForSeconds(1.0f);

            // Phase 2: 地面衝撃波
            if (currentPhase >= BossPhase.Phase2_Distortion && type == 1)
            {
                SpawnShockwave();
            }

            yield return new WaitForSeconds(0.5f);
            
            currentState = BossState.Idle;
            nextActionTime = Time.time + (currentPhase == BossPhase.Phase3_ZERO ? 0.5f : 1.5f);
            if (animator != null) animator.SetInteger(AnimState, 0);
        }

        private void SpawnShockwave()
        {
            if (shockwavePrefab != null && attackPoint != null)
            {
                Instantiate(shockwavePrefab, attackPoint.position, Quaternion.identity);
            }
        }

        private IEnumerator TeleportRoutine()
        {
            currentState = BossState.Teleporting;
            if (animator != null) animator.SetInteger(AnimState, 4);

            if (teleportEffectPrefab != null) Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);

            yield return new WaitForSeconds(0.5f);

            // プレイヤーの後ろに回り込む
            Vector3 targetPos = player.position - player.forward * 3f;
            targetPos.y = transform.position.y;
            transform.position = targetPos;
            transform.LookAt(player);

            if (teleportEffectPrefab != null) Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);

            yield return new WaitForSeconds(0.2f);

            currentState = BossState.Idle;
            StartAttack(); // テレポート後は即攻撃
        }

        public void TakeDamage(float damage)
        {
            float actualDamage = isDown ? damage : damage * 0.1f;
            currentHealth -= actualDamage;

            if (currentHealth <= 0) Die();
        }

        private void OnJustEvasionReceived()
        {
            if (currentState == BossState.Attacking)
            {
                StopAllCoroutines();
                StartCoroutine(DownRoutine());
            }
        }

        private IEnumerator DownRoutine()
        {
            isDown = true;
            currentState = BossState.Down;
            if (animator != null) animator.SetInteger(AnimState, 3);
            rb.linearVelocity = Vector3.zero;

            yield return new WaitForSeconds(3.0f);

            isDown = false;
            currentState = BossState.Idle;
            if (animator != null) animator.SetInteger(AnimState, 0);
            nextActionTime = Time.time + 0.5f;
        }

        private void Die()
        {
            Debug.Log("ZERO Defeated. '...Were you an agent too?'");
            StopAllCoroutines();
            if (animator != null) animator.SetTrigger(AnimDie);
            // ここでエンディング演出へ
        }
    }
}
