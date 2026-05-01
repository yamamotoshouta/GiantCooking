using UnityEngine;

namespace AntiGravity
{
    /// <summary>
    /// A simple controller to handle the "Fall Out" vignette effect.
    /// To avoid version-specific namespace issues with XRI, we use a simple active/inactive approach
    /// or look for the component by name.
    /// </summary>
    public class VignetteController : MonoBehaviour
    {
        [SerializeField] private GameObject vignetteObject;
        [SerializeField] private float fadeDuration = 0.5f;
        
        private CanvasGroup canvasGroup;
        private bool isFading = false;
        private float timer = 0f;

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
                vignetteObject.SetActive(false);
            }
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
                vignetteObject.SetActive(true);
                isFading = true;
                timer = 0f;
            }
        }

        public void ResetVignette()
        {
            isFading = false;
            timer = 0f;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (vignetteObject != null) vignetteObject.SetActive(false);
        }
    }
}
