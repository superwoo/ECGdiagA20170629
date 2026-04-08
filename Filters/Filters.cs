namespace ECGDiag.Filters;

/// <summary>
/// Lowpass filter implementation
/// </summary>
public class LowpassFilter
{
    private readonly double[] _a0 = new double[2];
    private readonly double[] _a1 = new double[2];
    private readonly double[] _a2 = new double[2];
    private readonly double[] _b1 = new double[2];
    private readonly double[] _b2 = new double[2];
    private double _x1, _x2, _y1, _y2, _z1, _z2;

    public LowpassFilter(double freq, int sampleRate)
    {
        Init(freq, sampleRate);
    }

    public void Init(double freq, int sampleRate)
    {
        double fts = sampleRate / freq;
        double[] c1 = { 0.5883, 0.243507 };
        double[] c2 = { 0.101309, 0.101268 };

        for (int i = 0; i < 2; i++)
        {
            double c = 1.0 + c1[i] * fts + c2[i] * fts * fts;
            _a0[i] = 1.0 / c;
            _a1[i] = 2.0 / c;
            _a2[i] = _a0[i];
            _b1[i] = 2.0 * (1.0 - c2[i] * fts * fts) / c;
            _b2[i] = (1.0 - c1[i] * fts + c2[i] * fts * fts) / c;
        }

        _x1 = _x2 = 0.0;
        _y1 = _y2 = _z1 = _z2 = 0.0;
    }

    public int Filter(int xt)
    {
        double yt = _a0[0] * xt + _a1[0] * _x1 + _a2[0] * _x2 - _b1[0] * _y1 - _b2[0] * _y2;
        double zt = _a0[1] * yt + _a1[1] * _y1 + _a2[1] * _y2 - _b1[1] * _z1 - _b2[1] * _z2;

        if (Math.Abs(yt) < 1.0e-100) yt = 0.0;
        if (Math.Abs(zt) < 1.0e-100) zt = 0.0;

        _x2 = _x1;
        _x1 = xt;
        _y2 = _y1;
        _y1 = yt;
        _z2 = _z1;
        _z1 = zt;

        return (int)zt;
    }
}

/// <summary>
/// Highpass filter implementation
/// </summary>
public class HighpassFilter
{
    private int _fs;
    private double _fc;
    private double _a0, _a1, _a2, _b1, _b2;
    private double _x1, _x2, _y1, _y2;

    public HighpassFilter(double freq, int sampleRate)
    {
        if (freq <= 0.01) freq = 0.01;
        Init(freq, sampleRate);
    }

    public void Init(double freq, int sampleRate)
    {
        _fs = sampleRate;
        _fc = freq;

        double fsfc = sampleRate / freq;
        double c = 0.5883003;
        double tscp = fsfc / Math.PI;
        double a = 1.0 + c * fsfc + tscp * tscp;

        _a0 = tscp * tscp / a;
        _a1 = -2.0 * _a0;
        _a2 = _a0;
        _b1 = (2.0 - 2.0 * tscp * tscp) / a;
        _b2 = (1.0 - c * fsfc + tscp * tscp) / a;

        _x1 = _x2 = _y1 = _y2 = 0;
    }

    public void Init(double freq)
    {
        Init(freq, _fs);
    }

    public void Init()
    {
        Init(_fc, _fs);
    }

    public void InitialValue(int xt)
    {
        _x1 = _x2 = _y1 = _y2 = xt;
    }

    public void InitialValue(double xt)
    {
        _x1 = _x2 = _y1 = _y2 = xt;
    }

    public int Filter(int xt)
    {
        double yt = _a0 * xt + _a1 * _x1 + _a2 * _x2 - _b1 * _y1 - _b2 * _y2;
        if (Math.Abs(yt) < 1.0e-100) yt = 0.0;

        _x2 = _x1;
        _x1 = xt;
        _y2 = _y1;
        _y1 = yt;

        return (int)yt;
    }

    public double Filter(double xt)
    {
        double yt = _a0 * xt + _a1 * _x1 + _a2 * _x2 - _b1 * _y1 - _b2 * _y2;
        if (Math.Abs(yt) < 1.0e-100) yt = 0.0;

        _x2 = _x1;
        _x1 = xt;
        _y2 = _y1;
        _y1 = yt;

        return yt;
    }
}

/// <summary>
/// Sorting utilities
/// </summary>
public static class FilterUtilities
{
    /// <summary>
    /// Bubble sort for integer arrays (ascending order)
    /// 冒泡排序（整数）从小到大
    /// </summary>
    public static void BubbleSort(int[] data, int n)
    {
        bool bubble;
        do
        {
            bubble = false;
            for (int i = 0; i < n - 1; i++)
            {
                if (data[i] > data[i + 1])
                {
                    (data[i], data[i + 1]) = (data[i + 1], data[i]);
                    bubble = true;
                }
            }
        } while (bubble);
    }

    /// <summary>
    /// Bubble sort for short arrays with corresponding array sorting (ascending order)
    /// 冒泡排序（整数）从小到大 Array排序
    /// </summary>
    public static void BubbleSort(short[] data, short[] sortArray, int n)
    {
        bool bubble;
        do
        {
            bubble = false;
            for (int i = 0; i < n - 1; i++)
            {
                if (data[i] > data[i + 1])
                {
                    (data[i], data[i + 1]) = (data[i + 1], data[i]);
                    (sortArray[i], sortArray[i + 1]) = (sortArray[i + 1], sortArray[i]);
                    bubble = true;
                }
            }
        } while (bubble);
    }
}
