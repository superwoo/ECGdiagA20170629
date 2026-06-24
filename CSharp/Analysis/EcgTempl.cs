namespace EcgDiag;

public enum WaveType { Pwave, QRScomplex, Twave }
public enum CommonIntervalType { cPd, cPRd, cQRSd, cQTd }
public enum IndividualIntervalType { Pd, Qd, Rd1, Rd2, Sd1, Sd2, Td, PRd, QTd, QRSd }
public enum IndividualValueType { Pa1, Pa2, Qa, Ra1, Ra2, Sa1, Sa2, Ta1, Ta2 }

public class QTdiscretion
{
    public short msQTd, msMax, msMin;
    public short MaxLeadNo, MinLeadNo;
}

/// <summary>
/// ECG Base class for template analysis
/// </summary>
public class ECG_Base
{
    protected int m_nSampleRate;
    protected double m_dUvperbit;
    protected int MinimumWave;
    protected int FlateAngle;

    protected ECG_Base(int SampleRate, double Uvperbit)
    {
        m_nSampleRate = SampleRate;
        m_dUvperbit = Uvperbit;
        MinimumWave = (int)(30 / Uvperbit);
        FlateAngle = 3;
    }

    protected int TurnPoint(short[] data, int l, int r, int sign)
    {
        int p = l;
        if (sign > 0) { for (int i = l + 1; i <= r; i++) { if (data[i] > data[p]) p = i; } }
        else { for (int i = l + 1; i <= r; i++) { if (data[i] < data[p]) p = i; } }
        return p;
    }

    protected int FlatestPoint(short[] data, int p)
    {
        return p;
    }

    protected bool Flat(short[] data, int p, int w, int Angle)
    {
        if (p - w < 0 || p + w >= data.Length) return false;
        int diff = Math.Abs(data[p + w] - data[p - w]);
        return diff < Angle * w;
    }

    protected void Average(short[] data, int len, short ms)
    {
        int w = ms * m_nSampleRate / 1000;
        if (w < 1) w = 1;
        int hw = w / 2;
        short[] temp = new short[len];
        for (int i = 0; i < len; i++)
        {
            int sum = 0, count = 0;
            for (int j = -hw; j <= hw; j++)
            {
                int idx = i + j;
                if (idx >= 0 && idx < len) { sum += data[idx]; count++; }
            }
            temp[i] = (short)(sum / count);
        }
        Array.Copy(temp, data, len);
    }
}

/// <summary>
/// QRS Complex analysis
/// </summary>
public class QRS_Complex : ECG_Base
{
    private short[]? VHs;
    private int length;
    public short[]? Data;
    public int OnSet, OffSet;
    public int[] Q = new int[4], R1 = new int[4], S1 = new int[4], R2 = new int[4], S2 = new int[4];

    private short bpHigh, bpLow;

    public QRS_Complex(int SampleRate, double Uvperbit) : base(SampleRate, Uvperbit) { }

    public void SetData(short[] TemplateData, int QRSstart, int QRSend)
    {
        Data = TemplateData;
        OnSet = QRSstart;
        OffSet = QRSend;
        length = QRSend - QRSstart;
    }

    public void Analysis()
    {
        if (Data == null || length <= 0) { OnSet = -2; return; }
        ComplexFeatures();
    }

    public void FeatureSet(short[] TemplateData, int start, int end)
    {
        SetData(TemplateData, start, end);
        ComplexFeatures();
    }

    private void ComplexFeatures()
    {
        // Reset features
        Array.Clear(Q); Array.Clear(R1); Array.Clear(S1); Array.Clear(R2); Array.Clear(S2);
    }

    public void SubSetting(int l, int p, int r, int[] QRSsub)
    {
        Array.Clear(QRSsub);
    }

    public bool Delta() { return false; }
    public bool Split() { return false; }
    public void Notch(ref short Incrs, ref short Decrs) { Incrs = 0; Decrs = 0; }
    public short IncreaseNotch(int Start, int End) { return 0; }
    public short DecreaseNotch(int Start, int End) { return 0; }
    public int GetLength() { return length; }
    public void CorrectDependsOnZero() { }
}

/// <summary>
/// P/T Wave analysis
/// </summary>
public class PT_Wave : ECG_Base
{
    private int length;
    private int minWave, Width;
    public short[]? Data;
    public int Status;
    public int OnSet, OffSet;
    public int OnePos, TwoPos;
    public ushort AbV;

    public PT_Wave(char WaveType, int SampleRate, double Uvperbit) : base(SampleRate, Uvperbit)
    {
        minWave = MinimumWave / 3;
        Width = 0;
    }

    public void SetData(short[] TemplateData, int Start, int End)
    {
        Data = TemplateData;
        OnSet = Start; OffSet = End;
        length = End - Start;
    }

