using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


/// <summary>
/// Wrapper for AnimationCurve that allows sampling at a fixed number of points, making Evaluate() much cheaper.
/// </summary>
[BurstCompile]
[System.Serializable]
public struct NativeSampledAnimationCurve
{
    [SerializeField] private AnimationCurve curve;

    [Tooltip("More samples = more accurate, but more memory usage")]
    [Range(2, 500)]
    [SerializeField] private int sampleCount;

    private NativeArray<float> bakedCurve;

    private float minTime;
    private float maxTime;


    public static NativeSampledAnimationCurve Default => new NativeSampledAnimationCurve()
    {
        curve = AnimationCurve.Linear(1, 1, 0, 0),
        sampleCount = 50,
    };
    
    /// <summary>
    /// Bake the AnimationCurve into the fixed number of samples.
    /// </summary>
    public void Bake()
    {
#if ENABLE_DEBUG_SYSTEMS
        if (_curve.keys.Length == 0)
        {
            DebugLogger.LogError("AnimationCurve is null!");
            return;
        }
#endif

        sampleCount = math.max(sampleCount, 2);

        bakedCurve.DisposeIfCreated();
        bakedCurve = new NativeArray<float>(sampleCount, Allocator.Persistent);

        minTime = curve.keys[0].time;
        maxTime = curve.keys[^1].time;

        float timeRange = maxTime - minTime;

        for (int i = 0; i < sampleCount; i++)
        {
            float percent = (float)i / (sampleCount - 1);
            float time = minTime + percent * timeRange;

            bakedCurve[i] = curve.Evaluate(time);
        }
    }
    public readonly void DisposeIfCreated()
    {
        bakedCurve.DisposeIfCreated();
    }

    /// <summary>
    /// Get value from the baked curve based on <paramref name="time"/>
    /// </summary>
    public float Evaluate(float time)
    {
#if UNITY_EDITOR
        if (bakedCurve == null || bakedCurve.Length != sampleCount)
        {
            DebugLogger.LogError("NativeSampledAnimationCurve: Baked curve is not initialized properly, call Bake before use. This will throw in builds.");
            Bake();
        }
#endif
        float percent01 = math.saturate((time - minTime) / (maxTime - minTime));
        return EvaluateWithBurst(bakedCurve, sampleCount, percent01);
    }
    /// <summary>
    /// Get value from the baked curve based on <paramref name="percent01"/> (Slightly faster than .Evaluate(time))
    /// </summary>
    public float Evaluate01(float percent01)
    {
#if UNITY_EDITOR
        if (bakedCurve == null || bakedCurve.Length != sampleCount)
        {
            DebugLogger.LogError("NativeSampledAnimationCurve: Baked curve is not initialized properly, call Bake before use. This will throw in builds.");
            Bake();
        }
#endif

        return EvaluateWithBurst(bakedCurve, sampleCount, percent01);
    }

    /// <summary>
    /// Get value based on percent (0-1) from the baked curve, compiled with burst for super fast evaluation.
    /// </summary>
    [BurstCompile]
    private static float EvaluateWithBurst(in NativeArray<float> bakedCurve, int sampleCount, float percent01)
    {
        float curvePosition = percent01 * (sampleCount - 1);

        int floorIndex = (int)math.floor(curvePosition);
        int ceilIndex = (int)curvePosition;

        return math.lerp(bakedCurve[floorIndex], bakedCurve[ceilIndex], curvePosition - floorIndex);
    }
}
