using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AntiGravity.System
{
    public class StageCrumbleManager : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Assign all the stage piece GameObjects here. The script will drop the ones furthest from this object's position first.")]
        [SerializeField] private List<GameObject> stageChunks = new List<GameObject>();
        [SerializeField] private float timeUntilFirstCrumble = 30f;
        [SerializeField] private float crumbleInterval = 5f;
        [SerializeField] private float warningDuration = 2f;
        
        [Header("Effects")]
        [SerializeField] private float vibrationIntensity = 0.05f;

        private Coroutine crumbleRoutine;
        private List<GameObject> remainingChunks = new List<GameObject>();
        private Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
        private Dictionary<GameObject, Quaternion> originalRotations = new Dictionary<GameObject, Quaternion>();
        private Dictionary<GameObject, bool> originalKinematicState = new Dictionary<GameObject, bool>();

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStarted.AddListener(ResetAndStartCrumbling);
            }
            
            // Store original states
            foreach (var chunk in stageChunks)
            {
                if (chunk == null) continue;
                originalPositions[chunk] = chunk.transform.position;
                originalRotations[chunk] = chunk.transform.rotation;
                
                Rigidbody rb = chunk.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    originalKinematicState[chunk] = rb.isKinematic;
                }
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStarted.RemoveListener(ResetAndStartCrumbling);
            }
        }

        public void ResetAndStartCrumbling()
        {
            if (crumbleRoutine != null)
            {
                StopCoroutine(crumbleRoutine);
            }

            // Reset chunks to original state
            remainingChunks.Clear();
            foreach (var chunk in stageChunks)
            {
                if (chunk == null) continue;
                
                chunk.transform.position = originalPositions[chunk];
                chunk.transform.rotation = originalRotations[chunk];
                chunk.SetActive(true);

                Rigidbody rb = chunk.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = originalKinematicState.ContainsKey(chunk) ? originalKinematicState[chunk] : true;
                    // Reset velocity via newer API property name or fallback
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                remainingChunks.Add(chunk);
            }

            crumbleRoutine = StartCoroutine(CrumbleSequence());
        }

        private IEnumerator CrumbleSequence()
        {
            yield return new WaitForSeconds(timeUntilFirstCrumble);

            while (remainingChunks.Count > 0)
            {
                if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                {
                    yield return null;
                    continue;
                }

                // Find the outermost chunk
                GameObject outermostChunk = null;
                float maxDistSq = -1f;

                for (int i = remainingChunks.Count - 1; i >= 0; i--)
                {
                    GameObject chunk = remainingChunks[i];
                    if (chunk == null)
                    {
                        remainingChunks.RemoveAt(i);
                        continue;
                    }

                    // Calculate distance squared from this manager's position
                    float distSq = (chunk.transform.position - transform.position).sqrMagnitude;
                    if (distSq > maxDistSq)
                    {
                        maxDistSq = distSq;
                        outermostChunk = chunk;
                    }
                }

                if (outermostChunk != null)
                {
                    remainingChunks.Remove(outermostChunk);
                    yield return StartCoroutine(WarningAndDropRoutine(outermostChunk));
                }

                yield return new WaitForSeconds(crumbleInterval);
            }
        }

        private IEnumerator WarningAndDropRoutine(GameObject chunk)
        {
            Vector3 startPos = chunk.transform.position;
            float elapsed = 0f;

            // Warning Vibration
            while (elapsed < warningDuration)
            {
                // Pause if game is not playing
                if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                {
                    chunk.transform.position = startPos;
                    yield return null;
                    continue;
                }

                float offsetX = Random.Range(-1f, 1f) * vibrationIntensity;
                float offsetZ = Random.Range(-1f, 1f) * vibrationIntensity;
                chunk.transform.position = startPos + new Vector3(offsetX, 0, offsetZ);
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            chunk.transform.position = startPos; // Reset position

            // Drop
            Rigidbody rb = chunk.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = chunk.AddComponent<Rigidbody>();
            }
            
            rb.isKinematic = false;
            rb.useGravity = true;
            
            // Add a slight downward and outward force
            Vector3 dropDir = (chunk.transform.position - transform.position).normalized;
            dropDir.y = -0.5f;
            rb.AddForce(dropDir * 2f, ForceMode.Impulse);
        }
    }
}
