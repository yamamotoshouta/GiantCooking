using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace AntiGravity
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
    public class Sword : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float bounceForce = 5f;
        [SerializeField] private float hapticIntensity = 0.5f;
        [SerializeField] private float hapticDuration = 0.1f;

        [Header("Audio & Visual Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clashClip;
        [SerializeField] private AudioClip issenClip;
        [SerializeField] private GameObject sparkPrefab;
        
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;
        private Rigidbody rb;

        private void Awake()
        {
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Sword"))
            {
                HandleSwordClash(collision);
            }
            else if (collision.gameObject.CompareTag("Enemy"))
            {
                HandleEnemyHit(collision);
            }
        }

        private void HandleSwordClash(Collision collision)
        {
            // Calculate bounce direction
            Vector3 bounceDir = (transform.position - collision.contacts[0].point).normalized;
            rb.AddForce(bounceDir * bounceForce, ForceMode.Impulse);

            // Trigger Haptics
            TriggerHaptics();

            // Add to Gauge
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGauge(0.05f);
            }

            // Play Sound & VFX
            if (audioSource != null && clashClip != null)
            {
                audioSource.PlayOneShot(clashClip);
            }

            if (sparkPrefab != null)
            {
                Instantiate(sparkPrefab, collision.contacts[0].point, Quaternion.identity);
            }

            Debug.Log("Sword Clash!");
        }

        private void HandleEnemyHit(Collision collision)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsIssenActive)
            {
                // Issen blowback
                Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 blowbackDir = (collision.transform.position - transform.position).normalized;
                    blowbackDir.y = 0.2f; // Slight upward lift
                    enemyRb.AddForce(blowbackDir * 20f, ForceMode.Impulse);
                    
                    // Play Sound
                    if (audioSource != null && issenClip != null)
                    {
                        audioSource.PlayOneShot(issenClip);
                    }

                    GameManager.Instance.ResetGauge();
                    Debug.Log("Issen Blast!");
                }
            }
        }

        private void TriggerHaptics()
        {
            if (interactable != null && interactable.isSelected)
            {
                var interactor = interactable.firstInteractorSelecting;
                if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
                {
                    controllerInteractor.xrController.SendHapticImpulse(hapticIntensity, hapticDuration);
                }
            }
        }
    }
}
