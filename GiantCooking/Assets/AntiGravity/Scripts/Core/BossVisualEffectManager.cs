using UnityEngine;
using System.Collections;

namespace AntiGravity
{
    public class BossVisualEffectManager : MonoBehaviour
    {
        public static BossVisualEffectManager Instance { get; private set; }

        [Header("Phase 2: Distortion")]
        [SerializeField] private Material skyboxPhase2;
        [SerializeField] private GameObject noiseParticles;
        [SerializeField] private float timeScaleVariation = 0.9f;

        [Header("Phase 3: ZERO")]
        [SerializeField] private Material skyboxPhase3;
        [SerializeField] private GameObject darknessOverlay;
        [SerializeField] private AudioSource bgmSource;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        public void ApplyPhase2Effects()
        {
            if (skyboxPhase2 != null) RenderSettings.skybox = skyboxPhase2;
            if (noiseParticles != null) noiseParticles.SetActive(true);
            
            // TimeScaleを少し変えて違和感を出す
            Time.timeScale = timeScaleVariation;
        }

        public void ApplyPhase3Effects()
        {
            if (skyboxPhase3 != null) RenderSettings.skybox = skyboxPhase3;
            if (darknessOverlay != null) darknessOverlay.SetActive(true);
            
            // BGMを停止して静寂を演出
            if (bgmSource != null) bgmSource.Stop();
            
            Time.timeScale = 1.0f; // Phase 3はガチバトルなので等速に戻す
        }
    }
}
