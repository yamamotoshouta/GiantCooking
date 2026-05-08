using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AntiGravity
{
    public class SwordGaugeUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI statusText;
        
        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.cyan;
        [SerializeField] private Color maxColor = Color.yellow;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGaugeChanged.AddListener(UpdateGauge);
                GameManager.Instance.OnGaugeMaxed.AddListener(HandleMaxGauge);
            }
            
            if (statusText != null) statusText.text = "";
            if (fillImage != null) fillImage.color = normalColor;
        }

        private void UpdateGauge(float progress)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = progress;
                if (progress < 1.0f)
                {
                    fillImage.color = normalColor;
                    if (statusText != null) statusText.text = "";
                }
            }
        }

        private void HandleMaxGauge()
        {
            if (fillImage != null)
            {
                fillImage.color = maxColor;
            }
            
            if (statusText != null)
            {
                statusText.text = "ISSEN READY!";
                statusText.color = maxColor;
            }
        }
    }
}
