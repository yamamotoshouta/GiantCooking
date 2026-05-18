using UnityEngine;
using UnityEngine.Events;
using System.Collections;

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
        [SerializeField] private AudioClip titleBGM;
        [SerializeField] private AudioClip playingBGM;
        [SerializeField] private AudioClip victoryBGM;
        [SerializeField] private AudioClip defeatBGM;
        [SerializeField] private AudioClip heartbeatClip; // Added for damage feedback

        [Header("Gauge Settings")]
        [SerializeField] private float maxGauge = 1.0f;
        [SerializeField] private float gaugeDecayRate = 0.02f;
        
        public enum GameState { StartMenu, Playing, Victory, Defeat }
        private GameState currentState = GameState.StartMenu;
        public GameState CurrentState => currentState;
        private Coroutine bgmFadeCoroutine;

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
            
            if (bgmSource != null && titleBGM != null)
            {
                PlayBGM(titleBGM, true);
            }
        }

        public void PlayBGM(AudioClip clip, bool loop)
        {
            if (bgmSource == null || clip == null) return;
            
            // If already playing the same clip, don't restart
            if (bgmSource.isPlaying && bgmSource.clip == clip) return;

            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeBGM(clip, loop));
        }

        private IEnumerator FadeBGM(AudioClip clip, bool loop)
        {
            float duration = 1.0f;
            float startVolume = bgmSource.volume;

            if (bgmSource.isPlaying)
            {
                // Fade out
                for (float t = 0; t < duration; t += Time.deltaTime)
                {
                    bgmSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
                    yield return null;
                }
                bgmSource.Stop();
            }

            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();

            // Fade in
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                bgmSource.volume = Mathf.Lerp(0, startVolume, t / duration);
                yield return null;
            }
            bgmSource.volume = startVolume;
        }

        public void StartGame()
        {
            currentState = GameState.Playing;
            currentPlayerHP = playerMaxHP;
            OnPlayerHPChanged?.Invoke(1.0f);
            ResetGauge();
            OnGameStarted?.Invoke();
            
            if (playingBGM != null)
            {
                PlayBGM(playingBGM, true);
            }
            
            ResetAllPositions();
            
            Debug.Log("Game Started!");
        }

        private void ResetAllPositions()
        {
            var handlers = FindObjectsOfType<FallOutHandler>();
            foreach (var handler in handlers)
            {
                handler.ResetPosition();
            }
        }

        public void TriggerVictory()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.Victory;
                OnVictory?.Invoke();
                
                if (victoryBGM != null) PlayBGM(victoryBGM, false);
                else bgmSource.Stop();
                
                Debug.Log("Victory!");
            }
        }

        public void TakeDamage(float amount)
        {
            if (currentState != GameState.Playing) return;

            currentPlayerHP -= amount;
            OnPlayerHPChanged?.Invoke(currentPlayerHP / playerMaxHP);
            OnPlayerHit?.Invoke();
            
            if (audioSource != null && heartbeatClip != null)
            {
                audioSource.PlayOneShot(heartbeatClip, 1.0f);
            }

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
                
                if (defeatBGM != null) PlayBGM(defeatBGM, false);
                else bgmSource.Stop();
                
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
