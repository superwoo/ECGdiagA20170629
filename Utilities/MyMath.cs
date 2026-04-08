namespace ECGDiag.Utilities;

/// <summary>
/// Utility class for finding maximum value in arrays
/// </summary>
public class CMax
{
    private int _index;

    public CMax()
    {
        _index = 0;
    }

    public short Maximum(short[] x, int length)
    {
        _index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] > x[_index]) _index = i;
        }
        return x[_index];
    }

    public int Maximum(int[] x, int length)
    {
        _index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] > x[_index]) _index = i;
        }
        return x[_index];
    }

    public float Maximum(float[] x, int length)
    {
        _index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] > x[_index]) _index = i;
        }
        return x[_index];
    }

    public double Maximum(double[] x, int length)
    {
        _index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] > x[_index]) _index = i;
        }
        return x[_index];
    }

    public int GetIndex() => _index;
}

/// <summary>
/// Utility class for finding minimum value in arrays
/// </summary>
public class CMin
{
    private int _index;

    public CMin()
    {
        _index = 0;
    }

    public short Minimum(short[] x, int length)
    {
        _index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] < x[_index]) _index = i;
        }
        return x[_index];
    }

    public int Minimum(int[] x, int length)
    {
        _index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] < x[_index]) _index = i;
        }
        return x[_index];
    }

    public float Minimum(float[] x, int length)
    {
        _index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] < x[_index]) _index = i;
        }
        return x[_index];
    }

    public double Minimum(double[] x, int length)
    {
        _index = 0;
        for (int i = 0; i < length; i++)
        {
            if (x[i] < x[_index]) _index = i;
        }
        return x[_index];
    }

    public int GetIndex() => _index;
}

/// <summary>
/// Utility class for calculating average of arrays
/// </summary>
public class CAverage
{
    public CAverage()
    {
    }

    public short Average(short[] x, int length)
    {
        if (length == 0) return 0;
        int sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += x[i];
        }
        return (short)(sum / length);
    }

    public int Average(int[] x, int length)
    {
        if (length == 0) return 0;
        long sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += x[i];
        }
        return (int)(sum / length);
    }

    public float Average(float[] x, int length)
    {
        if (length == 0) return 0;
        double sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += x[i];
        }
        return (float)(sum / length);
    }

    public double Average(double[] x, int length)
    {
        if (length == 0) return 0;
        double sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += x[i];
        }
        return sum / length;
    }
}

/// <summary>
/// Utility class for calculating root mean square
/// </summary>
public class CRootMeanSquare : CAverage
{
    public CRootMeanSquare()
    {
    }

    public double MeanSquare(short[] x, int length)
    {
        if (length == 0) return 0;
        double sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += (double)x[i] * x[i];
        }
        return sum / length;
    }

    public double MeanSquare(int[] x, int length)
    {
        if (length == 0) return 0;
        double sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += (double)x[i] * x[i];
        }
        return sum / length;
    }

    public double MeanSquare(float[] x, int length)
    {
        if (length == 0) return 0;
        double sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += (double)x[i] * x[i];
        }
        return sum / length;
    }

    public double MeanSquare(double[] x, int length)
    {
        if (length == 0) return 0;
        double sum = 0;
        for (int i = 0; i < length; i++)
        {
            sum += x[i] * x[i];
        }
        return sum / length;
    }

    public double RootMeanSquare(short[] x, int length) => Math.Sqrt(MeanSquare(x, length));
    public double RootMeanSquare(int[] x, int length) => Math.Sqrt(MeanSquare(x, length));
    public double RootMeanSquare(float[] x, int length) => Math.Sqrt(MeanSquare(x, length));
    public double RootMeanSquare(double[] x, int length) => Math.Sqrt(MeanSquare(x, length));
}

/// <summary>
/// Utility class for calculating linear correlation
/// </summary>
public class CLinearCorrelation : CRootMeanSquare
{
    public CLinearCorrelation()
    {
    }

    public double LinearCorrelation(short[] x, short[] y, int length)
    {
        if (length == 0) return 0;

        double avgX = Average(x, length);
        double avgY = Average(y, length);

        double sumXY = 0;
        double sumXX = 0;
        double sumYY = 0;

        for (int i = 0; i < length; i++)
        {
            double dx = x[i] - avgX;
            double dy = y[i] - avgY;
            sumXY += dx * dy;
            sumXX += dx * dx;
            sumYY += dy * dy;
        }

        double denominator = Math.Sqrt(sumXX * sumYY);
        return denominator != 0 ? sumXY / denominator : 0;
    }

    public double LinearCorrelation(int[] x, int[] y, int length)
    {
        if (length == 0) return 0;

        double avgX = Average(x, length);
        double avgY = Average(y, length);

        double sumXY = 0;
        double sumXX = 0;
        double sumYY = 0;

        for (int i = 0; i < length; i++)
        {
            double dx = x[i] - avgX;
            double dy = y[i] - avgY;
            sumXY += dx * dy;
            sumXX += dx * dx;
            sumYY += dy * dy;
        }

        double denominator = Math.Sqrt(sumXX * sumYY);
        return denominator != 0 ? sumXY / denominator : 0;
    }