    public void FeatureSet(short[] TemplateData, int start, int end)
    {
        SetData(TemplateData, start, end);
    }

    public void TwaveAnalysis()
    {
        if (Data == null || length <= 0) { Status = 0; return; }
        Status = 1;
    }

    public void PwaveAnalysis()
    {
        if (Data == null || length <= 0) { Status = 0; return; }
        Status = 1;
    }

    public int GetLength() { return length; }
}

/// <summary>
/// ECG Template for single lead
/// </summary>
public class ECG_Template : ECG_Base
{
    private int m_nLength;
    private byte[] Morpho = new byte[8];
    public short[]? TemplateData;
    public QRS_Complex QRS;
    public PT_Wave P;
    public PT_Wave T;
    public bool QRSok;
    public int QRSstart, QRSend;
    public int Pstart, Pend, Tstart, Tend;
    public short[] AvantOnOff = new short[6];
    public short[] OnOff = new short[6];

    public ECG_Template(short[] ECGdata, int Length, int SampleRate, double Uvperbit) : base(SampleRate, Uvperbit)
    {
        m_nLength = Length;
        TemplateData = new short[Length];
        Array.Copy(ECGdata, TemplateData, Length);
        QRS = new QRS_Complex(SampleRate, Uvperbit);
        P = new PT_Wave('P', SampleRate, Uvperbit);
        T = new PT_Wave('T', SampleRate, Uvperbit);
        QRSok = false;
    }

    public short CalculateZeroValue()
    {
        if (TemplateData == null || m_nLength == 0) return 0;
        long sum = 0;
        for (int i = 0; i < m_nLength; i++) sum += TemplateData[i];
        return (short)(sum / m_nLength);
    }

    public void QRS_Location(int Center)
    {
        QRSstart = Center - m_nSampleRate * 60 / 1000;
        QRSend = Center + m_nSampleRate * 60 / 1000;
        if (QRSstart < 0) QRSstart = 0;
        if (QRSend >= m_nLength) QRSend = m_nLength - 1;
        QRSok = true;
    }

    public void FeatureSet()
    {
        if (TemplateData == null) return;
        if (QRSok)
        {
            QRS.FeatureSet(TemplateData, QRSstart, QRSend);
        }
    }

    public int msInterval(IndividualIntervalType Type) { return 0; }
    public int uvValue(IndividualValueType Type) { return 0; }
    public int uvSTvalue(int MSor123) { return 0; }
    public float STslope(int msStep) { return 0; }
    public string QRSmorpho() { return System.Text.Encoding.ASCII.GetString(Morpho).TrimEnd('\0'); }
}

/// <summary>
/// Multi-lead template analysis
/// </summary>
public class MultiLead_Templates : ECG_Base
{
    private int m_nLength, m_nCenter;
    private int m_nLeft, m_nRight;
    private int ChN;
    private int msRR_field;
    public ECG_Template[]? Lead;
    public short[] OnOff = new short[6];

    private ECG_Parameters? Beats_OutPut;

    public MultiLead_Templates(short ChNumber, short[][] MultiLeadData, short Length, short SampleRate, double Uvperbit)
        : base(SampleRate, Uvperbit)
    {
        ChN = ChNumber;
        m_nLength = Length;
        m_nCenter = Length / 2;
        m_nLeft = 0; m_nRight = Length;
        msRR_field = 800;

        Lead = new ECG_Template[ChN];
        for (int i = 0; i < ChN; i++)
            Lead[i] = new ECG_Template(MultiLeadData[i], Length, SampleRate, Uvperbit);
    }

    public MultiLead_Templates(Template Temp, ECG_Parameters BeatsOutPut)
        : base(Temp.SampleRate, Temp.Uvperbit)
    {
        ChN = Temp.ChN;
        m_nLength = Temp.Length;
        m_nCenter = Temp.Pos;
        m_nLeft = Temp.Left; m_nRight = Temp.Right;
        msRR_field = BeatsOutPut.AverageRR;
        Beats_OutPut = BeatsOutPut;

        Lead = new ECG_Template[ChN];
        for (int i = 0; i < ChN; i++)
            Lead[i] = new ECG_Template(Temp.Data![i], m_nLength, Temp.SampleRate, Temp.Uvperbit);
    }

    public void InitLead() { }

    public void AutoAnalysis()
    {
        if (Lead == null) return;
        for (int i = 0; i < ChN; i++)
        {
            Lead[i].QRS_Location(m_nCenter);
            Lead[i].FeatureSet();
        }
    }

    public void IndividualManual() { }
    public void CommenManual() { }

    public int msInterval(CommonIntervalType Type) { return 0; }
    public int msQTc(int msQTd)
    {
        if (msRR_field > 0)
            return (int)(msQTd * Math.Sqrt(1000.0) / Math.Sqrt(msRR_field));
        return 0;
    }

