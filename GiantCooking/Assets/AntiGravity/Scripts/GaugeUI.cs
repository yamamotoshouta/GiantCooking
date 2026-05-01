using UnityEngine;
using UnityEngine.UI;

namespace AntiGravity
{
    public class GaugeUI : MonoBehaviour
    {
        [SerializeField] private Image gaugeFill;
        [SerializeField] private Color normalColor = Color.cyan;
        [SerializeField] private Color maxColor = Color.red;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGaugeChanged.AddListener(UpdateGauge);
                GameManager.Instance.OnGaugeMaxed.AddListener(HandleMaxGauge);
            }
            
            if (gaugeFill != null)
            {
                gaugeFill.color = normalColor;
                gaugeFill.fillAmount = 0;
            }
        }

        private void UpdateGauge(float percent)
        {
            if (gaugeFill != null)
            {
                gaugeFill.fillAmount = percent;
                if (percent < 1.0f)
                {
                    gaugeFill.color = normalColor;
                }
            }
        }

        private void HandleMaxGauge()
        {
            if (gaugeFill != null)
            {
                gaugeFill.color = maxColor;
                // Add some juice like pulsing here
            }
        }
    }
}
