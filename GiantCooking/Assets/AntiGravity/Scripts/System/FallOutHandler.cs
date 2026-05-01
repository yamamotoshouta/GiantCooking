using UnityEngine;

namespace AntiGravity
{
    public class FallOutHandler : MonoBehaviour
    {
        [SerializeField] private float fallThreshold = -10f;
        [SerializeField] private string targetTag = "Player"; // Or Enemy

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            if (transform.position.y < fallThreshold)
            {
                OnFallOut();
            }
        }

        private void OnFallOut()
        {
            Debug.Log(gameObject.name + " fell out!");
            // Reset position for demo purposes
            transform.position = startPosition;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            // In a real game, you would trigger a win/loss screen here
        }
    }
}
