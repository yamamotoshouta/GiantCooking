using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AntiGravity.UI
{
    public class MenuUIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton; // From Victory
        [SerializeField] private Button tryAgainButton; // From Defeat

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStarted.AddListener(HideAll);
                GameManager.Instance.OnVictory.AddListener(ShowVictory);
                GameManager.Instance.OnDefeat.AddListener(ShowDefeat);
            }

            if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (tryAgainButton != null) tryAgainButton.onClick.AddListener(OnRestartClicked);

            ShowStart();
        }

        public void ShowStart()
        {
            if (startPanel != null) startPanel.SetActive(true);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
        }

        public void ShowVictory()
        {
            if (startPanel != null) startPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(true);
            if (defeatPanel != null) defeatPanel.SetActive(false);
        }

        public void ShowDefeat()
        {
            if (startPanel != null) startPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(true);
        }

        public void HideAll()
        {
            if (startPanel != null) startPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
        }

        private void OnStartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }

        private void OnRestartClicked()
        {
            // Just restart the game state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }
    }
}
