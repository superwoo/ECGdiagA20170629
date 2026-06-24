namespace EcgDiag;

/// <summary>
/// Base notch filter with highpass
/// </summary>
public class CBaseNotchFilter
{
    private int m_f, m_fs;
    private int L, M, K;
    private int d1, d2, d3;
    private int[]? x, y;
    private int xi, yi;

    public CBaseNotchFilter() { m_f = 0; m_fs = 0; }
    public CBaseNotchFilter(int NotFreq, int SampleRate) { m_f = 0; m_fs = 0; Init(NotFreq, SampleRate); }

    public void Init(int Freq = 0, int SampleRate = 0)
    {
        if (Freq > 0 || SampleRate > 0)
        {
            if (m_f != Freq || m_fs != SampleRate) { x = null; y = null; }
            if (Freq > 0) m_f = Freq;
            if (SampleRate > 0) m_fs = SampleRate;
        }
        int f = m_f, fs = m_fs;
        if (f != 50) { f = 50; fs = fs * 5 / 6; }
        L = fs / f; K = 2 * fs / L; M = 3 * fs / 4 - L / 2;
        d1 = fs / 2; d2 = fs; d3 = d1 + d2;
        if (x == null) x = new int[d3 + 1];
        if (y == null) y = new int[L + 1];
        xi = yi = 0;
        Array.Clear(x); Array.Clear(y);
    }

    public int Filter(int xt)
    {
        int d = d3 + 1;
        x![xi] = xt;
        int i = xi - d1; if (i < 0) i += d;
        int j = xi - d2; if (j < 0) j += d;
        int k = xi - d3; if (k < 0) k += d;
        int m = yi - L; if (m < 0) m += (L + 1);
        int n = xi - M; if (n < 0) n += d;
        y![yi] = x[xi] + x[i] - x[j] - x[k] + y[m];
        int yt = x[n] - y[yi] / K;
        xi++; if (xi > d3) xi = 0;
        yi++; if (yi > L) yi = 0;
        return yt;
    }
}

/// <summary>
/// Baseline notch filter with frequency division
/// </summary>
public class CBlNotchFilter
{
    private int m_f, m_fs, m_n, m_index;
    private CBaseNotchFilter[]? m_filters;

    public CBlNotchFilter() { m_f = 0; m_fs = 0; m_n = 0; m_index = 0; }
    public CBlNotchFilter(int NotFreq, int SampleRate) { m_f = 0; m_fs = 0; m_n = 0; m_index = 0; Init(NotFreq, SampleRate); }

    public void Init(int Freq = 0, int SampleRate = 0)
    {
        if (Freq > 0 || SampleRate > 0)
        {
            if (m_f != Freq || m_fs != SampleRate) m_filters = null;
            if (Freq > 0) m_f = Freq;
            if (SampleRate > 0) m_fs = SampleRate;
        }
        m_index = 0;
        int fs = m_f * 10;
        if (m_fs % fs == 0) m_n = m_fs / fs;
        else { m_n = 1; fs = m_fs; }
        m_filters = new CBaseNotchFilter[m_n];
        for (int i = 0; i < m_n; i++) m_filters[i] = new CBaseNotchFilter(m_f, fs);
    }

    public int Filter(int xt)
    {
        if (m_filters != null)
        {
            xt = m_filters[m_index++].Filter(xt);
            if (m_index >= m_n) m_index = 0;
        }
        return xt;
    }
}

/// <summary>
/// Multi-channel baseline notch filter
/// </summary>
public class CMutiBlNotchFilter
{
    private int m_chnum;
    private CBlNotchFilter[] blac;

    public CMutiBlNotchFilter(int Freq, int SampleRate, int ChlNumber)
    {
        m_chnum = ChlNumber;
        blac = new CBlNotchFilter[m_chnum];
        for (int i = 0; i < m_chnum; i++) blac[i] = new CBlNotchFilter(Freq, SampleRate);
    }

    public void Init(int Freq = 0, int SampleRate = 0)
    {
        for (int i = 0; i < m_chnum; i++) blac[i].Init(Freq);
    }

    public void Filter(int[] xt)
    {
        for (int i = 0; i < m_chnum; i++) xt[i] = blac[i].Filter(xt[i]);
    }

    public void Filter(short[] xt)
    {
        for (int i = 0; i < m_chnum; i++) xt[i] = (short)blac[i].Filter(xt[i]);
    }
}

/// <summary>
/// Real-time frequency interpolation
/// </summary>
public class FreqInterpretate
{
    private int m_fsrc, m_fdst;
    private int m_n, m_maxn;
    private double m_y0, m_t0, m_dt;
    private short[]? m_y;

