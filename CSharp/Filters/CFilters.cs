namespace EcgDiag;

/// <summary>
/// 2-order lowpass filter (Butterworth)
/// </summary>
public class CLowpassFilter2
{
    private double a1, a2, b0, b1, b2;
    private double x1, x2, y1, y2;
    private double fc;
    private int fs;

    public CLowpassFilter2(double Freq, int SampleRate) { Init(Freq, SampleRate); }

    public void Init(double Freq, int SampleRate)
    {
        fc = Freq; fs = SampleRate;
        double c = 1.0 / Math.Tan(Math.PI * Freq / SampleRate);
        double cc = c * c, s2 = Math.Sqrt(2.0);
        double d0 = cc + s2 * c + 1;
        double d1 = -2 * (cc - 1);
        double d2 = cc - s2 * c + 1;
        b0 = 1.0 / d0; b1 = 2.0 / d0; b2 = 1.0 / d0;
        a1 = d1 / d0; a2 = d2 / d0;
        x1 = x2 = y1 = y2 = 0;
    }

    public void Init(double Freq) { Init(Freq, fs); }
    public void Init() { Init(fc, fs); }
    public void InitialValue(int xt) { x1 = x2 = y1 = y2 = xt; }
    public void InitialValue(double xt) { x1 = x2 = y1 = y2 = xt; }

    public int Filter(int xt)
    {
        double yt = b0 * xt + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
        x2 = x1; x1 = xt; y2 = y1; y1 = yt;
        return (int)yt;
    }

    public double Filter(double xt)
    {
        double yt = b0 * xt + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
        x2 = x1; x1 = xt; y2 = y1; y1 = yt;
        return yt;
    }
}

/// <summary>
/// Highpass filter (same algorithm as Filters.cs HighpassFilter but for CFilters)
/// </summary>
public class CHighpassFilter
{
    private int fs;
    private double fc;
    private double a0, a1, a2, b1, b2;
    private double x1, x2, y1, y2;

    public CHighpassFilter(double Freq, int SampleRate)
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
        a0 = tscp * tscp / a; a1 = -2.0 * a0; a2 = a0;
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

    public double Filter(double xt)
    {
        double yt = a0 * xt + a1 * x1 + a2 * x2 - b1 * y1 - b2 * y2;
        if (Math.Abs(yt) < 1.0e-100) yt = 0;
        x2 = x1; x1 = xt; y2 = y1; y1 = yt;
        return yt;
    }
}

/// <summary>
/// 2-order notch filter
/// </summary>
public class CNotchFilter
{
    private double Q, fc;
    private int fs;
    private double a1, a2, b0, b1, b2;
    private double x1, x2, y1, y2;

    public CNotchFilter(double Freq, int SampleRate, double q = 40) { Init(Freq, SampleRate, q); }

    public void Init(double Freq, int SampleRate, double q = 40)
    {
        Q = q; fc = Freq; fs = SampleRate;
        double c = 1.0 / Math.Tan(Math.PI * Freq / SampleRate);
        double n0 = q * (c * c + 1); double n1 = -2 * q * (c * c - 1); double n2 = n0;
        double d0 = n0 + c; double d1 = n1; double d2 = n0 - c;
        b0 = n0 / d0; b1 = n1 / d0; b2 = n2 / d0; a1 = d1 / d0; a2 = d2 / d0;
        x1 = x2 = y1 = y2 = 0;
    }

    public void Init(double Freq) { Init(Freq, fs, Q); }
    public void Init() { Init(fc, fs, Q); }

    public int Filter(int xt)
    {
        double yt = b0 * xt + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
        x2 = x1; x1 = xt; y2 = y1; y1 = yt;
        return (int)yt;
    }

    public double Filter(double xt)
    {
        double yt = b0 * xt + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;
        x2 = x1; x1 = xt; y2 = y1; y1 = yt;
        return yt;
    }
}

