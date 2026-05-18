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
        [SerializeField] private float playerDamage = 1.0f;

        [Header("Audio & Visual Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip clashClip;
        [SerializeField] private AudioClip issenClip;
        [SerializeField] private GameObject sparkPrefab;
        [SerializeField] private Renderer swordRenderer;
        [SerializeField] private Color issenColor = Color.yellow;
        [SerializeField] private TrailRenderer swordTrail;
        [SerializeField] private ParticleSystem auraParticles;
        
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

            if (swordTrail != null) swordTrail.enabled = false;
            if (auraParticles != null) auraParticles.Stop();
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
            HandleHit(collision.gameObject, collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position);
        }

        private void OnTriggerEnter(Collider other)
        {
            // For triggers, we use the closest point or the object position as a contact point
            HandleHit(other.gameObject, other.ClosestPointOnBounds(transform.position));
        }

        private void HandleHit(GameObject hitObj, Vector3 contactPoint)
        {
            if (hitObj.CompareTag("Sword"))
            {
                HandleSwordClash(hitObj, contactPoint);
            }
            else if (hitObj.CompareTag("Enemy"))
            {
                HandleEnemyHit(hitObj, contactPoint);
            }
            else if (hitObj.CompareTag("MainCamera") || hitObj.CompareTag("Player"))
            {
                HandlePlayerHit(hitObj, contactPoint);
            }
        }

        private void HandleSwordClash(GameObject otherSword, Vector3 contactPoint)
        {
            EnemyAI enemy = otherSword.GetComponentInParent<EnemyAI>();
            EnemyAI myEnemy = GetComponentInParent<EnemyAI>();

            // Is the enemy doing an unblockable attack?
            if (enemy != null && enemy.CurrentAttackType == EnemyAI.AttackType.Unblockable)
            {
                // Player's sword hitting an unblockable enemy sword
                if (rb != null) rb.AddForce((transform.position - contactPoint).normalized * bounceForce * 1.5f, ForceMode.Impulse);
                TriggerHaptics(1.0f); // Heavy haptic to indicate guard break
                if (audioSource != null && clashClip != null) audioSource.PlayOneShot(clashClip, 1.0f);
                if (sparkPrefab != null) Instantiate(sparkPrefab, contactPoint, Quaternion.identity);
                return; // Do not add gauge, do not hit stop, do not recoil enemy
            }

            if (myEnemy != null && myEnemy.CurrentAttackType == EnemyAI.AttackType.Unblockable)
            {
                // Enemy's unblockable sword hitting player's sword
                // Enemy sword does NOT bounce, it pushes right through
                return;
            }

            // --- Normal Clash ---
            // Calculate bounce direction
            Vector3 bounceDir = (transform.position - contactPoint).normalized;
            if (rb != null) rb.AddForce(bounceDir * bounceForce, ForceMode.Impulse);

            // Trigger Haptics
            TriggerHaptics(0.5f);

            // Hit Stop
            if (AntiGravity.System.TimeManager.Instance != null)
            {
                AntiGravity.System.TimeManager.Instance.DoHitStop(0.05f);
            }

            // Add to Gauge
            if (enemy != null)
            {
                if (enemy.IsInvincible) return;
                enemy.TriggerRecoil();
            }

            if (GameManager.Instance != null)
            {
                // Heavy attack gives slightly more gauge when parried
                float gaugeToAdd = (enemy != null && enemy.CurrentAttackType == EnemyAI.AttackType.Heavy) ? 0.08f : 0.05f;
                GameManager.Instance.AddGauge(gaugeToAdd);
            }

            // Play Sound & VFX
            if (audioSource != null && clashClip != null)
            {
                audioSource.PlayOneShot(clashClip, 0.6f);
            }

            if (sparkPrefab != null)
            {
                Instantiate(sparkPrefab, contactPoint, Quaternion.identity);
            }
        }

        private void HandleEnemyHit(GameObject enemyObj, Vector3 contactPoint)
        {
            EnemyAI enemy = enemyObj.GetComponent<EnemyAI>();
            if (enemy != null && enemy.IsInvincible) return;

            if (GameManager.Instance != null && GameManager.Instance.IsIssenActive)
            {
                Rigidbody enemyRb = enemyObj.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 blowbackDir = (enemyObj.transform.position - transform.position).normalized;
                    blowbackDir.y = 0.2f;
                    enemyRb.AddForce(blowbackDir * 20f, ForceMode.Impulse);
                    
                    if (audioSource != null && issenClip != null)
                    {
                        audioSource.PlayOneShot(issenClip);
                    }

                    GameManager.Instance.ResetGauge();
                }
            }
        }

        private void HandlePlayerHit(GameObject playerObj, Vector3 contactPoint)
        {
            // Only enemy swords can damage the player
            if (gameObject.name.Contains("Enemy") && GameManager.Instance != null)
            {
                EnemyAI myEnemy = GetComponentInParent<EnemyAI>();
                float damageToDeal = playerDamage;

                // Unblockable attacks deal double damage!
                if (myEnemy != null && myEnemy.CurrentAttackType == EnemyAI.AttackType.Unblockable)
                {
                    damageToDeal *= 2.0f;
                }

                GameManager.Instance.TakeDamage(damageToDeal);
                
                // Audio feedback for being hit
                if (audioSource != null && clashClip != null)
                {
                    audioSource.PlayOneShot(clashClip, 0.8f);
                }

                Debug.Log($"Player was hit by Enemy Sword! Damage: {damageToDeal}");
            }
        }

        private void EnableIssenVisuals()
        {
            if (swordMaterial != null)
            {
                swordMaterial.SetColor("_EmissionColor", issenColor);
                swordMaterial.EnableKeyword("_EMISSION");
            }

            if (swordTrail != null) swordTrail.enabled = true;
            if (auraParticles != null) auraParticles.Play();
        }

        private void HandleGaugeResetVisuals(float ratio)
        {
            if (ratio <= 0)
            {
                if (swordMaterial != null)
                {
                    swordMaterial.SetColor("_EmissionColor", originalColor);
                }

                if (swordTrail != null) swordTrail.enabled = false;
                if (auraParticles != null) auraParticles.Stop();
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
