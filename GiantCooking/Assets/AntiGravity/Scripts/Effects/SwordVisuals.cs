using UnityEngine;

namespace AntiGravity
{
    public class SwordVisuals : MonoBehaviour
    {
        [SerializeField] private Renderer swordRenderer;
        [SerializeField] private Color normalEmission = Color.black;
        [SerializeField] private Color maxEmission = Color.cyan;
        [SerializeField] private float pulseSpeed = 5f;

        private Material mat;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void Start()
        {
            if (swordRenderer != null)
            {
                mat = swordRenderer.material;
                mat.EnableKeyword("_EMISSION");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGaugeMaxed.AddListener(OnMaxed);
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsIssenActive)
            {
                float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
                mat.SetColor(EmissionColor, Color.Lerp(normalEmission, maxEmission, pulse * 2f));
            }
        }

        private void OnMaxed()
        {
            Debug.Log("Sword Visuals: MAXED!");
        }

        public void ResetVisuals()
        {
            if (mat != null)
            {
                mat.SetColor(EmissionColor, normalEmission);
            }
        }
    }
}
