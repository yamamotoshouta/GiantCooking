using UnityEngine;
using UnityEngine.Events;

namespace AntiGravity
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioClip gaugeMaxClip;
        [SerializeField] private AudioClip issenActivateClip;
        [SerializeField] private AudioClip playingBGM;
        [SerializeField] private AudioClip victoryBGM;
        [SerializeField] private AudioClip defeatBGM;

        [Header("Gauge Settings")]
        [SerializeField] private float maxGauge = 1.0f;
        [SerializeField] private float gaugeDecayRate = 0.02f;
        
        public enum GameState { StartMenu, Playing, Victory, Defeat }
        private GameState currentState = GameState.StartMenu;
        public GameState CurrentState => currentState;

        private float currentGauge = 0f;
        private bool isIssenReady = false;
        private bool isIssenActive = false;

        public float CurrentGauge => currentGauge;
        public bool IsIssenReady => isIssenReady;
        public bool IsIssenActive => isIssenActive;

        [Header("Player Settings")]
        [SerializeField] private float playerMaxHP = 3f;
        private float currentPlayerHP;

        public float CurrentPlayerHP => currentPlayerHP;
        public float PlayerMaxHP => playerMaxHP;

        public UnityEvent<float> OnPlayerHPChanged;
        public UnityEvent OnPlayerHit;
        public UnityEvent<float> OnGaugeChanged;
        public UnityEvent OnGaugeMaxed;
        public UnityEvent OnIssenActivated;
        public UnityEvent OnGameStarted;
        public UnityEvent OnVictory;
        public UnityEvent OnDefeat;

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

        private void Start()
        {
            // Initial state is StartMenu
            currentState = GameState.StartMenu;
        }

        public void StartGame()
        {
            currentState = GameState.Playing;
            currentPlayerHP = playerMaxHP;
            OnPlayerHPChanged?.Invoke(1.0f);
            ResetGauge();
            OnGameStarted?.Invoke();
            
            if (bgmSource != null && playingBGM != null)
            {
                bgmSource.clip = playingBGM;
                bgmSource.loop = true;
                bgmSource.Play();
            }
            
            Debug.Log("Game Started!");
        }

        public void TriggerVictory()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.Victory;
                OnVictory?.Invoke();
                
                if (bgmSource != null)
                {
                    bgmSource.Stop();
                    if (victoryBGM != null) bgmSource.PlayOneShot(victoryBGM);
                }
                
                Debug.Log("Victory!");
            }
        }

        public void TakeDamage(float amount)
        {
            if (currentState != GameState.Playing) return;

            currentPlayerHP -= amount;
            OnPlayerHPChanged?.Invoke(currentPlayerHP / playerMaxHP);
            OnPlayerHit?.Invoke();

            Debug.Log($"Player Hit! HP: {currentPlayerHP}");

            if (currentPlayerHP <= 0)
            {
                TriggerDefeat();
            }
        }

        public void TriggerDefeat()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.Defeat;
                OnDefeat?.Invoke();
                
                if (bgmSource != null)
                {
                    bgmSource.Stop();
                    if (defeatBGM != null) bgmSource.PlayOneShot(defeatBGM);
                }
                
                Debug.Log("Defeat...");
            }
        }

        private void Update()
        {
            if (currentState != GameState.Playing) return;

            if (currentGauge > 0 && !isIssenReady && !isIssenActive)
            {
                currentGauge -= gaugeDecayRate * Time.deltaTime;
                currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);
                OnGaugeChanged?.Invoke(currentGauge / maxGauge);
            }
        }

        public void AddGauge(float amount)
        {
            if (currentState != GameState.Playing || isIssenReady || isIssenActive) return;

            currentGauge += amount;
            currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);
            OnGaugeChanged?.Invoke(currentGauge / maxGauge);

            if (currentGauge >= maxGauge)
            {
                isIssenReady = true;
                OnGaugeMaxed?.Invoke();
                if (audioSource != null && gaugeMaxClip != null)
                {
                    audioSource.PlayOneShot(gaugeMaxClip);
                }
                Debug.Log("Kiwami Gauge MAX - Ready for Issen!");
            }
        }

        public void ResetGauge()
        {
            currentGauge = 0f;
            isIssenReady = false;
            isIssenActive = false;
            OnGaugeChanged?.Invoke(0f);
        }
        
        public bool TryActivateIssen()
        {
            if (isIssenReady && !isIssenActive)
            {
                isIssenActive = true;
                isIssenReady = false;
                OnIssenActivated?.Invoke();
                
                if (audioSource != null && issenActivateClip != null)
                {
                    audioSource.PlayOneShot(issenActivateClip);
                }

                if (AntiGravity.System.TimeManager.Instance != null)
                {
                    AntiGravity.System.TimeManager.Instance.DoSlowMotion(1.5f, 0.3f);
                }

                Debug.Log("Issen Activated! Next hit will blast the enemy.");
                return true;
            }
            return false;
        }
    }
}
