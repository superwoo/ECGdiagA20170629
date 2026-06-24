namespace EcgDiag;

/// <summary>
/// Lowpass filter (4th order Butterworth)
/// </summary>
public class LowpassFilter
{
    private double[] a0 = new double[2], a1 = new double[2], a2 = new double[2];
    private double[] b1 = new double[2], b2 = new double[2];
    private double x1, x2, y1, y2, z1, z2;

    public LowpassFilter(double Freq, int SampleRate) { Init(Freq, SampleRate); }

    public void Init(double Freq, int SampleRate)
    {
        double[] c1 = { 0.5883, 0.243507 };
        double[] c2 = { 0.101309, 0.101268 };
        double fts = (double)SampleRate / Freq;
        for (int i = 0; i < 2; i++)
        {
            double c = 1.0 + c1[i] * fts + c2[i] * fts * fts;
            a0[i] = 1.0 / c; a1[i] = 2.0 / c; a2[i] = a0[i];
            b1[i] = 2.0 * (1.0 - c2[i] * fts * fts) / c;
            b2[i] = (1.0 - c1[i] * fts + c2[i] * fts * fts) / c;
        }
        x1 = x2 = 0; y1 = y2 = z1 = z2 = 0;
    }

    public int Filter(int xt)
    {
        double yt = a0[0] * xt + a1[0] * x1 + a2[0] * x2 - b1[0] * y1 - b2[0] * y2;
        double zt = a0[1] * yt + a1[1] * y1 + a2[1] * y2 - b1[1] * z1 - b2[1] * z2;
        if (Math.Abs(yt) < 1.0e-100) yt = 0.0;
        if (Math.Abs(zt) < 1.0e-100) zt = 0.0;
        x2 = x1; x1 = xt; y2 = y1; y1 = yt; z2 = z1; z1 = zt;
        return (int)zt;
    }
}

/// <summary>
/// Highpass filter
/// </summary>
public class HighpassFilter
{
    private int fs;
    private double fc;
    private double a0, a1, a2, b1, b2;
    private double x1, x2, y1, y2;

    public HighpassFilter(double Freq, int SampleRate)
    {
        if (Freq <= 0.01) Freq = 0.01;
        Init(Freq, SampleRate);
    }

    public void Init(double Freq, int SampleRate)
    {
        fs = SampleRate; fc = Freq;
        double fsfc = SampleRate / Freq;
        double c = 0.5883003;
        double tscp = fsfc / Math.PI;
        double a = 1.0 + c * fsfc + tscp * tscp;
        a0 = tscp * tscp / a;
        a1 = -2.0 * a0;
        a2 = a0;
        b1 = (2.0 - 2.0 * tscp * tscp) / a;
        b2 = (1.0 - c * fsfc + tscp * tscp) / a;
        x1 = x2 = y1 = y2 = 0;
    }

    public void Init(double Freq) { Init(Freq, fs); }
    public void Init() { Init(fc, fs); }

    public void InitialValue(int xt) { x1 = x2 = y1 = y2 = xt; }
    public void InitialValue(double xt) { x1 = x2 = y1 = y2 = xt; }

    public int Filter(int xt)
    {
        double yt = a0 * xt + a1 * x1 + a2 * x2 - b1 * y1 - b2 * y2;
        if (Math.Abs(yt) < 1.0e-100) yt = 0;
        x2 = x1; x1 = xt; y2 = y1; y1 = yt;
        return (int)yt;
    }

    public double FilterDouble(double xt)
    {
        double yt = a0 * xt + a1 * x1 + a2 * x2 - b1 * y1 - b2 * y2;
        if (Math.Abs(yt) < 1.0e-100) yt = 0;
        x2 = x1; x1 = xt; y2 = y1; y1 = yt;
        return yt;
    }
}

/// <summary>
/// 冒泡排序
/// </summary>
public static class BubbleSortHelper
{
    public static void BubbleSort(int[] data, int n)
    {
        bool bubble;
        for (; ; )
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
            if (!bubble) break;
        }
    }

    public static void BubbleSort(short[] data, short[] SortArray, int n)
    {
        bool bubble;
        for (; ; )
        {
            bubble = false;
            for (int i = 0; i < n - 1; i++)
            {
                if (data[i] > data[i + 1])
                {
                    (data[i], data[i + 1]) = (data[i + 1], data[i]);
                    (SortArray[i], SortArray[i + 1]) = (SortArray[i + 1], SortArray[i]);
                    bubble = true;
                }
            }
            if (!bubble) break;
        }
    }
}