/// <summary>
/// Smooth filter (depends on power)
/// </summary>
public class CSmoothFilter
{
    private int[] m_pSpr, m_pLowPass, m_pNoise;
    private int Width, Pos, PhaseShift, SamplingRate;
    private int sw1, sw2, sw3, sw4, sw5, sw6, sw7, sw8, sw9;
    private long Count;
    private double TotalSum, WindowSum;
    private CLowpassFilter2 LowPass;

    public CSmoothFilter(int SampleRate)
    {
        SamplingRate = SampleRate;
        Width = 30 * SamplingRate / 1000;
        m_pSpr = new int[Width]; m_pLowPass = new int[Width]; m_pNoise = new int[Width];
        LowPass = new CLowpassFilter2(25, SampleRate);
        Init();
    }

    public void Init()
    {
        Pos = 0; PhaseShift = 9 * SamplingRate / 1000;
        Array.Clear(m_pSpr); Array.Clear(m_pLowPass); Array.Clear(m_pNoise);
        Count = 0; TotalSum = 0; WindowSum = 0;
        sw1 = 1 * SamplingRate / 1000; sw2 = 2 * SamplingRate / 1000;
        sw3 = 3 * SamplingRate / 1000; sw4 = 4 * SamplingRate / 1000;
        sw5 = 5 * SamplingRate / 1000; sw6 = 6 * SamplingRate / 1000;
        sw7 = 7 * SamplingRate / 1000; sw8 = 8 * SamplingRate / 1000;
        sw9 = 9 * SamplingRate / 1000;
    }

    public int Smooth(int xt)
    {
        int sw, CrectValue;
        m_pSpr[Pos] = xt;
        m_pLowPass[Pos] = LowPass.Filter(xt);
        CrectValue = m_pNoise[Pos];
        m_pNoise[Pos] = m_pSpr[(Pos - PhaseShift + Width) % Width] - m_pLowPass[Pos];
        Count++;
        TotalSum += Math.Abs(m_pNoise[Pos]);
        WindowSum += (Math.Abs(m_pNoise[Pos]) - Math.Abs(CrectValue));
        double threshold = (double)Width * TotalSum / Count;
        if (WindowSum > 4.0 * threshold) sw = 0;
        else if (WindowSum > 3.5 * threshold) sw = sw1;
        else if (WindowSum > 3.0 * threshold) sw = sw2;
        else if (WindowSum > 2.6 * threshold) sw = sw3;
        else if (WindowSum > 2.2 * threshold) sw = sw4;
        else if (WindowSum > 1.8 * threshold) sw = sw5;
        else if (WindowSum > 1.5 * threshold) sw = sw6;
        else if (WindowSum > 1.2 * threshold) sw = sw7;
        else if (WindowSum > 1.0 * threshold) sw = sw8;
        else sw = sw9;
        if (sw == 0)
        {
            CrectValue = (2 * m_pNoise[(Pos + Width / 2) % Width] + m_pNoise[(Pos + Width / 2 - 1) % Width] + m_pNoise[(Pos + Width / 2 + 1) % Width]) / 4;
        }
        else
        {
            CrectValue = m_pNoise[(Pos + Width / 2 + sw) % Width];
            for (int i = -sw; i < sw; i++) CrectValue += m_pNoise[(Pos + Width / 2 + i) % Width];
            CrectValue /= (sw * 2 + 1);
        }
        CrectValue += m_pLowPass[(Pos + Width / 2) % Width];
        Pos = (Pos + 1) % Width;
        return CrectValue;
    }
}

/// <summary>
/// PaceMaker spike detection and removal
/// </summary>
public class CPaceMaker
{
    private int fs, m_nLowPassFreq;
    private bool m_bSmooth;
    private int LowPassPhaseShift, SmoothPhaseShift, PhaseShift, ShiftCount;
    private int ThresholdValue, ReserveValue, RestoreValue;
    private int[] m_Reserve;
    private int iRes, jRes;
    private int SpikeWidth, WidthCount;
    private bool PaceFound, Start, FirstTime;
    private int MaxPace, MinPace, PaceCount;
    private int[] m_Source, m_Differ;
    private int Width, Pos;

