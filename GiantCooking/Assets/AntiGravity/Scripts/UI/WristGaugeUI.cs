using UnityEngine;
using UnityEngine.UI;

namespace AntiGravity.UI
{
    public class WristGaugeUI : MonoBehaviour
    {
        [SerializeField] private Slider gaugeSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Color normalColor = Color.cyan;
        [SerializeField] private Color maxColor = Color.red;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGaugeChanged.AddListener(UpdateUI);
            }
            
            if (gaugeSlider != null) gaugeSlider.value = 0;
        }

        private void UpdateUI(float ratio)
        {
            if (gaugeSlider != null)
            {
                gaugeSlider.value = ratio;
            }

            if (fillImage != null)
            {
                fillImage.color = (ratio >= 1.0f) ? maxColor : normalColor;
            }
        }
        
        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGaugeChanged.RemoveListener(UpdateUI);
            }
        }
    }
}
