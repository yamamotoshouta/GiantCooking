using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AntiGravity.System
{
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        [SerializeField] private Image fadeImage;
        [SerializeField] private float defaultFadeDuration = 1.0f;

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
