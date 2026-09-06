using System;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public static class EzRandom
{
    private static Random random;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void ReSeed() => random = new Random((uint)DateTime.UtcNow.Ticks);

    public static void ReSeed(uint seed) => random = new Random(seed == 0 ? 1u : seed);

    public static bool CoinFlip() => random.NextBool();

    public static bool Chance(float chance) => random.NextFloat(0f, 100f) < chance;

    public static uint Seed() => random.NextUInt();

    public static float Range01() => random.NextFloat();

    public static int Range(int a, int b) => random.NextInt(a, b);
    public static int Range(IntRange range) => Range(range.Min, range.Max);

    public static uint Range(uint a, uint b) => random.NextUInt(a, b);
    public static uint Range(UIntRange range) => Range(range.Min, range.Max);

    public static short Range(short a, short b) => (short)random.NextInt(a, b);
    public static short Range(ShortRange range) => Range(range.Min, range.Max);

    public static ushort Range(ushort a, ushort b) => (ushort)random.NextUInt(a, b);
    public static ushort Range(UShortRange range) => Range(range.Min, range.Max);

    public static byte Range(byte a, byte b) => (byte)random.NextUInt(a, b);
    public static byte Range(ByteRange range) => Range(range.Min, range.Max);

    public static sbyte Range(sbyte a, sbyte b) => (sbyte)random.NextInt(a, b);
    public static sbyte Range(SByteRange range) => Range(range.Min, range.Max);

    public static long Range(long a, long b)
    {
        ulong value = ((ulong)random.NextUInt() << 32) | random.NextUInt();
        ulong range = (ulong)(b - a);
        return a + (long)(value % range);
    }

    public static long Range(LongRange range) => Range(range.Min, range.Max);

    public static ulong Range(ulong a, ulong b)
    {
        ulong value = ((ulong)random.NextUInt() << 32) | random.NextUInt();
        return a + value % (b - a);
    }

    public static ulong Range(ULongRange range) => Range(range.Min, range.Max);

    public static float Range(float a, float b) => random.NextFloat(a, b);
    public static float Range(FloatRange range) => Range(range.Min, range.Max);

    public static double Range(double a, double b)
    {
        ulong value = ((ulong)random.NextUInt() << 32) | random.NextUInt();
        double normalized = value / (double)ulong.MaxValue;
        return a + (b - a) * normalized;
    }

    public static double Range(DoubleRange range) => Range(range.Min, range.Max);

    public static Vector2 Range(Vector2 a, Vector2 b) => new(
        Range(a.x, b.x),
        Range(a.y, b.y)
    );

    public static Vector2 Range(Vector2Range range) => Range(range.Min, range.Max);

    public static Vector3 Range(Vector3 a, Vector3 b) => new(
        Range(a.x, b.x),
        Range(a.y, b.y),
        Range(a.z, b.z)
    );

    public static Vector3 Range(Vector3Range range) => Range(range.Min, range.Max);

    public static Vector4 Range(Vector4 a, Vector4 b) => new(
        Range(a.x, b.x),
        Range(a.y, b.y),
        Range(a.z, b.z),
        Range(a.w, b.w)
    );

    public static Vector4 Range(Vector4Range range) => Range(range.Min, range.Max);

    public static float2 Range(float2 a, float2 b) => new(
        Range(a.x, b.x),
        Range(a.y, b.y)
    );

    public static float3 Range(float3 a, float3 b) => new(
        Range(a.x, b.x),
        Range(a.y, b.y),
        Range(a.z, b.z)
    );

    public static float4 Range(float4 a, float4 b) => new(
        Range(a.x, b.x),
        Range(a.y, b.y),
        Range(a.z, b.z),
        Range(a.w, b.w)
    );

    public static int2 Range(int2 a, int2 b) => new(
        Range(a.x, b.x),
        Range(a.y, b.y)
    );

    public static int3 Range(int3 a, int3 b) => new(
        Range(a.x, b.x),
        Range(a.y, b.y),
        Range(a.z, b.z)
    );

    public static int4 Range(int4 a, int4 b) => new(
        Range(a.x, b.x),
        Range(a.y, b.y),
        Range(a.z, b.z),
        Range(a.w, b.w)
    );

    public static Color Range(Color a, Color b) => new(
        Range(a.r, b.r),
        Range(a.g, b.g),
        Range(a.b, b.b),
        Range(a.a, b.a)
    );

    public static Color RandomColor(bool randomizeAlpha = false) => new(
        Range01(),
        Range01(),
        Range01(),
        randomizeAlpha ? Range01() : 1f
    );

    public static float RandomRotationAxis() => Range(-90f, 270f);
}