    public CPaceMaker(int LowPassFreq, bool Smooth, int SampleRate)
    {
        fs = SampleRate; m_nLowPassFreq = LowPassFreq; m_bSmooth = Smooth;
        SpikeWidth = 20 * SampleRate / 1000; WidthCount = SpikeWidth;
        PaceFound = false; ThresholdValue = 750;
        SmoothPhaseShift = Smooth ? 24 * SampleRate / 1000 : 0;
        LowPassPhaseShift = CalculateLowPassPhaseShift(LowPassFreq, SampleRate);
        PhaseShift = SmoothPhaseShift + LowPassPhaseShift + 1;
        ShiftCount = PhaseShift;
        m_Reserve = new int[SpikeWidth + 1];
        iRes = jRes = 0; Start = false; MaxPace = 0;
        MinPace = 4 * SampleRate / 1000; PaceCount = 0; FirstTime = true;
        Width = 4 * SampleRate / 1000;
        m_Source = new int[Width]; m_Differ = new int[Width]; Pos = 0;
    }

    private static int CalculateLowPassPhaseShift(int LowPassFreq, int SampleRate)
    {
        if (LowPassFreq == 0) return 0;
        else if (LowPassFreq < 30) return 10 * SampleRate / 1000;
        else if (LowPassFreq < 40) return 8 * SampleRate / 1000;
        else if (LowPassFreq < 50) return 7 * SampleRate / 1000;
        else if (LowPassFreq < 60) return 6 * SampleRate / 1000;
        else if (LowPassFreq < 70) return 5 * SampleRate / 1000;
        else if (LowPassFreq < 80) return 4 * SampleRate / 1000;
        else if (LowPassFreq < 100) return 3 * SampleRate / 1000;
        else if (LowPassFreq < 120) return 2 * SampleRate / 1000;
        else if (LowPassFreq <= 150) return 1 * SampleRate / 1000;
        else return 0;
    }

    public void Init() { Init(m_nLowPassFreq, m_bSmooth); }

    public void Init(int nLowPassFreq, bool bSmooth)
    {
        WidthCount = SpikeWidth; PaceFound = false; ThresholdValue = 750;
        SmoothPhaseShift = bSmooth ? 24 * fs / 1000 : 0;
        LowPassPhaseShift = CalculateLowPassPhaseShift(nLowPassFreq, fs);
        PhaseShift = SmoothPhaseShift + LowPassPhaseShift + 1;
        ShiftCount = PhaseShift;
        for (iRes = 0; iRes <= SpikeWidth; iRes++) m_Reserve[iRes] = 0;
        iRes = jRes = 0; Start = false; MaxPace = 0;
        MinPace = 4 * fs / 1000; PaceCount = 0; FirstTime = true;
        for (Pos = 0; Pos < Width; Pos++) { m_Source[Pos] = 0; m_Differ[Pos] = 0; }
        Pos = 0;
    }

    public int Remove(int xt)
    {
        int xv = m_Source[Pos];
        Pos = (Pos + 1) % Width;
        m_Differ[Pos] = xt - xv;
        xv = m_Source[Pos]; m_Source[Pos] = xt;
        if (PaceFound)
        {
            WidthCount--;
            if (WidthCount == 0) PaceFound = false;
            RestoreValue = xv; xv = ReserveValue;
        }
        else if (Math.Abs(m_Differ[(Pos + 1) % Width]) > ThresholdValue)
        {
            for (int i = 1; i < Width; i++)
            {
                if (Math.Abs(m_Differ[(Pos + 1 + i) % Width]) > ThresholdValue &&
                    m_Differ[(Pos + 1) % Width] * m_Differ[(Pos + 1 + i) % Width] < 0)
                    PaceFound = true;
            }
            if (PaceFound) { WidthCount = SpikeWidth; ReserveValue = xv; }
        }
        return xv;
    }

