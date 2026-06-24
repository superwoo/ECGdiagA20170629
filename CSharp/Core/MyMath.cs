namespace EcgDiag;

public class CMax
{
    private int index;

    public CMax() { index = 0; }

    public int GetIndex() { return index; }

    public short Maximum(short[] x, int length)
    {
        index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] > x[index]) index = i;
        }
        return x[index];
    }

    public int Maximum(int[] x, int length)
    {
        index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] > x[index]) index = i;
        }
        return x[index];
    }

    public float Maximum(float[] x, int length)
    {
        index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] > x[index]) index = i;
        }
        return x[index];
    }

    public double Maximum(double[] x, int length)
    {
        index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] > x[index]) index = i;
        }
        return x[index];
    }
}

public class CMin
{
    private int index;

    public CMin() { index = 0; }

    public int GetIndex() { return index; }

    public short Minimum(short[] x, int length)
    {
        index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] < x[index]) index = i;
        }
        return x[index];
    }

    public int Minimum(int[] x, int length)
    {
        index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] < x[index]) index = i;
        }
        return x[index];
    }

    public float Minimum(float[] x, int length)
    {
        index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] < x[index]) index = i;
        }
        return x[index];
    }

    public double Minimum(double[] x, int length)
    {
        index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] < x[index]) index = i;
        }
        return x[index];
    }
}

public class CAverage
{
    public short Average(short[] x, int length)
    {
        long ave = 0;
        for (int i = 0; i < length; i++) ave += x[i];
        return (short)(ave / length);
    }

    public int Average(int[] x, int length)
    {
        long ave = 0;
        for (int i = 0; i < length; i++) ave += x[i];
        return (int)(ave / length);
    }

    public float Average(float[] x, int length)
    {
        double ave = 0;
        for (int i = 0; i < length; i++) ave += x[i];
        return (float)(ave / length);
    }

    public double Average(double[] x, int length)
    {
        double ave = 0;
        for (int i = 0; i < length; i++) ave += x[i];
        return ave / length;
    }
}

public class CRootMeanSquare : CAverage
{
    public double MeanSquare(short[] x, int length)
    {
        double s = 0, z = Average(x, length);
        for (int i = 0; i < length; i++) s += (x[i] - z) * (x[i] - z);
        return s / length;
    }

    public double MeanSquare(int[] x, int length)
    {
        double s = 0, z = Average(x, length);
        for (int i = 0; i < length; i++) s += (x[i] - z) * (x[i] - z);
        return s / length;
    }

    public double MeanSquare(float[] x, int length)
    {
        double s = 0, z = Average(x, length);
        for (int i = 0; i < length; i++) s += (x[i] - z) * (x[i] - z);
        return s / length;
    }

    public double MeanSquare(double[] x, int length)
    {
        double s = 0, z = Average(x, length);
        for (int i = 0; i < length; i++) s += (x[i] - z) * (x[i] - z);
        return s / length;
    }

    public double RootMeanSquare(short[] x, int length) { return Math.Sqrt(MeanSquare(x, length)); }
    public double RootMeanSquare(int[] x, int length) { return Math.Sqrt(MeanSquare(x, length)); }
    public double RootMeanSquare(float[] x, int length) { return Math.Sqrt(MeanSquare(x, length)); }
    public double RootMeanSquare(double[] x, int length) { return Math.Sqrt(MeanSquare(x, length)); }
}

public class CLinearCorrelation : CRootMeanSquare
{
    public double LinearCorrelation(short[] x, short[] y, int length, int xOffset = 0, int yOffset = 0)
    {
        double zx = 0, zy = 0;
        for (int i = 0; i < length; i++) { zx += x[i + xOffset]; zy += y[i + yOffset]; }
        zx /= length; zy /= length;
        double sxx = 0, syy = 0, sxy = 0;
        for (int i = 0; i < length; i++)
        {
            double xt = x[i + xOffset] - zx;
            double yt = y[i + yOffset] - zy;
            sxx += xt * xt; syy += yt * yt; sxy += xt * yt;
        }
        if (sxx > 0 && syy > 0) return sxy * sxy / (sxx * syy);
        else return 0;
    }

