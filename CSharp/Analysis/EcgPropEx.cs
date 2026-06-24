namespace EcgDiag;

/// <summary>
/// ECG Property extraction class
/// </summary>
public class ECGprop
{
    public MultiLead_ECG? m_classBeats;
    public MultiLead_Templates? m_classTemplates;

    public short m_nDataChN, m_nSeconds;
    public short m_nSamplerate;
    public double m_fuVperbit;
    public short m_nPACpercent, m_nPVCpercent;

    public short[][]? m_pDataIn;
    public short m_nTemplMaxlen;
    public ECG_Parameters? m_pOutPut;
    public Template Temp = new();

    public int m_nBeats, m_nBeatsMax;
    public byte[]? Beats;   //心搏类型
    public byte[]? BeatAdd; //心搏是否参与叠加

    public VH_ECGparm? Parm;
    public VH_ECGlead[]? Lead;

    private short m_lenCorr;
    private short[]?[] m_corrTempl = new short[2][];
    private short[]? m_corrBeat;

    public ECGprop(short ChNumber, short[][] DataIn, short Seconds, short Samplerate, double uVperbit)
    {
        m_nDataChN = ChNumber;
        m_nSeconds = Seconds;
        m_nSamplerate = Samplerate;
        m_fuVperbit = uVperbit;
        m_nPACpercent = 10;
        m_nPVCpercent = 10;
        m_pDataIn = DataIn;
        m_nBeatsMax = 5 * Seconds;
        m_nTemplMaxlen = (short)(2 * Samplerate);

        // Create beat analysis
        m_classBeats = new MultiLead_ECG(ChNumber, DataIn, Seconds, Samplerate, uVperbit);
        m_pOutPut = m_classBeats.OutPut;

        // Initialize Parm and Lead
        Parm = new VH_ECGparm();
        Lead = new VH_ECGlead[EcgDiagConstants.VH_EcgMaxLeads];
        for (int i = 0; i < EcgDiagConstants.VH_EcgMaxLeads; i++)
            Lead[i] = new VH_ECGlead();

        Beats = new byte[m_nBeatsMax];
        BeatAdd = new byte[m_nBeatsMax];
    }

    public bool AutoProcess(short nTemplChN = 0, bool all = true)
    {
        if (m_classBeats == null || !m_classBeats.ProcessSucceed()) return false;

        InitTemplate(nTemplChN);
        BeatsClassify();
        CommParameters();
        LeadParameters();
        return true;
    }

    public void SetPrematurePpercent(short percentPAC, short percentPVC)
    {
        m_nPACpercent = percentPAC;
        m_nPVCpercent = percentPVC;
    }

    public short GetSampleRate() { return m_nSamplerate; }

    public void CommParameters()
    {
        if (Parm == null || m_classTemplates == null) return;
        Parm.HR = (short)m_classTemplates.HR();
        Parm.RR = (short)m_classTemplates.RR();
        Parm.QRS = (short)m_classTemplates.msInterval(CommonIntervalType.cQRSd);
        Parm.QT = (short)m_classTemplates.msInterval(CommonIntervalType.cQTd);
        Parm.PR = (short)m_classTemplates.msInterval(CommonIntervalType.cPRd);
        Parm.Pd = (short)m_classTemplates.msInterval(CommonIntervalType.cPd);
        Parm.QTC = (short)m_classTemplates.msQTc(Parm.QT);
        Parm.axisP = (short)m_classTemplates.Axis(WaveType.Pwave);
        Parm.axisQRS = (short)m_classTemplates.Axis(WaveType.QRScomplex);
        Parm.axisT = (short)m_classTemplates.Axis(WaveType.Twave);
        Parm.uvRV5 = (short)m_classTemplates.uvRV5();
        Parm.uvRV6 = (short)m_classTemplates.uvRV6();
        Parm.uvSV1 = (short)m_classTemplates.uvSV1();
        Parm.uvSV2 = (short)m_classTemplates.uvSV2();
        Parm.WPW = m_classTemplates.WPW();
    }

