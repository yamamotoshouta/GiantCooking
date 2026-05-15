using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AntiGravity.System
{
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        [SerializeField] private Image fadeImage;
        [SerializeField] private Image damageImage;
        [SerializeField] private float defaultFadeDuration = 1.0f;
        [SerializeField] private float flashSpeed = 4f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (fadeImage == null)
            {
                // Create a full-screen image if not assigned
                GameObject canvasObj = new GameObject("FadeCanvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                
                GameObject imageObj = new GameObject("FadeImage");
                imageObj.transform.SetParent(canvasObj.transform);
                fadeImage = imageObj.AddComponent<Image>();
                fadeImage.color = Color.clear;
                fadeImage.rectTransform.anchorMin = Vector2.zero;
                fadeImage.rectTransform.anchorMax = Vector2.one;
                fadeImage.rectTransform.sizeDelta = Vector2.zero;
                
                DontDestroyOnLoad(canvasObj);
            }

            if (damageImage == null)
            {
                // Create a damage flash image on the same canvas if not assigned
                GameObject imageObj = new GameObject("DamageFlashImage");
                imageObj.transform.SetParent(fadeImage.transform.parent);
                damageImage = imageObj.AddComponent<Image>();
                damageImage.color = Color.clear;
                damageImage.rectTransform.anchorMin = Vector2.zero;
                damageImage.rectTransform.anchorMax = Vector2.one;
                damageImage.rectTransform.sizeDelta = Vector2.zero;
                damageImage.raycastTarget = false; // Don't block clicks
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerHit.AddListener(TriggerFlash);
            }
        }

        public void TriggerFlash()
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (damageImage == null) yield break;

            damageImage.color = new Color(1, 0, 0, 0.4f); // Red with 40% alpha
            while (damageImage.color.a > 0.01f)
            {
                Color c = damageImage.color;
                c.a = Mathf.MoveTowards(c.a, 0, flashSpeed * Time.deltaTime);
                damageImage.color = c;
                yield return null;
            }
            damageImage.color = Color.clear;
        }

        public Coroutine FadeIn(float duration = -1)
        {
            float d = duration < 0 ? defaultFadeDuration : duration;
            return StartCoroutine(FadeRoutine(1, 0, d));
        }

        public Coroutine FadeOut(float duration = -1)
        {
            float d = duration < 0 ? defaultFadeDuration : duration;
            return StartCoroutine(FadeRoutine(0, 1, d));
        }

        private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration)
        {
            float elapsed = 0;
            Color color = fadeImage.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                fadeImage.color = color;
                yield return null;
            }

            color.a = endAlpha;
            fadeImage.color = color;
        }
    }
}
