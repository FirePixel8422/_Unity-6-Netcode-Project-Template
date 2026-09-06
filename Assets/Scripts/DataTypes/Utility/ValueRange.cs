using UnityEngine;


/// <summary>
/// A lightweight range that holds an int Min and int Max.
/// </summary>
[System.Serializable]
public struct IntRange
{
    public int Min;
    public int Max;

    public IntRange(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public readonly int Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a uint Min and uint Max.
/// </summary>
[System.Serializable]
public struct UIntRange
{
    public uint Min;
    public uint Max;

    public UIntRange(uint min, uint max)
    {
        Min = min;
        Max = max;
    }

    public readonly uint Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a short Min and short Max.
/// </summary>
[System.Serializable]
public struct ShortRange
{
    public short Min;
    public short Max;

    public ShortRange(short min, short max)
    {
        Min = min;
        Max = max;
    }

    public readonly short Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a ushort Min and ushort Max.
/// </summary>
[System.Serializable]
public struct UShortRange
{
    public ushort Min;
    public ushort Max;

    public UShortRange(ushort min, ushort max)
    {
        Min = min;
        Max = max;
    }

    public readonly ushort Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a byte Min and byte Max.
/// </summary>
[System.Serializable]
public struct ByteRange
{
    public byte Min;
    public byte Max;

    public ByteRange(byte min, byte max)
    {
        Min = min;
        Max = max;
    }

    public readonly byte Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds an sbyte Min and sbyte Max.
/// </summary>
[System.Serializable]
public struct SByteRange
{
    public sbyte Min;
    public sbyte Max;

    public SByteRange(sbyte min, sbyte max)
    {
        Min = min;
        Max = max;
    }

    public readonly sbyte Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a long Min and long Max.
/// </summary>
[System.Serializable]
public struct LongRange
{
    public long Min;
    public long Max;

    public LongRange(long min, long max)
    {
        Min = min;
        Max = max;
    }

    public readonly long Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a ulong Min and ulong Max.
/// </summary>
[System.Serializable]
public struct ULongRange
{
    public ulong Min;
    public ulong Max;

    public ULongRange(ulong min, ulong max)
    {
        Min = min;
        Max = max;
    }

    public readonly ulong Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a float Min and float Max.
/// </summary>
[System.Serializable]
public struct FloatRange
{
    public float Min;
    public float Max;

    public FloatRange(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public readonly float Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a double Min and double Max.
/// </summary>
[System.Serializable]
public struct DoubleRange
{
    public double Min;
    public double Max;

    public DoubleRange(double min, double max)
    {
        Min = min;
        Max = max;
    }

    public readonly double Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a Vector2 Min and Vector2 Max.
/// </summary>
[System.Serializable]
public struct Vector2Range
{
    public Vector2 Min;
    public Vector2 Max;

    public Vector2Range(Vector2 min, Vector2 max)
    {
        Min = min;
        Max = max;
    }

    public readonly Vector2 Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a Vector3 Min and Vector3 Max.
/// </summary>
[System.Serializable]
public struct Vector3Range
{
    public Vector3 Min;
    public Vector3 Max;

    public Vector3Range(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public readonly Vector3 Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a Vector4 Min and Vector4 Max.
/// </summary>
[System.Serializable]
public struct Vector4Range
{
    public Vector4 Min;
    public Vector4 Max;

    public Vector4Range(Vector4 min, Vector4 max)
    {
        Min = min;
        Max = max;
    }

    public readonly Vector4 Random() => EzRandom.Range(Min, Max);
}

/// <summary>
/// A lightweight range that holds a Color Min and Color Max.
/// </summary>
[System.Serializable]
public struct ColorRange
{
    public Color Min;
    public Color Max;

    public ColorRange(Color min, Color max)
    {
        Min = min;
        Max = max;
    }

    public readonly Color Random() => EzRandom.Range(Min, Max);
}