    public int Recover(int xt)
    {
        int xv = xt;
        if (PaceFound)
        {
            if (WidthCount == SpikeWidth) { Start = true; iRes = 0; jRes = 0; ShiftCount = PhaseShift; }
            else m_Reserve[iRes++] = RestoreValue;
        }
        if (Start && ShiftCount > 0) ShiftCount--;
        if (ShiftCount == 0) { Start = false; if (jRes < iRes) xv = m_Reserve[jRes++]; }
        if (jRes == 1)
        {
            if (FirstTime) FirstTime = false;
            else { if (PaceCount > MaxPace) MaxPace = PaceCount; if (PaceCount < MinPace) MinPace = PaceCount; }
            PaceCount = 0;
        }
        else PaceCount++;
        return xv;
    }

    public int GetSpikes()
    {
        if (jRes > 0 && jRes < SpikeWidth / 3) return m_Reserve[jRes - 1];
        else return 0;
    }

    public int Type() { if (MaxPace == 0) return 0; if (MaxPace > 3 * MinPace) return 2; else return 1; }
}

/// <summary>
/// Multi-channel lowpass
/// </summary>
public class CMultiChannelLowpass2
{
    private double fc; private int fs, ChlNum;
    private CLowpassFilter2[] pass;

    public CMultiChannelLowpass2(double Freq, int SampleRate, int Channel)
    {
        fc = Freq; ChlNum = Channel; fs = SampleRate;
        pass = new CLowpassFilter2[ChlNum];
        for (int i = 0; i < ChlNum; i++) pass[i] = new CLowpassFilter2(Freq, SampleRate);
    }

    public void Init(double Freq, int SampleRate) { fc = Freq; fs = SampleRate; for (int i = 0; i < ChlNum; i++) pass[i].Init(Freq, SampleRate); }
    public void Init(double Freq) { Init(Freq, fs); }
    public void Init() { Init(fc, fs); }
    public void Filter(short[] data) { for (int i = 0; i < ChlNum; i++) data[i] = (short)pass[i].Filter(data[i]); }
}

/// <summary>
/// Multi-channel highpass
/// </summary>
public class CMultiChannelHighpass
{
    private double hfreq; private int fs, ChlNum;
    private CHighpassFilter[] pass;

    public CMultiChannelHighpass(double Freq, int SampleRate, int Channel)
    {
        hfreq = Freq; ChlNum = Channel; fs = SampleRate;
        pass = new CHighpassFilter[ChlNum];
        for (int i = 0; i < ChlNum; i++) pass[i] = new CHighpassFilter(Freq, SampleRate);
    }

    public void Init(double Freq, int SampleRate) { for (int i = 0; i < ChlNum; i++) pass[i].Init(Freq, SampleRate); }
    public void Init(double Freq) { Init(Freq, fs); }
    public void Init() { Init(hfreq, fs); }
    public void Filter(short[] data) { for (int i = 0; i < ChlNum; i++) data[i] = (short)pass[i].Filter(data[i]); }
}

/// <summary>
/// Multi-channel notch
/// </summary>
public class CMultiChannelNotch
{
    private double Q, fc; private int fs, ChlNum;
    private CNotchFilter[] pass;

    public CMultiChannelNotch(double Freq, int SampleRate, int Channel, double q = 40)
    {
        Q = q; fc = Freq; fs = SampleRate; ChlNum = Channel;
        pass = new CNotchFilter[ChlNum];
        for (int i = 0; i < ChlNum; i++) pass[i] = new CNotchFilter(Freq, SampleRate, q);
    }

    public void Init(double Freq, int SampleRate, double q = 40) { Q = q; fc = Freq; fs = SampleRate; for (int i = 0; i < ChlNum; i++) pass[i].Init(Freq, SampleRate, q); }
    public void Init(double Freq) { Init(Freq, fs, Q); }
    public void Init() { Init(fc, fs, Q); }
    public void Filter(short[] data) { for (int i = 0; i < ChlNum; i++) data[i] = (short)pass[i].Filter(data[i]); }
}

