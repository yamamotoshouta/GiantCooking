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

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            GameObject playerObj = GameObject.Find("XR Origin (XR Rig)");
            if (playerObj != null) player = playerObj.transform;
            
            // Find the sword holder (the primitive or model part)
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
            }
            else
            {
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
            if (swordHand == null) return;
            
            // Simple physics-based swing: Apply torque or rotate the holder
            Rigidbody swordRb = swordHand.GetComponent<Rigidbody>();
            if (swordRb != null)
            {
                // Push the sword forward/sideways
                swordRb.AddForce(transform.forward * 10f + transform.right * 5f, ForceMode.Impulse);
                Debug.Log("Enemy Swings!");
            }
        }
    }
}
