using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Fire_Pixel.Utility
{
    /// <summary>
    /// Wrapper for AnimationCurve that allows sampling at a fixed number of points, making Evaluate() much cheaper.
    /// </summary>
    [InspectorButtonCompatibility]
    [System.Serializable, BurstCompile]
    public class SampledAnimationCurve
    {
#if UNITY_EDITOR
        [SerializeField] private AnimationCurve _curve = AnimationCurve.Linear(1, 1, 0, 0);
#endif

        [Tooltip("More samples = more accurate, but more memory usage")]
        [Range(2, 500)]
        [SerializeField] private int _sampleCount = 50;

        [SerializeField, InspectorReadOnly] private float[] _bakedCurve;

        private float _minTime;
        private float _maxTime;

        /// <summary>
        /// Bake the AnimationCurve into the fixed number of samples.
        /// </summary>
        [InspectorButton("Bake Curve", true)]
        public void Bake()
        {
#if ENABLE_DEBUG_SYSTEMS
            if (_curve.keys.Length == 0)
            {
                DebugLogger.LogError("AnimationCurve is null!");
                return;
            }
#endif

            _sampleCount = math.max(_sampleCount, 2);
            _bakedCurve = new float[_sampleCount];

            _minTime = _curve.keys[0].time;
            _maxTime = _curve.keys[^1].time;

            float timeRange = _maxTime - _minTime;

            for (int i = 0; i < _sampleCount; i++)
            {
                float percent = (float)i / (_sampleCount - 1);
                float time = _minTime + percent * timeRange;

                _bakedCurve[i] = _curve.Evaluate(time);
            }
        }

        /// <summary>
        /// Get value from the baked curve based on <paramref name="time"/>
        /// </summary>
        public float Evaluate(float time)
        {
            VerifyCurveEditor();

            float percent01 = math.saturate((time - _minTime) / (_maxTime - _minTime));
            return EvaluateInternal(_bakedCurve, _sampleCount, percent01);
        }
        /// <summary>
        /// Get value from the baked curve based on <paramref name="percent01"/> (Slightly faster than .Evaluate(time))
        /// </summary>
        public float Evaluate01(float percent01)
        {
            VerifyCurveEditor();

            return EvaluateInternal(_bakedCurve, _sampleCount, percent01);
        }

        /// <summary>
        /// Get value based on percent (0-1) from the baked curve.
        /// </summary>
        private static float EvaluateInternal(in float[] bakedCurve, int sampleCount, float percent01)
        {
            float curvePosition = percent01 * (sampleCount - 1);

            int floorIndex = (int)curvePosition;
            int ceilIndex = math.min(floorIndex + 1, sampleCount - 1);

            float t = curvePosition - floorIndex;

            return math.lerp(bakedCurve[floorIndex], bakedCurve[ceilIndex], t);
        }

        [System.Diagnostics.Conditional(GlobalGameData.DEBUG_ONLY)]
        private void VerifyCurveEditor()
        {
            if (_bakedCurve == null || _bakedCurve.Length != _sampleCount)
            {
                DebugLogger.LogError($"{this}: not baked in inspector in editor, call Bake before using this curve in playmode. This will throw in non-debugbuilds.");
                Bake();
            }
        }
    }
}