    public void LeadParameters()
    {
        // Fill in individual lead parameters from templates
    }

    public bool BeatsClassify()
    {
        if (m_pOutPut == null) return false;
        m_nBeats = m_pOutPut.BeatsNum;
        if (Beats == null || Beats.Length < m_nBeats)
            Beats = new byte[m_nBeats];
        for (int i = 0; i < m_nBeats; i++)
            Beats[i] = (byte)'N';
        return true;
    }

    public bool IsPacedECG()
    {
        if (m_pOutPut == null) return false;
        return m_pOutPut.PaceMaker == 'A' || m_pOutPut.PaceMaker == 'V' || m_pOutPut.PaceMaker == 'B';
    }

    public bool TemplIsOk() { return m_classTemplates != null; }
    public short GetTemplChN() { return (short)(m_classTemplates?.ChNumber() ?? 0); }

    protected void InitTemplate(short nTemplChN)
    {
        if (m_pOutPut == null) return;
        // Initialize template analysis
        var temp = m_pOutPut.Temp;
        if (temp.Data != null && temp.Length > 0)
        {
            m_classTemplates = new MultiLead_Templates(temp, m_pOutPut);
            m_classTemplates.AutoAnalysis();
        }
    }

    // Access methods
    public bool isPositiveQRS(short lead) { return Lead != null && (Lead[lead].Qa + Lead[lead].Ra1 + Lead[lead].Ra2 + Lead[lead].Sa1 + Lead[lead].Sa2 >= 0); }
    public bool isNegativeQRS(short lead) { return Lead != null && (Lead[lead].Qa + Lead[lead].Ra1 + Lead[lead].Ra2 + Lead[lead].Sa1 + Lead[lead].Sa2 < 0); }
    public bool isPositiveP(short lead) { return Lead != null && Lead[lead].Pa1 > 0 && Lead[lead].Pa2 == 0; }
    public bool isNegativeP(short lead) { return Lead != null && Lead[lead].Pa1 < 0 && Lead[lead].Pa2 == 0; }
    public bool isDualP(short lead) { return Lead != null && Lead[lead].Pa1 * Lead[lead].Pa2 < 0; }
    public bool isPositiveT(short lead) { return Lead != null && Lead[lead].Ta1 > 0 && Lead[lead].Ta2 == 0; }
    public bool isNegativeT(short lead) { return Lead != null && Lead[lead].Ta1 < 0 && Lead[lead].Ta2 == 0; }
    public bool isDualT(short lead) { return Lead != null && Lead[lead].Ta1 * Lead[lead].Ta2 < 0; }

    public bool isFlatT(short lead)
    {
        if (Lead == null) return false;
        return Math.Min(Lead[lead].Ta1, Lead[lead].Ta2) >= 0 && Math.Max(Lead[lead].Ta1, Lead[lead].Ta2) < 100;
    }

    public bool isQS(short lead)
    {
        if (Lead == null) return false;
        byte m0 = Lead[lead].morpho[0], m1 = Lead[lead].morpho[1];
        return (char.ToUpper((char)m0) == 'Q' && char.ToUpper((char)m1) == 'S');
    }

    public bool isQr(short lead)
    {
        if (Lead == null) return false;
        return (char)Lead[lead].morpho[0] == 'Q' && (char)Lead[lead].morpho[1] == 'r';
    }

    public bool isrsR(short lead)
    {
        if (Lead == null) return false;
        return (char)Lead[lead].morpho[0] == 'r' && (char)Lead[lead].morpho[1] == 's' && (char)Lead[lead].morpho[2] == 'R';
    }

    public bool isrsr(short lead)
    {
        if (Lead == null) return false;
        return (char)Lead[lead].morpho[0] == 'r' && (char)Lead[lead].morpho[1] == 's' && (char)Lead[lead].morpho[2] == 'r';
    }

