namespace ECGDiag.Filters;

/// <summary>
/// Base notch filter with highpass and lowpass filtering
/// 同时进行高通和陷波滤波，主要滤波频率不低于500/600Hz（为50/60基波），陷波带宽1.2Hz，高通0.6Hz
/// </summary>
public class CBaseNotchFilter
{
    private int _f;
    private int _fs;
    private int _L, _M, _K;
    private int _d1, _d2, _d3;
    private int[]? _x;
    private int[]? _y;
    private int _xi, _yi;

    public CBaseNotchFilter()
    {
        _x = null;
        _y = null;
        _f = 0;
        _fs = 0;
    }

    public CBaseNotchFilter(int notFreq, int sampleRate)
    {
        _x = null;
        _y = null;
        _f = 0;
        _fs = 0;
        Init(notFreq, sampleRate);
    }

    public void Init(int freq = 0, int sampleRate = 0)
    {
        if (freq > 0 || sampleRate > 0)
        {
            if (_f != freq || _fs != sampleRate)
            {
                _x = null;
                _y = null;
            }
            if (freq > 0) _f = freq;
            if (sampleRate > 0) _fs = sampleRate;
        }

        int f = _f, fs = _fs;
        if (f != 50)
        {
            f = 50;
            fs = fs * 5 / 6;
        }

        _L = fs / f;
        _K = 2 * fs / _L;
        _M = 3 * fs / 4 - _L / 2;
        _d1 = fs / 2;
        _d2 = fs;
        _d3 = _d1 + _d2;

        _x ??= new int[_d3 + 1];
        _y ??= new int[_L + 1];

        _xi = _yi = 0;
        Array.Fill(_x, 0);
        Array.Fill(_y, 0);
    }

    public int Filter(int xt)
    {
        int d = _d3 + 1;
        _x![_xi] = xt;

        int i = _xi - _d1; if (i < 0) i += d;
        int j = _xi - _d2; if (j < 0) j += d;
        int k = _xi - _d3; if (k < 0) k += d;
        int m = _yi - _L; if (m < 0) m += (_L + 1);
        int n = _xi - _M; if (n < 0) n += d;

        _y![_yi] = _x[_xi] + _x[i] - _x[j] - _x[k] + _y[m];
        int yt = _x[n] - _y[_yi] / _K;

        _xi++;
        if (_xi > _d3) _xi = 0;
        _yi++;
        if (_yi > _L) _yi = 0;

        return yt;
    }
}

/// <summary>
/// Bandlimited notch filter
/// 同时进行高通和陷波滤波，主要滤波频率为陷波频率的倍数，陷波带宽1.2Hz，高通0.6Hz
/// </summary>
public class CBlNotchFilter
{
    private int _f;
    private int _fs;
    private int _n;
    private int _index;
    private CBaseNotchFilter[]? _filters;

    public CBlNotchFilter()
    {
        _f = 0;
        _fs = 0;
        _n = 0;
        _index = 0;
        _filters = null;
    }

    public CBlNotchFilter(int notFreq, int sampleRate)
    {
        _f = 0;
        _fs = 0;
        _n = 0;
        _index = 0;
        _filters = null;
        Init(notFreq, sampleRate);
    }

    private void Destroy()
    {
        _filters = null;
    }

    public void Init(int freq = 0, int sampleRate = 0)
    {
        if (freq > 0 || sampleRate > 0)
        {
            if (_f != freq || _fs != sampleRate)
            {
                Destroy();
            }
            if (freq > 0) _f = freq;
            if (sampleRate > 0) _fs = sampleRate;
        }

        _index = 0;
        int fs = _f * 10;
        if (_fs % fs == 0)
        {
            _n = _fs / fs;
        }
        else
        {
            _n = 1;
            fs = _fs;
        }

        _filters = new CBaseNotchFilter[_n];
        for (int i = 0; i < _n; i++)
        {
            _filters[i] = new CBaseNotchFilter(_f, fs);
        }
    }

    public int Filter(int xt)
    {
        if (_filters != null)
        {
            xt = _filters[_index++].Filter(xt);
            if (_index >= _n) _index = 0;
        }
        return xt;
    }
}

/// <summary>
/// Multi-channel bandlimited notch filter
/// </summary>
public class CMutiBlNotchFilter
{
    private readonly int _chnum;
    private readonly CBlNotchFilter[] _blac;

    public CMutiBlNotchFilter(int freq, int sampleRate, int chlNumber)
    {
        _chnum = chlNumber;
        _blac = new CBlNotchFilter[_chnum];
        for (int i = 0; i < _chnum; i++)
        {
            _blac[i] = new CBlNotchFilter(freq, sampleRate);
        }
    }

    public void Init(int freq = 0, int sampleRate = 0)
    {
        for (int i = 0; i < _chnum; i++)
        {
            _blac[i].Init(freq, sampleRate);
        }
    }

    public void Filter(int[] xt)
    {
        for (int i = 0; i < _chnum; i++)
        {
            xt[i] = _blac[i].Filter(xt[i]);
        }
    }

    public void Filter(short[] xt)
    {
        for (int i = 0; i < _chnum; i++)
        {
            xt[i] = (short)_blac[i].Filter(xt[i]);
        }
    }
}

