using UnityEngine;
using System.Collections;

namespace AntiGravity
{
    /// <summary>
    /// A simple controller to handle the "Fall Out" vignette effect and Damage Flashes.
    /// To avoid version-specific namespace issues with XRI, we use a simple active/inactive approach
    /// or look for the component by name.
    /// </summary>
    public class VignetteController : MonoBehaviour
    {
        [SerializeField] private GameObject vignetteObject;
        [SerializeField] private float fadeDuration = 0.5f;
        
        [Header("Damage Flash Settings")]
        [SerializeField] private Color damageColor = new Color(1f, 0f, 0f, 0.6f);
        [SerializeField] private float damageFlashDuration = 0.5f;
        
        private UnityEngine.UI.Image vignetteImage;
        private Color originalColor = Color.black;

        private CanvasGroup canvasGroup;
        private bool isFading = false;
        private float timer = 0f;

        private Coroutine damageFlashCoroutine;

        private void Start()
        {
            if (vignetteObject != null)
            {
                canvasGroup = vignetteObject.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = vignetteObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f;
                
                vignetteImage = vignetteObject.GetComponent<UnityEngine.UI.Image>();
                if (vignetteImage != null)
                {
                    originalColor = vignetteImage.color;
                }

                vignetteObject.SetActive(false);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerHit.AddListener(TriggerDamageFlash);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerHit.RemoveListener(TriggerDamageFlash);
            }
        }

        public void TriggerDamageFlash()
        {
            if (vignetteObject == null) return;
            
            if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = StartCoroutine(DamageFlashRoutine());
        }

        private IEnumerator DamageFlashRoutine()
        {
            isFading = false; // Override fall-out fade
            vignetteObject.SetActive(true);
            
            if (vignetteImage != null) vignetteImage.color = damageColor;
            
            // Quick fade in
            float halfDur = damageFlashDuration / 2f;
            for (float t = 0; t < halfDur; t += Time.deltaTime)
            {
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / halfDur);
                yield return null;
            }
            
            // Quick fade out
            for (float t = 0; t < halfDur; t += Time.deltaTime)
            {
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / halfDur);
                yield return null;
            }
            
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (vignetteImage != null) vignetteImage.color = originalColor;
            vignetteObject.SetActive(false);
            
            damageFlashCoroutine = null;
        }

        private void Update()
        {
            if (isFading && canvasGroup != null)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            }
        }

        public void StartFadeOut()
        {
            if (vignetteObject != null)
            {
                if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
                if (vignetteImage != null) vignetteImage.color = originalColor;

                vignetteObject.SetActive(true);
                isFading = true;
                timer = 0f;
            }
        }

        public void ResetVignette()
        {
            if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
            
            isFading = false;
            timer = 0f;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (vignetteImage != null) vignetteImage.color = originalColor;
            if (vignetteObject != null) vignetteObject.SetActive(false);
        }
    }
}