/// <summary>
/// Multi-channel smooth
/// </summary>
public class CMultiChannelSmooth
{
    private int ChlNum;
    private CSmoothFilter[] smooth;

    public CMultiChannelSmooth(int SampleRate, int Channel)
    {
        ChlNum = Channel;
        smooth = new CSmoothFilter[ChlNum];
        for (int i = 0; i < ChlNum; i++) smooth[i] = new CSmoothFilter(SampleRate);
    }

    public void Init() { for (int i = 0; i < ChlNum; i++) smooth[i].Init(); }
    public void Smooth(short[] data) { for (int i = 0; i < ChlNum; i++) data[i] = (short)smooth[i].Smooth(data[i]); }
}

/// <summary>
/// Multi-channel pacemaker
/// </summary>
public class CMultiChannelPaceMaker
{
    private int ChlNum;
    private CPaceMaker[] pace;

    public CMultiChannelPaceMaker(int LowPassFreq, bool Smooth, int SampleRate, int Channel)
    {
        ChlNum = Channel;
        pace = new CPaceMaker[ChlNum];
        for (int i = 0; i < ChlNum; i++) pace[i] = new CPaceMaker(LowPassFreq, Smooth, SampleRate);
    }

    public void Init() { for (int i = 0; i < ChlNum; i++) pace[i].Init(); }
    public void Init(int LowPassFreq, bool Smooth) { for (int i = 0; i < ChlNum; i++) pace[i].Init(LowPassFreq, Smooth); }
    public void Remove(short[] data) { for (int i = 0; i < ChlNum; i++) data[i] = (short)pace[i].Remove(data[i]); }
    public void Recover(short[] data) { for (int i = 0; i < ChlNum; i++) data[i] = (short)pace[i].Recover(data[i]); }
    public void GetSpikes(short[] Spikes) { for (int i = 0; i < ChlNum; i++) Spikes[i] = (short)pace[i].GetSpikes(); }
    public void Type(short[] types) { for (int i = 0; i < ChlNum; i++) types[i] = (short)pace[i].Type(); }
}

/// <summary>
/// Length transform for QRS detection
/// </summary>
public class CLengthTransform
{
    private int wms, fs, w;
    private int p, Li;
    private int[] pre = new int[3];
    private int[] L = Array.Empty<int>();

    public CLengthTransform(int SampleRate, int ms = 65) { w = 0; wms = -1; fs = -1; Init(SampleRate, ms); }

    public void Init(int SampleRate, int ms = 65)
    {
        if (ms != wms || SampleRate != fs) L = Array.Empty<int>();
        wms = ms; fs = SampleRate; w = wms * fs / 1000;
        if (L.Length == 0) L = new int[w];
        Init();
    }

    public void Init() { p = 0; Li = 0; pre[0] = pre[1] = pre[2] = 0; Array.Clear(L); }

    public int Length(int xt)
    {
        L[p] = Math.Abs(xt - pre[0]);
        pre[0] = xt;
        Li += L[p]; p = (p + 1) % w; Li -= L[p];
        return Li;
    }

    public int Length(int xt, int yt, int zt)
    {
        L[p] = (Math.Abs(xt - pre[0]) + Math.Abs(yt - pre[1]) + Math.Abs(zt - pre[2])) / 3;
        pre[0] = xt; pre[1] = yt; pre[2] = zt;
        Li += L[p]; p = (p + 1) % w; Li -= L[p];
        return Li;
    }
}

/// <summary>
/// Realtime RR detect based on CLengthTransform
/// </summary>
public class CrtRRdetect
{
    private int fs, notch;
    private int sec2, sec3, ms240;
    private int count, rr;
    private int minp, maxp, er;
    private int lw, w, q;
    private int pre, repeat, status;
    private long doing;