/// <summary>
/// Frequency interpolation for real-time upsampling
/// 实时插值提频，插值频率需比原始频率高
/// </summary>
public class FreqInterpretate
{
    private int _fsrc, _fdst;      // 原始频率，插值后频率
    private int _n, _maxn;         // 实际插值个数，最大插值个数
    private double _y0;
    private double _t0;
    private double _dt;            // 间隔系数
    private short[]? _y;

    public FreqInterpretate()
    {
        _fsrc = _fdst = 0;
        _n = _maxn = 0;
        _dt = 0;
        _y = null;
    }

    public FreqInterpretate(int fsrc, int fdst)
    {
        _fsrc = _fdst = 0;
        _n = _maxn = 0;
        _dt = 0;
        _y = null;
        Init(fsrc, fdst);
    }

    public void Init(int fsrc = 0, int fdst = 0)
    {
        if (fsrc > 0 || fdst > 0)
        {
            if (_fsrc != fsrc || _fdst != fdst)
            {
                _y = null;
            }
            if (fsrc > 0) _fsrc = fsrc;
            if (fdst > 0) _fdst = fdst;
        }

        if (_fsrc > 0 && _fdst > _fsrc)
        {
            _dt = (double)_fsrc / _fdst;
            _maxn = (int)(1 / _dt);
            if (_fdst % _fsrc != 0) _maxn++;
        }
        else
        {
            _fdst = _fsrc;
            _dt = 1;
            _maxn = 1;
        }

        _y0 = 0;
        _t0 = 0;
        _y ??= new short[_maxn];
    }

    /// <summary>
    /// Calculate interpolated values and return count
    /// 计算插值返回值个数
    /// </summary>
    public short CalculateReturnValues(short currentValue)
    {
        double dy = (currentValue - _y0) / (1 - _t0);
        double t = _t0;
        _n = 0;

        while (t < 0.999999)
        {
            _y![_n++] = (short)(_y0 + (t - _t0) * dy);
            t += _dt;
        }

        _y0 = currentValue;
        _t0 = t - 1;
        return (short)_n;
    }

    /// <summary>
    /// Get interpolated data array
    /// 获取插值数据数组
    /// </summary>
    public short[] GetData() => _y!;
}

/// <summary>
/// HSpecg notch filter with optional frequency interpolation
/// </summary>
public class CHspecgNotchFilter
{
    private int _f;
    private int _fs;
    private bool _bFint;
    private readonly CBlNotchFilter _blac;
    private readonly FreqInterpretate _fint;

    public CHspecgNotchFilter()
    {
        _f = _fs = 0;
        _bFint = false;
        _blac = new CBlNotchFilter();
        _fint = new FreqInterpretate();
    }

    public CHspecgNotchFilter(int notFreq, int sampleRate)
    {
        _f = _fs = 0;
        _blac = new CBlNotchFilter();
        _fint = new FreqInterpretate();
        Init(notFreq, sampleRate);
    }

    public void Init(int freq = 0, int sampleRate = 0)
    {
        _bFint = false;
        if (freq > 0) _f = freq;
        if (sampleRate > 0) _fs = sampleRate;

        int f = _f, fs = _fs;
        if (_fs % _f != 0)
        {
            _bFint = true;
            fs = (short)(_fs / 600.0 + 19.5) * 600;
            if (fs < 300) fs = 600;
            _fint.Init(_fs, fs);
        }

        if (f != 50)
        {
            fs = fs * 50 / f;
            f = 50;
        }
        _blac.Init(f, fs);
    }

    public int Filter(int xt)
    {
        if (!_bFint)
        {
            return _blac.Filter(xt);
        }
        else
        {
            short n = _fint.CalculateReturnValues((short)xt);
            short[] p = _fint.GetData();
            for (int k = 0; k < n; k++)
            {
                p[k] = (short)_blac.Filter(p[k]);
            }
            return p[0];
        }
    }
}

/// <summary>
/// Multi-channel HSpecg notch filter
/// </summary>
public class CHspecgMultiNotchFilter
{
    private int _chnum;
    private CHspecgNotchFilter[]? _pNotchFilter;

    public CHspecgMultiNotchFilter()
    {
        _chnum = 0;
        _pNotchFilter = null;
    }

    public CHspecgMultiNotchFilter(int notFreq, int sampleRate, int chlNum)
    {
        _chnum = chlNum;
        _pNotchFilter = new CHspecgNotchFilter[chlNum];
        for (int i = 0; i < _chnum; i++)
        {
            _pNotchFilter[i] = new CHspecgNotchFilter(notFreq, sampleRate);
        }
    }

    public void Init(int freq = 0, int sampleRate = 0)
    {
        if (_pNotchFilter != null)
        {
            for (int i = 0; i < _chnum; i++)
            {
                _pNotchFilter[i].Init(freq, sampleRate);
            }
        }
    }

    public void Filter(int[] xt)
    {
        if (_pNotchFilter != null)
        {
            for (int i = 0; i < _chnum; i++)
            {
                xt[i] = _pNotchFilter[i].Filter(xt[i]);
            }
        }
    }

    public void Filter(short[] xt)
    {
        if (_pNotchFilter != null)
        {
            for (int i = 0; i < _chnum; i++)
            {
                xt[i] = (short)_pNotchFilter[i].Filter(xt[i]);
            }
        }
    }
}
