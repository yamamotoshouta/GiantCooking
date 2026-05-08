using UnityEngine;

namespace AntiGravity
{
    public class EnemyAI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 1.5f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float swingInterval = 2f;
        
        private Transform player;
        private float nextSwingTime;
        private Rigidbody rb;
        private Transform swordHand;
        private Animator animator;

        private static readonly int IsWalking = Animator.StringToHash("IsWalking");
        private static readonly int Attack = Animator.StringToHash("Attack");

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            GameObject playerObj = GameObject.Find("XR Origin (XR Rig)");
            if (playerObj != null) player = playerObj.transform;
            
            swordHand = transform.Find("Enemy_Sword");
        }

        private void FixedUpdate()
        {
            if (player == null) return;

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
            if (animator != null) animator.SetTrigger(Attack);

            if (swordHand == null) return;
            
            Rigidbody swordRb = swordHand.GetComponent<Rigidbody>();
            if (swordRb != null)
            {
                swordRb.AddForce(transform.forward * 12f + transform.right * 4f, ForceMode.Impulse);
            }
        }
    }
}