    public FreqInterpretate() { m_fsrc = m_fdst = 0; m_n = m_maxn = 0; m_dt = 0; }
    public FreqInterpretate(int fsrc, int fdst) { m_fsrc = m_fdst = 0; m_n = m_maxn = 0; m_dt = 0; Init(fsrc, fdst); }

    public void Init(int fsrc = 0, int fdst = 0)
    {
        if (fsrc > 0 || fdst > 0)
        {
            if (m_fsrc != fsrc || m_fdst != fdst) m_y = null;
            if (fsrc > 0) m_fsrc = fsrc;
            if (fdst > 0) m_fdst = fdst;
        }
        if (m_fsrc > 0 && m_fdst > m_fsrc)
        {
            m_dt = (double)m_fsrc / m_fdst;
            m_maxn = (int)(1 / m_dt);
            if (m_fdst % m_fsrc != 0) m_maxn++;
        }
        else { m_fdst = m_fsrc; m_dt = 1; m_maxn = 1; }
        m_y0 = 0; m_t0 = 0;
        if (m_y == null) m_y = new short[m_maxn];
    }

    public short CalculateReturnValues(short CurrentValue)
    {
        double dy = (CurrentValue - m_y0) / (1 - m_t0);
        double t = m_t0;
        m_n = 0;
        while (t < 0.999999)
        {
            m_y![m_n++] = (short)(m_y0 + (t - m_t0) * dy);
            t += m_dt;
        }
        m_y0 = CurrentValue;
        m_t0 = t - 1;
        return (short)m_n;
    }

    public short[]? GetData() { return m_y; }
}

/// <summary>
/// Hspecg notch filter with optional frequency interpolation
/// </summary>
public class CHspecgNotchFilter
{
    private int m_f, m_fs;
    private bool m_bFint;
    private CBlNotchFilter m_blac = new();
    private FreqInterpretate m_fint = new();

    public CHspecgNotchFilter() { m_f = m_fs = 0; m_bFint = false; }
    public CHspecgNotchFilter(int NotFreq, int SampleRate) { m_f = m_fs = 0; Init(NotFreq, SampleRate); }

    public void Init(int Freq = 0, int SampleRate = 0)
    {
        m_bFint = false;
        if (Freq > 0) m_f = Freq;
        if (SampleRate > 0) m_fs = SampleRate;
        int f = m_f, fs = m_fs;
        if (m_fs % m_f == 0) { }
        else
        {
            m_bFint = true;
            fs = (short)(m_fs / 600.0 + 19.5) * 600;
            if (fs < 300) fs = 600;
            m_fint.Init(m_fs, fs);
        }
        if (f != 50) { fs = fs * 50 / f; f = 50; }
        m_blac.Init(f, fs);
    }

    public int Filter(int xt)
    {
        if (!m_bFint) return m_blac.Filter(xt);
        else
        {
            short n = m_fint.CalculateReturnValues((short)xt);
            short[]? p = m_fint.GetData();
            for (int k = 0; k < n; k++) p![k] = (short)m_blac.Filter(p[k]);
            return p![0];
        }
    }
}

/// <summary>
/// Multi-channel Hspecg notch filter
/// </summary>
public class CHspecgMultiNotchFilter
{
    private int m_chnum;
    private CHspecgNotchFilter[]? m_pNotchFilter;

    public CHspecgMultiNotchFilter() { m_chnum = 0; }
    public CHspecgMultiNotchFilter(int NotFreq, int SampleRate, int ChlNum)
    {
        m_chnum = ChlNum;
        m_pNotchFilter = new CHspecgNotchFilter[ChlNum];
        for (int i = 0; i < m_chnum; i++) m_pNotchFilter[i] = new CHspecgNotchFilter(NotFreq, SampleRate);
    }

    public void Init(int Freq = 0, int SampleRate = 0)
    {
        if (m_pNotchFilter != null)
            for (int i = 0; i < m_chnum; i++) m_pNotchFilter[i].Init(Freq, SampleRate);
    }

    public void Filter(int[] xt)
    {
        if (m_pNotchFilter != null)
            for (int i = 0; i < m_chnum; i++) xt[i] = m_pNotchFilter[i].Filter(xt[i]);
    }

    public void Filter(short[] xt)
    {
        if (m_pNotchFilter != null)
            for (int i = 0; i < m_chnum; i++) xt[i] = (short)m_pNotchFilter[i].Filter(xt[i]);
    }
}