    private CLengthTransform xyzLength;
    private CLowpassFilter2 xLow, yLow, zLow;
    private CHighpassFilter xHigh, yHigh, zHigh;
    private CNotchFilter? xNotch, yNotch, zNotch;

    public CrtRRdetect(int SampleRate, int freqNotch = 50)
    {
        fs = SampleRate; notch = freqNotch;
        sec2 = fs * 2; sec3 = fs * 3; ms240 = fs * 240 / 1000;
        lw = 100; q = fs * lw / 1000;
        xLow = new CLowpassFilter2(10, fs); yLow = new CLowpassFilter2(10, fs); zLow = new CLowpassFilter2(10, fs);
        xHigh = new CHighpassFilter(7, fs); yHigh = new CHighpassFilter(7, fs); zHigh = new CHighpassFilter(7, fs);
        if (notch > 0) { xNotch = new CNotchFilter(notch, fs); yNotch = new CNotchFilter(notch, fs); zNotch = new CNotchFilter(notch, fs); }
        xyzLength = new CLengthTransform(fs, lw);
        Init();
    }

    public void Init()
    {
        w = 0; count = 0; rr = 0; maxp = 0; minp = 0x7fff; er = 0;
        pre = fs / 2; doing = 0; repeat = 0; status = 0;
        xLow.Init(10, fs); yLow.Init(10, fs); zLow.Init(10, fs);
        xHigh.Init(7, fs); yHigh.Init(7, fs); zHigh.Init(7, fs);
        if (notch > 0) { xNotch?.Init(notch, fs); yNotch?.Init(notch, fs); zNotch?.Init(notch, fs); }
        xyzLength.Init();
    }

    public void Init(int freqNotch) { notch = freqNotch; Init(); }

    public int QRSdetect(int xt)
    {
        if (xNotch != null) xt = xNotch.Filter(xt);
        xt = xLow.Filter(xt); xt = xHigh.Filter(xt);
        xt = xyzLength.Length(xt);
        return RRdetect(xt);
    }

    public int QRSdetect(int xt, int yt, int zt)
    {
        if (notch > 0) { xt = xNotch!.Filter(xt); yt = yNotch!.Filter(yt); zt = zNotch!.Filter(zt); }
        xt = xLow.Filter(xt); yt = yLow.Filter(yt); zt = zLow.Filter(zt);
        xt = xHigh.Filter(xt); yt = yHigh.Filter(yt); zt = zHigh.Filter(zt);
        xt = xyzLength.Length(xt, yt, zt);
        return RRdetect(xt);
    }

    private int RRdetect(int xt)
    {
        rr = 0;
        switch (status)
        {
            case 0:
                if (doing < pre) { xt = 0; doing++; }
                if (minp > xt) minp = xt;
                if (maxp < xt) maxp = xt;
                count++;
                if (count > sec2)
                {
                    count = 0; er = minp + (maxp - minp) / 3;
                    if (er > 0) { doing = 0; repeat = 0; maxp = 0; minp = 0x7fff; status = (xt < er) ? 2 : 1; }
                }
                break;
            case 1:
                doing++; count++;
                if (xt < er && doing > q) { doing = 0; status = 2; }
                break;
            case 2:
                count++;
                if (xt >= er) { rr = count; count = 0; repeat++; if (repeat >= 5) { if (maxp - minp > 3) er = minp + (maxp - minp) / 3; repeat = 0; maxp = 0; minp = 0x7fff; } status = 3; }
                if (count > sec3) status = 0;
                break;
            case 3:
                count++;
                if (count > ms240 && xt < er) status = 2;
                if (count > sec3) status = 0;
                if (minp > xt) minp = xt;
                if (maxp < xt) maxp = xt;
                break;
        }
        return rr;
    }
}