    public bool isRSrs(short lead)
    {
        if (Lead == null) return false;
        return (char)Lead[lead].morpho[0] == 'r' || (char)Lead[lead].morpho[0] == 'R';
    }

    public string QRSmorpho(short lead)
    {
        if (Lead == null) return "";
        return System.Text.Encoding.ASCII.GetString(Lead[lead].morpho).TrimEnd('\0');
    }

    public char GetPaceMaker()
    {
        if (m_pOutPut == null) return '\0';
        char pm = m_pOutPut.PaceMaker;
        if (pm == 'A' || pm == 'V' || pm == 'B') return pm;
        return '\0';
    }

    public short GetAflutAfib() { return m_pOutPut?.AflutAfib ?? 0; }

    // uV accessors
    public short uvPa1(short lead) { return Lead?[lead].Pa1 ?? 0; }
    public short uvPa2(short lead) { return Lead?[lead].Pa2 ?? 0; }
    public short uvQa(short lead)
    {
        if (Lead == null) return 0;
        if (isQS(lead)) return Math.Min(Lead[lead].Qa, Math.Min(Lead[lead].Sa1, Lead[lead].Sa2));
        return Lead[lead].Qa;
    }
    public short uvRa1(short lead) { return Lead?[lead].Ra1 ?? 0; }
    public short uvRa2(short lead) { return Lead?[lead].Ra2 ?? 0; }
    public short uvSa1(short lead) { return Lead?[lead].Sa1 ?? 0; }
    public short uvSa2(short lead) { return Lead?[lead].Sa2 ?? 0; }
    public short uvTa1(short lead) { return Lead?[lead].Ta1 ?? 0; }
    public short uvTa2(short lead) { return Lead?[lead].Ta2 ?? 0; }

    public short uvQRSa(short lead) { return (short)(Math.Max(uvRa1(lead), uvRa2(lead)) - Math.Min(uvQa(lead), Math.Min(uvSa1(lead), uvSa2(lead)))); }
    public short uvRa(short lead) { return Math.Max(uvRa1(lead), uvRa2(lead)); }
    public short uvSa(short lead) { return Math.Min(uvSa1(lead), uvSa2(lead)); }
    public short uvPa(short lead) { short p1 = uvPa1(lead), p2 = uvPa2(lead); return (Math.Abs(p1) >= Math.Abs(p2)) ? p1 : p2; }
    public short uvTa(short lead) { short t1 = uvTa1(lead), t2 = uvTa2(lead); return (Math.Abs(t1) >= Math.Abs(t2)) ? t1 : t2; }

    // ms accessors
    public short msPd(short lead) { return Lead?[lead].Pd ?? 0; }
    public short msQd(short lead)
    {
        if (Lead == null) return 0;
        if (isQS(lead)) return Math.Max(Lead[lead].Qd, Math.Max(Lead[lead].Sd1, Lead[lead].Sd2));
        return Lead[lead].Qd;
    }
    public short msRd1(short lead) { return Lead?[lead].Rd1 ?? 0; }
    public short msRd2(short lead) { return Lead?[lead].Rd2 ?? 0; }
    public short msSd1(short lead) { return Lead?[lead].Sd1 ?? 0; }
    public short msSd2(short lead) { return Lead?[lead].Sd2 ?? 0; }
    public short msTd(short lead) { return Lead?[lead].Td ?? 0; }
    public short msPR(short lead) { return Lead?[lead].PR ?? 0; }
    public short msQT(short lead) { return Lead?[lead].QT ?? 0; }
    public short msQRS(short lead) { return Lead?[lead].QRS ?? 0; }
    public short msSd(short lead) { return (short)(msSd1(lead) + msSd2(lead)); }
    public short msRd(short lead) { return (short)(msRd1(lead) + msRd2(lead)); }

