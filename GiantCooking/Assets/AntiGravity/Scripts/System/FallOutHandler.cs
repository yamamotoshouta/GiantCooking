using UnityEngine;
using System.Collections;
using AntiGravity.System;

namespace AntiGravity
{
    public class FallOutHandler : MonoBehaviour
    {
        [SerializeField] private float fallThreshold = -10f;
        [SerializeField] private string targetTag = "Player"; // Or Enemy

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
        }

        private bool isFading = false;

        private void Update()
        {
            if (!isFading && transform.position.y < fallThreshold)
            {
                StartCoroutine(FallOutRoutine());
            }
        }

        private IEnumerator FallOutRoutine()
        {
            isFading = true;
            Debug.Log(gameObject.name + " fell out!");

            if (gameObject.CompareTag("Player") && ScreenFader.Instance != null)
            {
                if (GameManager.Instance != null) GameManager.Instance.TriggerDefeat();
                yield return ScreenFader.Instance.FadeOut(0.5f);
            }
            else if (gameObject.CompareTag("Enemy") && GameManager.Instance != null)
            {
                GameManager.Instance.TriggerVictory();
            }

            // Reset position
            transform.position = startPosition;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (gameObject.CompareTag("Player") && ScreenFader.Instance != null)
            {
                yield return ScreenFader.Instance.FadeIn(0.5f);
            }

            isFading = false;
        }
    }
}