    public double LinearCorrelation(int[] x, int[] y, int length, int xOffset = 0, int yOffset = 0)
    {
        double zx = 0, zy = 0;
        for (int i = 0; i < length; i++) { zx += x[i + xOffset]; zy += y[i + yOffset]; }
        zx /= length; zy /= length;
        double sxx = 0, syy = 0, sxy = 0;
        for (int i = 0; i < length; i++)
        {
            double xt = x[i + xOffset] - zx;
            double yt = y[i + yOffset] - zy;
            sxx += xt * xt; syy += yt * yt; sxy += xt * yt;
        }
        if (sxx > 0 && syy > 0) return sxy * sxy / (sxx * syy);
        else return 0;
    }

    public double LinearCorrelation(float[] x, float[] y, int length, int xOffset = 0, int yOffset = 0)
    {
        double zx = 0, zy = 0;
        for (int i = 0; i < length; i++) { zx += x[i + xOffset]; zy += y[i + yOffset]; }
        zx /= length; zy /= length;
        double sxx = 0, syy = 0, sxy = 0;
        for (int i = 0; i < length; i++)
        {
            double xt = x[i + xOffset] - zx;
            double yt = y[i + yOffset] - zy;
            sxx += xt * xt; syy += yt * yt; sxy += xt * yt;
        }
        if (sxx > 0 && syy > 0) return sxy * sxy / (sxx * syy);
        else return 0;
    }

    public double LinearCorrelation(double[] x, double[] y, int length, int xOffset = 0, int yOffset = 0)
    {
        double zx = 0, zy = 0;
        for (int i = 0; i < length; i++) { zx += x[i + xOffset]; zy += y[i + yOffset]; }
        zx /= length; zy /= length;
        double sxx = 0, syy = 0, sxy = 0;
        for (int i = 0; i < length; i++)
        {
            double xt = x[i + xOffset] - zx;
            double yt = y[i + yOffset] - zy;
            sxx += xt * xt; syy += yt * yt; sxy += xt * yt;
        }
        if (sxx > 0 && syy > 0) return sxy * sxy / (sxx * syy);
        else return 0;
    }
}

public class CMaxLinearCorrelation : CLinearCorrelation
{
    private int m_offset_range;
    private int m_offset;
    private CMax _max = new();
    private CMin _min = new();

    public CMaxLinearCorrelation(int offset_range = 0)
    {
        m_offset_range = offset_range;
        m_offset = 0;
    }

    public int GetOffset() { return m_offset; }
    public CMax MaxHelper => _max;
    public CMin MinHelper => _min;

    public double MaxLinearCorrelation(short[] x, short[] y, int length)
    {
        m_offset = 0;
        int range = m_offset_range;
        if (range <= 0) range = length * 2 / 5;
        int start = range;
        int len = length - range;
        int j = start / 2;
        double rmax = 0;
        for (int i = 0; i < start; i++)
        {
            double r = LinearCorrelation(x, y, len, j, i);
            if (rmax < r) { rmax = r; m_offset = i; }
        }
        m_offset -= j;
        return rmax;
    }

    public double MaxLinearCorrelation(int[] x, int[] y, int length)
    {
        m_offset = 0;
        int range = m_offset_range;
        if (range <= 0) range = length * 2 / 5;
        int start = range;
        int len = length - range;
        int j = start / 2;
        double rmax = 0;
        for (int i = 0; i < start; i++)
        {
            double r = LinearCorrelation(x, y, len, j, i);
            if (rmax < r) { rmax = r; m_offset = i; }
        }
        m_offset -= j;
        return rmax;
    }

    public double MaxLinearCorrelation(float[] x, float[] y, int length)
    {
        m_offset = 0;
        int range = m_offset_range;
        if (range <= 0) range = length * 2 / 5;
        int start = range;
        int len = length - range;
        int j = start / 2;
        double rmax = 0;
        for (int i = 0; i < start; i++)
        {
            double r = LinearCorrelation(x, y, len, j, i);
            if (rmax < r) { rmax = r; m_offset = i; }
        }
        m_offset -= j;
        return rmax;
    }

    public double MaxLinearCorrelation(double[] x, double[] y, int length)
    {
        m_offset = 0;
        int range = m_offset_range;
        if (range <= 0) range = length * 2 / 5;
        int start = range;
        int len = length - range;
        int j = start / 2;
        double rmax = 0;
        for (int i = 0; i < start; i++)
        {
            double r = LinearCorrelation(x, y, len, j, i);
            if (rmax < r) { rmax = r; m_offset = i; }
        }
        m_offset -= j;
        return rmax;
    }
}
