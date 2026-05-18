using UnityEngine;
using System.Collections;

namespace AntiGravity
{
    public class EnemyAI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 1.5f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float swingInterval = 2f;
        
        [Header("Attack Settings")]
        [SerializeField] private Renderer bodyRenderer; // For flashing red during unblockable
        [SerializeField] private Color unblockableColor = Color.red;
        private Color originalBodyColor;
        
        public enum AttackType { Normal, Heavy, Unblockable }
        private AttackType currentAttackType = AttackType.Normal;
        public AttackType CurrentAttackType => currentAttackType;

        private Transform player;
        private float nextSwingTime;
        private bool isRecoiling = false;
        public bool IsInvincible => isRecoiling;

        private Rigidbody rb;
        private Transform swordHand;
        private Animator animator;

        private static readonly int IsWalking = Animator.StringToHash("IsWalking");
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Recoil = Animator.StringToHash("Recoil");

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            GameObject playerObj = GameObject.Find("XR Origin (XR Rig)");
            if (playerObj != null) player = playerObj.transform;
            
            swordHand = transform.Find("Enemy_Sword");

            if (bodyRenderer == null)
            {
                // Try to find a renderer in children if not assigned
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            if (bodyRenderer != null)
            {
                originalBodyColor = bodyRenderer.material.color;
            }
        }

        public void TriggerRecoil(float duration = 1.0f)
        {
            if (isRecoiling) return;
            StartCoroutine(RecoilRoutine(duration));
        }

        private IEnumerator RecoilRoutine(float duration)
        {
            isRecoiling = true;
            if (animator != null) animator.SetTrigger(Recoil);
            
            yield return new WaitForSeconds(duration);
            
            isRecoiling = false;
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                if (animator != null) animator.SetBool(IsWalking, false);
                return;
            }

            if (isRecoiling || player == null) return;

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > attackRange)
            {
                // Move towards player
                Vector3 moveDir = (player.position - transform.position).normalized;
                moveDir.y = 0;
                rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
                
                // Rotate to face player
                Quaternion lookRot = Quaternion.LookRotation(moveDir);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRot, 0.1f));

                if (animator != null) animator.SetBool(IsWalking, true);
            }
            else
            {
                if (animator != null) animator.SetBool(IsWalking, false);

                // Attack range
                if (Time.time > nextSwingTime)
                {
                    PerformSwing();
                    nextSwingTime = Time.time + swingInterval;
                }
            }
        }

        private void PerformSwing()
        {
            // Randomize attack type (25% Unblockable, 35% Heavy, 40% Normal)
            float rand = Random.value;
            if (rand < 0.25f) currentAttackType = AttackType.Unblockable;
            else if (rand < 0.60f) currentAttackType = AttackType.Heavy;
            else currentAttackType = AttackType.Normal;

            if (currentAttackType == AttackType.Unblockable)
            {
                StartCoroutine(UnblockableWarningRoutine());
                return;
            }

            ExecuteSwingLogic();
        }

        private IEnumerator UnblockableWarningRoutine()
        {
            // Flash red as a warning
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = unblockableColor;
                if (bodyRenderer.material.HasProperty("_EmissionColor"))
                {
                    bodyRenderer.material.EnableKeyword("_EMISSION");
                    bodyRenderer.material.SetColor("_EmissionColor", unblockableColor);
                }
            }
            
            // Wait for player to see the warning and react (0.8 seconds is enough time to dodge)
            yield return new WaitForSeconds(0.8f);
            
            // Reset color
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = originalBodyColor;
                if (bodyRenderer.material.HasProperty("_EmissionColor"))
                {
                    bodyRenderer.material.SetColor("_EmissionColor", Color.black);
                }
            }

            ExecuteSwingLogic();
        }

        private void ExecuteSwingLogic()
        {
            if (animator != null) animator.SetTrigger(Attack);

            if (swordHand == null) return;
            
            Rigidbody swordRb = swordHand.GetComponent<Rigidbody>();
            if (swordRb != null && !swordRb.isKinematic)
            {
                Vector3 force = transform.forward * 12f + transform.right * 4f;
                
                // Adjust physical force based on attack type
                if (currentAttackType == AttackType.Heavy) force *= 1.5f;
                if (currentAttackType == AttackType.Unblockable) force *= 2.0f;

                swordRb.AddForce(force, ForceMode.Impulse);
            }

            StartCoroutine(ResetAttackTypeRoutine());
        }

        private IEnumerator ResetAttackTypeRoutine()
        {
            // Reset attack type after the swing is complete
            yield return new WaitForSeconds(1.5f);
            currentAttackType = AttackType.Normal;
        }
    }
}
