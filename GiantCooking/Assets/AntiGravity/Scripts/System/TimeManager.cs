using UnityEngine;
using System.Collections;

namespace AntiGravity.System
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        private float originalFixedDeltaTime;
        private Coroutine activeSlowMo;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            originalFixedDeltaTime = Time.fixedDeltaTime;
        }

        public void DoSlowMotion(float duration, float scale = 0.2f)
        {
            if (activeSlowMo != null) StopCoroutine(activeSlowMo);
            activeSlowMo = StartCoroutine(SlowMoRoutine(duration, scale));
        }

        private IEnumerator SlowMoRoutine(float duration, float scale)
        {
            Time.timeScale = scale;
            Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = originalFixedDeltaTime;
            activeSlowMo = null;
        }

        public void DoHitStop(float duration)
        {
            StartCoroutine(HitStopRoutine(duration));
        }

        private IEnumerator HitStopRoutine(float duration)
        {
            float prevScale = Time.timeScale;
            Time.timeScale = 0f;
            
            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = prevScale;
        }
    }
}
