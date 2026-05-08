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
        [SerializeField] private float minHapticIntensity = 0.1f;
        [SerializeField] private float maxHapticIntensity = 0.8f;
        [SerializeField] private float hapticDuration = 0.1f;

        [Header("Audio & Visual Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clashClip;
        [SerializeField] private AudioClip issenClip;
        [SerializeField] private GameObject sparkPrefab;
        [SerializeField] private Renderer swordRenderer;
        [SerializeField] private Color issenColor = Color.yellow;
        
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable;
        private Rigidbody rb;
        private Material swordMaterial;
        private Color originalColor;

        private void Awake()
        {
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();
            
            if (swordRenderer != null)
            {
                swordMaterial = swordRenderer.material;
                originalColor = swordMaterial.GetColor("_EmissionColor");
            }
        }

        private void OnEnable()
        {
            if (interactable != null)
            {
                interactable.activated.AddListener(OnTriggerPulled);
            }
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnIssenActivated.AddListener(EnableIssenVisuals);
                GameManager.Instance.OnGaugeChanged.AddListener(HandleGaugeResetVisuals);
            }
        }

        private void OnDisable()
        {
            if (interactable != null)
            {
                interactable.activated.RemoveListener(OnTriggerPulled);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnIssenActivated.RemoveListener(EnableIssenVisuals);
                GameManager.Instance.OnGaugeChanged.RemoveListener(HandleGaugeResetVisuals);
            }
        }

        private void OnTriggerPulled(ActivateEventArgs args)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TryActivateIssen();
            }
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
            // Calculate relative velocity for dynamic haptics
            float relativeVel = collision.relativeVelocity.magnitude;
            float intensity = Mathf.Clamp(relativeVel / 10f, minHapticIntensity, maxHapticIntensity);

            // Calculate bounce direction
            Vector3 bounceDir = (transform.position - collision.contacts[0].point).normalized;
            rb.AddForce(bounceDir * bounceForce, ForceMode.Impulse);

            // Trigger Haptics
            TriggerHaptics(intensity);

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

            Debug.Log($"Sword Clash! Intensity: {intensity}");
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

        private void EnableIssenVisuals()
        {
            if (swordMaterial != null)
            {
                swordMaterial.SetColor("_EmissionColor", issenColor);
                swordMaterial.EnableKeyword("_EMISSION");
            }
        }

        private void HandleGaugeResetVisuals(float ratio)
        {
            if (ratio <= 0 && swordMaterial != null)
            {
                swordMaterial.SetColor("_EmissionColor", originalColor);
            }
        }

        private void TriggerHaptics(float intensity)
        {
            if (interactable != null && interactable.isSelected)
            {
                var interactor = interactable.firstInteractorSelecting;
                if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
                {
                    controllerInteractor.xrController.SendHapticImpulse(intensity, hapticDuration);
                }
            }
        }
    }
}