    // ST accessors
    public short STj(short lead) { return Lead?[lead].ST[0] ?? 0; }
    public short ST1(short lead) { return Lead?[lead].ST[1] ?? 0; }
    public short ST2(short lead) { return Lead?[lead].ST[2] ?? 0; }
    public short ST3(short lead) { return Lead?[lead].ST[3] ?? 0; }
    public short ST20(short lead) { return Lead?[lead].ST[4] ?? 0; }
    public short ST40(short lead) { return Lead?[lead].ST[5] ?? 0; }
    public short ST60(short lead) { return Lead?[lead].ST[6] ?? 0; }
    public short ST80(short lead) { return Lead?[lead].ST[7] ?? 0; }
    public short Rnotch(short lead) { return Lead?[lead].Rnotch ?? 0; }

    // Derived
    public short positivePa(short lead) { return Math.Max(Math.Max(uvPa1(lead), uvPa2(lead)), (short)0); }
    public short negativePa(short lead) { return Math.Min(Math.Min(uvPa1(lead), uvPa2(lead)), (short)0); }
    public short positiveTa(short lead) { return Math.Max(Math.Max(uvTa1(lead), uvTa2(lead)), (short)0); }
    public short negativeTa(short lead) { return Math.Min(Math.Min(uvTa1(lead), uvTa2(lead)), (short)0); }

    public short positivePd(short lead)
    {
        if (uvPa1(lead) > 0 || uvPa2(lead) > 0) return msPd(lead);
        return 0;
    }
    public short negativePd(short lead)
    {
        if (uvPa1(lead) < 0 || uvPa2(lead) < 0) return msPd(lead);
        return 0;
    }
    public short positiveTd(short lead)
    {
        if (uvTa1(lead) > 0 || uvTa2(lead) > 0) return msTd(lead);
        return 0;
    }
    public short negativeTd(short lead)
    {
        if (uvTa1(lead) < 0 || uvTa2(lead) < 0) return msTd(lead);
        return 0;
    }

    // Common parameters
    public static short HRFromRR(short msRR) { return (short)(60 * 1000 / msRR); }
    public static short QTcCalc(short msQT, short msRR) { return (msRR > 0) ? (short)(msQT * Math.Sqrt(1000.0) / Math.Sqrt(msRR)) : (short)0; }

    public short GetHR() { return Parm?.HR ?? 0; }
    public short GetRR() { return Parm?.RR ?? 0; }
    public short Pd_ms() { return Parm?.Pd ?? 0; }
    public short GetPR() { return Parm?.PR ?? 0; }
    public short GetQRS() { return Parm?.QRS ?? 0; }
    public short GetQT() { return Parm?.QT ?? 0; }
    public short GetQTc() { return Parm?.QTC ?? 0; }
    public short QTdis() { return (Parm != null && Parm.QTdis >= 0) ? Parm.QTdis : (short)0; }
    public short QTmax() { return (Parm != null && Parm.QTmax > Parm.QTmin) ? Parm.QTmax : (short)0; }
    public short QTmin() { return (Parm != null && Parm.QTmax > Parm.QTmin) ? Parm.QTmin : (short)0; }
    public short QTmaxLead() { return (Parm != null && Parm.QTmaxLead >= 0) ? Parm.QTmaxLead : (short)-1; }
    public short QTminLead() { return (Parm != null && Parm.QTminLead >= 0) ? Parm.QTminLead : (short)-1; }
    public short Paxis() { return Parm?.axisP ?? 0; }
    public short QRSaxis() { return Parm?.axisQRS ?? 0; }
    public short Taxis() { return Parm?.axisT ?? 0; }
    public char WPW() { return Parm?.WPW ?? '\0'; }

    // Beat parameters
    public short RR(int index)
    {
        if (m_pOutPut?.Beats == null || index <= 0) return 0;
        return (short)((m_pOutPut.Beats[index].Pos - m_pOutPut.Beats[index - 1].Pos) * 1000 / m_nSamplerate);
    }
    public short beatPnum(int index) { return m_pOutPut?.Beats?[index].Pnum ?? 0; }
}
