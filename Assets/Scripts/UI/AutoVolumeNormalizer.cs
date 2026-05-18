using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AutoVolumeNormalizer : MonoBehaviour
{
    [Tooltip("Turn this up to forcefully boost the volume! (1 = Normal, 10 = 10x Louder)")]
    [Range(1f, 50f)]
    public float manualBoostOverride = 1f;

    [Header("Auto Normalizer Settings")]
    public bool useAutoNormalizer = true;
    [Range(0.01f, 1.0f)]
    public float targetVolume = 0.2f; 
    public float maxBoost = 15f;
    public float noiseGate = 0.0001f;

    [SerializeField] private float currentGain = 1f; // Visible for debugging

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (manualBoostOverride > 1.01f)
        {
            // HARD OVERRIDE: Just multiply the wave!
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Mathf.Clamp(data[i] * manualBoostOverride, -1f, 1f);
            }
            return;
        }

        if (!useAutoNormalizer) return;

        float sum = 0f;
        for (int i = 0; i < data.Length; i++) sum += data[i] * data[i];
        float rms = Mathf.Sqrt(sum / data.Length);
        
        if (rms > noiseGate) 
        {
            float targetGain = targetVolume / rms;
            currentGain = Mathf.Clamp(targetGain, 1f, maxBoost); // Instantly snap, only boost
        }
        else
        {
            currentGain = 1f;
        }

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = Mathf.Clamp(data[i] * currentGain, -1f, 1f);
        }
    }
}