    public double LinearCorrelation(float[] x, float[] y, int length)
    {
        if (length == 0) return 0;

        double avgX = Average(x, length);
        double avgY = Average(y, length);

        double sumXY = 0;
        double sumXX = 0;
        double sumYY = 0;

        for (int i = 0; i < length; i++)
        {
            double dx = x[i] - avgX;
            double dy = y[i] - avgY;
            sumXY += dx * dy;
            sumXX += dx * dx;
            sumYY += dy * dy;
        }

        double denominator = Math.Sqrt(sumXX * sumYY);
        return denominator != 0 ? sumXY / denominator : 0;
    }

    public double LinearCorrelation(double[] x, double[] y, int length)
    {
        if (length == 0) return 0;

        double avgX = Average(x, length);
        double avgY = Average(y, length);

        double sumXY = 0;
        double sumXX = 0;
        double sumYY = 0;

        for (int i = 0; i < length; i++)
        {
            double dx = x[i] - avgX;
            double dy = y[i] - avgY;
            sumXY += dx * dy;
            sumXX += dx * dx;
            sumYY += dy * dy;
        }

        double denominator = Math.Sqrt(sumXX * sumYY);
        return denominator != 0 ? sumXY / denominator : 0;
    }
}

/// <summary>
/// Utility class for finding maximum linear correlation with offset
/// </summary>
public class CMaxLinearCorrelation : CLinearCorrelation
{
    private readonly int _offsetRange;
    private int _offset;

    public CMaxLinearCorrelation(int offsetRange = 0)
    {
        _offsetRange = offsetRange;
        _offset = 0;
    }

    public double MaxLinearCorrelation(short[] x, short[] y, int length)
    {
        double maxCorr = double.MinValue;
        _offset = 0;

        for (int offset = -_offsetRange; offset <= _offsetRange; offset++)
        {
            if (offset < 0 && -offset >= length) continue;
            if (offset > 0 && offset >= length) continue;

            int start = Math.Max(0, offset);
            int end = Math.Min(length, length + offset);
            int len = end - start;

            if (len <= 0) continue;

            short[] xSlice = new short[len];
            short[] ySlice = new short[len];

            for (int i = 0; i < len; i++)
            {
                xSlice[i] = x[start + i];
                ySlice[i] = y[i + (offset >= 0 ? 0 : -offset)];
            }

            double corr = LinearCorrelation(xSlice, ySlice, len);
            if (corr > maxCorr)
            {
                maxCorr = corr;
                _offset = offset;
            }
        }

        return maxCorr;
    }

    public double MaxLinearCorrelation(int[] x, int[] y, int length)
    {
        double maxCorr = double.MinValue;
        _offset = 0;

        for (int offset = -_offsetRange; offset <= _offsetRange; offset++)
        {
            if (offset < 0 && -offset >= length) continue;
            if (offset > 0 && offset >= length) continue;

            int start = Math.Max(0, offset);
            int end = Math.Min(length, length + offset);
            int len = end - start;

            if (len <= 0) continue;

            int[] xSlice = new int[len];
            int[] ySlice = new int[len];

            for (int i = 0; i < len; i++)
            {
                xSlice[i] = x[start + i];
                ySlice[i] = y[i + (offset >= 0 ? 0 : -offset)];
            }

            double corr = LinearCorrelation(xSlice, ySlice, len);
            if (corr > maxCorr)
            {
                maxCorr = corr;
                _offset = offset;
            }
        }

        return maxCorr;
    }

    public double MaxLinearCorrelation(float[] x, float[] y, int length)
    {
        double maxCorr = double.MinValue;
        _offset = 0;

        for (int offset = -_offsetRange; offset <= _offsetRange; offset++)
        {
            if (offset < 0 && -offset >= length) continue;
            if (offset > 0 && offset >= length) continue;

            int start = Math.Max(0, offset);
            int end = Math.Min(length, length + offset);
            int len = end - start;

            if (len <= 0) continue;

            float[] xSlice = new float[len];
            float[] ySlice = new float[len];

            for (int i = 0; i < len; i++)
            {
                xSlice[i] = x[start + i];
                ySlice[i] = y[i + (offset >= 0 ? 0 : -offset)];
            }

            double corr = LinearCorrelation(xSlice, ySlice, len);
            if (corr > maxCorr)
            {
                maxCorr = corr;
                _offset = offset;
            }
        }

        return maxCorr;
    }

    public double MaxLinearCorrelation(double[] x, double[] y, int length)
    {
        double maxCorr = double.MinValue;
        _offset = 0;

        for (int offset = -_offsetRange; offset <= _offsetRange; offset++)
        {
            if (offset < 0 && -offset >= length) continue;
            if (offset > 0 && offset >= length) continue;

            int start = Math.Max(0, offset);
            int end = Math.Min(length, length + offset);
            int len = end - start;

            if (len <= 0) continue;

            double[] xSlice = new double[len];
            double[] ySlice = new double[len];

            for (int i = 0; i < len; i++)
            {
                xSlice[i] = x[start + i];
                ySlice[i] = y[i + (offset >= 0 ? 0 : -offset)];
            }

            double corr = LinearCorrelation(xSlice, ySlice, len);
            if (corr > maxCorr)
            {
                maxCorr = corr;
                _offset = offset;
            }
        }

        return maxCorr;
    }

    public int GetOffset() => _offset;
}
