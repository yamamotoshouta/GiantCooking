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

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStarted.AddListener(HideAll);
                GameManager.Instance.OnVictory.AddListener(ShowVictory);
            }

            if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);

            ShowStart();
        }

        public void ShowStart()
        {
            if (startPanel != null) startPanel.SetActive(true);
            if (victoryPanel != null) victoryPanel.SetActive(false);
        }

        public void ShowVictory()
        {
            if (startPanel != null) startPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }

        public void HideAll()
        {
            if (startPanel != null) startPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
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
