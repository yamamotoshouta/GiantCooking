using UnityEngine;

namespace AntiGravity
{
    public class Shockwave : MonoBehaviour
    {
        [SerializeField] private float speed = 5.0f;
        [SerializeField] private float lifetime = 2.0f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float scaleSpeed = 2.0f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            // 外側へ広がる
            transform.localScale += Vector3.one * scaleSpeed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // プレイヤーへのダメージ処理（必要に応じて実装）
                Debug.Log("Player hit by Shockwave!");
            }
        }
    }
}
