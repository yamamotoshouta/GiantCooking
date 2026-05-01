using UnityEngine;
using UnityEngine.Events;

namespace AntiGravity
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Gauge Settings")]
        [SerializeField] private float maxGauge = 1.0f;
        [SerializeField] private float gaugeDecayRate = 0.02f;
        
        private float currentGauge = 0f;
        private bool isIssenActive = false;

        public float CurrentGauge => currentGauge;
        public bool IsIssenActive => isIssenActive;

        public UnityEvent<float> OnGaugeChanged;
        public UnityEvent OnGaugeMaxed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (currentGauge > 0 && !isIssenActive)
            {
                currentGauge -= gaugeDecayRate * Time.deltaTime;
                currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);
                OnGaugeChanged?.Invoke(currentGauge / maxGauge);
            }
        }

        public void AddGauge(float amount)
        {
            if (isIssenActive) return;

            currentGauge += amount;
            currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);
            OnGaugeChanged?.Invoke(currentGauge / maxGauge);

            if (currentGauge >= maxGauge)
            {
                isIssenActive = true;
                OnGaugeMaxed?.Invoke();
                Debug.Log("Kiwami Gauge MAX!");
            }
        }

        public void ResetGauge()
        {
            currentGauge = 0f;
            isIssenActive = false;
            OnGaugeChanged?.Invoke(0f);
        }
        
        // This would be called by player input (trigger)
        public void TryActivateIssen()
        {
            if (currentGauge >= maxGauge)
            {
                // In a real implementation, this might trigger a visual effect on the sword
                Debug.Log("Issen Ready - Hit the enemy to blast them!");
            }
        }
    }
}