    public int Axis(WaveType Type) { return 0; }
    public int uvRV5() { return 0; }
    public int uvRV6() { return 0; }
    public int uvSV1() { return 0; }
    public int uvSV2() { return 0; }
    public void QTdiscrete(QTdiscretion QTd) { QTd.msQTd = -1; }
    public char WPW() { return ' '; }
    public int RR() { return msRR_field; }
    public int HR() { return (msRR_field > 0) ? 60000 / msRR_field : 0; }
    public int ChNumber() { return ChN; }
    public int Length() { return m_nLength; }
    public int Center() { return m_nCenter; }

    private int Axis(double vI, double vIII)
    {
        if (Math.Abs(vI) < 1 && Math.Abs(vIII) < 1) return 0;
        double angle = Math.Atan2(2 * vIII / Math.Sqrt(3.0), vI - vIII / 3.0) * 180.0 / Math.PI;
        return (int)angle;
    }

    private int FindMaxPch(int ChFound) { return 0; }
    private int FindMaxTch(int Ch1, int Ch2) { return 0; }
    private void QRSTanalysis() { }
    private void SetOnOff(bool Auto) { }
    private void AflutAfibCorrect() { }
    private void P_Added_Depends() { }
    private void P_OnOff_Correct() { }
}

/// <summary>
/// DoubleSampleRate interpolation
/// </summary>
public class CDoubleSampleRate
{
    private int td;
    private int y0, y1, y2, y3;
    private double y1t, y2t, y3t;
    private double judge, judge1;
    private short[] ReturnValues = new short[2];

    public CDoubleSampleRate()
    {
        td = 2;
        ReturnValues[0] = ReturnValues[1] = 0;
        Init(1);
    }

    public void Init(double uVpb)
    {
        y0 = y1 = y2 = y3 = 0;
        y1t = y2t = y3t = 0;
        ReturnValues[0] = ReturnValues[1] = 0;
        judge = 15 / uVpb;
        judge1 = 240 / uVpb;
    }

    private void CalculateReturnValues(short CurrentValue)
    {
        double y, temp;
        y3 = CurrentValue;
        temp = (double)(y3 - y1) / (td + td);
        y3t = 12.0 * (y1 - y2) / (td * td * td) + 6.0 * (y1t + temp) / (td * td);
        y2t = -6.0 * (y1 - y2) / (td * td) - 2.0 * (2 * y1t + temp) / td;
        y = y1;

        short dy1 = (short)Math.Abs(y1 - y0);
        short dy2 = (short)Math.Abs(y2 - y1);
        short dy3 = (short)Math.Abs(y3 - y2);

        if ((dy1 <= judge && dy3 <= judge && dy2 > judge1) ||
            (dy1 <= judge && dy2 <= judge && dy3 > judge1) ||
            (dy2 <= judge && dy3 <= judge && dy1 > judge1))
        {
            y2 = y3;
        }
        else
        {
            y += (y1t + y2t / 2 + y3t / 6);
        }
        y1t += (y2t + y3t / 2);
        y2t += y3t;
        ReturnValues[0] = (short)y;
        ReturnValues[1] = (short)y2;
        y0 = y1; y1 = y2; y2 = y3;
    }

    public short GetNumbers() { return 2; }

    public short[] GetDoubleSampleRateData(short xn)
    {
        CalculateReturnValues(xn);
        return ReturnValues;
    }
}

/// <summary>
/// Multi-channel double sample rate
/// </summary>
public class CMultiChannelDoubleSampleRate
{
    private short m_chnum;
    private short[][] ReturnValues;
    private short[][] YangReturnValues;
    private CDoubleSampleRate[] m_pDoubleSampleRate;

    public CMultiChannelDoubleSampleRate(short chnum)
    {
        m_chnum = chnum;
        m_pDoubleSampleRate = new CDoubleSampleRate[chnum];
        for (int i = 0; i < chnum; i++) m_pDoubleSampleRate[i] = new CDoubleSampleRate();
        ReturnValues = new short[chnum][];
        YangReturnValues = new short[2][];
        YangReturnValues[0] = new short[chnum];
        YangReturnValues[1] = new short[chnum];
    }

    public void Init(double uVpb)
    {
        for (short i = 0; i < m_chnum; i++) m_pDoubleSampleRate[i].Init(uVpb);
    }

    public short GetNumbers() { return 2; }

    public short[][] GetDoubleSampleRateData(short[] xn)
    {
        for (short i = 0; i < m_chnum; i++)
            ReturnValues[i] = m_pDoubleSampleRate[i].GetDoubleSampleRateData(xn[i]);
        return ReturnValues;
    }

    public short[][] GetData()
    {
        for (short i = 0; i < m_chnum; i++)
        {
            for (short j = 0; j < 2; j++)
                YangReturnValues[j][i] = ReturnValues[i][j];
        }
        return YangReturnValues;
    }
}