/// <summary>
/// Static helper class for free functions
/// </summary>
public static class CFiltersHelper
{
    public static void ButterwirthFilter(int Freq, int SampleRate, double[] data, int length, int maxp)
    {
        double[] ab = new double[10]; // ab[2][5] flattened
        double[] c = { 0.5883003, 0.24350705 };
        double fsfc = SampleRate / (double)Freq;
        double tscp = fsfc / Math.PI;

        for (int i = 0; i < 2; i++)
        {
            double a = 1.0 + c[i] * fsfc + tscp * tscp;
            ab[i * 5 + 0] = tscp * tscp / a;
            ab[i * 5 + 1] = -2.0 * ab[i * 5 + 0];
            ab[i * 5 + 2] = ab[i * 5 + 0];
            ab[i * 5 + 3] = (2.0 - 2.0 * tscp * tscp) / a;
            ab[i * 5 + 4] = (1.0 - c[i] * fsfc + tscp * tscp) / a;
        }

        double[] y = new double[length];
        for (int i = 0; i < 2; i++)
        {
            y[0] = y[1] = 0.0;
            for (int j = 2; j <= maxp; j++)
                y[j] = (ab[i * 5 + 0] * data[j] + ab[i * 5 + 1] * data[j - 1] + ab[i * 5 + 2] * data[j - 2])
                      - (ab[i * 5 + 3] * y[j - 1] + ab[i * 5 + 4] * y[j - 2]);
            for (int j = 0; j < maxp; j++) data[j] = y[j];
        }
        for (int i = 0; i < 2; i++)
        {
            y[length - 1] = y[length - 2] = 0.0;
            for (int j = length - 3; j >= maxp; j--)
                y[j] = (ab[i * 5 + 0] * data[j] + ab[i * 5 + 1] * data[j + 1] + ab[i * 5 + 2] * data[j + 2])
                      - (ab[i * 5 + 3] * y[j + 1] + ab[i * 5 + 4] * y[j + 2]);
            for (int j = length - 1; j >= maxp; j--) data[j] = y[j];
        }
    }

    public static void PostProcess(int Channel, double[][] data, int n, int NotchFreq, int SampleRate)
    {
        int ch = Channel == 1 ? 0 : 1;
        double f = NotchFreqDetect(data[ch], n, NotchFreq, SampleRate);
        CNotchFilter notch = new CNotchFilter(f, SampleRate);
        for (ch = 0; ch < Channel; ch++)
        {
            for (int i = 0; i < n; i++) data[ch][i] = notch.Filter(data[ch][i]);
            for (int i = n - 1; i >= 0; i--) data[ch][i] = notch.Filter(data[ch][i]);
        }
    }

    public static double NotchFreqDetect(double[] data, int n, int NotchFreq, int SampleRate)
    {
        double step = 0.0001;
        double f0 = NotchFreq - 0.5, f1 = NotchFreq + 0.5;
        double div0 = NotchDiv(f0, data, n, SampleRate);
        double div1 = NotchDiv(f1, data, n, SampleRate);
        while (f1 - f0 > step)
        {
            if (div0 < div1) { f1 -= ((f1 - f0) / 4.0); div1 = NotchDiv(f1, data, n, SampleRate); }
            else { f0 += ((f1 - f0) / 4.0); div0 = NotchDiv(f0, data, n, SampleRate); }
        }
        return f0 + (f1 - f0) / 2.0;
    }

    public static double NotchDiv(double f, double[] data, int n, int SampleRate)
    {
        double[] temp = new double[n];
        CNotchFilter notch = new CNotchFilter(f, SampleRate);
        double average = 0, div = 0;
        for (int i = 0; i < n; i++) temp[i] = data[i];
        for (int i = 0; i < n; i++) temp[i] = notch.Filter(temp[i]);
        for (int i = n / 2 - n / 4; i < n / 2 + n / 4; i++) average += temp[i];
        average /= (n / 2);
        for (int i = n / 2 - n / 4; i < n / 2 + n / 4; i++) div += (temp[i] - average) * (temp[i] - average);
        return div;
    }
}
