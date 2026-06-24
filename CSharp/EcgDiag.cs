namespace EcgDiag;

/// <summary>
/// Main ECG Diagnosis class (from vhEcgDiag.h/.cpp)
/// </summary>
public class CvhEcgDiag
{
    public enum Vindex
    {
        V1 = 0, V2, V3, V4, V5, V6, V3R, V4R, V5R, V7, V8, V9
    }

    protected short[] m_nVindex = new short[12]; //V1-V6,V3R-V5R,V7,V9
    protected VH_ECGparm? m_pEcgParm;
    protected VH_ECGlead[]? m_pEcgLead;
    protected VH_ECGbeat[]? m_pEcgBeat;
    protected VH_ECGinfo? m_pEcgInfo;
    protected string m_szLeadName = "";

    private ECGprop? g_pEcgProp;
    private CvhCode? g_vhCode;

    public CvhEcgDiag()
    {
        g_pEcgProp = null;
        g_vhCode = new CvhCode();
        for (int i = 0; i < 12; i++) m_nVindex[i] = (short)(i + 6);
        m_pEcgParm = new VH_ECGparm();
        m_pEcgLead = new VH_ECGlead[EcgDiagConstants.VH_EcgMaxLeads];
        for (int i = 0; i < EcgDiagConstants.VH_EcgMaxLeads; i++)
            m_pEcgLead[i] = new VH_ECGlead();
        m_pEcgInfo = new VH_ECGinfo();
        m_pEcgBeat = null;
    }

    public void SetVindex(Vindex index, short chn)
    {
        if (index < Vindex.V1 || index > Vindex.V9) return;
        if (chn < 6 || chn >= 18) chn = -1;
        m_nVindex[(int)index] = chn;
    }

    public bool CreateEcgDiag(short ChNumber, short[][] DataIn, short Seconds, short Samplerate, double uVperbit)
    {
        g_pEcgProp = new ECGprop(ChNumber, DataIn, Seconds, Samplerate, uVperbit);
        short[][]? data = g_pEcgProp.m_pDataIn;
        int length = Seconds * Samplerate;
        g_vhCode!.SetEcgDataInfo(Samplerate, ChNumber, uVperbit);
        g_vhCode.SetEcgData(data, length);
        if (g_pEcgProp.AutoProcess())
        {
            short templpos = g_pEcgProp.Temp.Pos;
            short templen = g_pEcgProp.Temp.Length;
            short[][]? templ = g_pEcgProp.Temp.Data;
            g_vhCode.SetEcgTempl(templpos, templen, templ);

            SetParameters();
            return true;
        }
        else
        {
            g_pEcgProp = null;
            return false;
        }
    }

    protected void SetParameters()
    {
        if (g_pEcgProp != null && m_pEcgLead != null && m_pEcgParm != null && m_pEcgInfo != null)
        {
            short templchn = g_pEcgProp.GetTemplChN();
            for (short i = 0; i < templchn; i++)
            {
                if (g_pEcgProp.Lead == null) break;
                for (short j = 0; j < 6; j++) m_pEcgLead[i].OnOff[j] = g_pEcgProp.Lead[i].OnOff[j];
                m_pEcgLead[i].Pstatus = g_pEcgProp.Lead[i].Pstatus;
                m_pEcgLead[i].Tstatus = g_pEcgProp.Lead[i].Tstatus;
                m_pEcgLead[i].Pd = g_pEcgProp.Lead[i].Pd;
                m_pEcgLead[i].Qd = g_pEcgProp.Lead[i].Qd;
                m_pEcgLead[i].Rd1 = g_pEcgProp.Lead[i].Rd1;
                m_pEcgLead[i].Rd2 = g_pEcgProp.Lead[i].Rd2;
                m_pEcgLead[i].Sd1 = g_pEcgProp.Lead[i].Sd1;
                m_pEcgLead[i].Sd2 = g_pEcgProp.Lead[i].Sd2;
                m_pEcgLead[i].Td = g_pEcgProp.Lead[i].Td;
                m_pEcgLead[i].PR = g_pEcgProp.Lead[i].PR;
                m_pEcgLead[i].QT = g_pEcgProp.Lead[i].QT;
                m_pEcgLead[i].QRS = g_pEcgProp.Lead[i].QRS;
                m_pEcgLead[i].Pa1 = g_pEcgProp.Lead[i].Pa1;
                m_pEcgLead[i].Pa2 = g_pEcgProp.Lead[i].Pa2;
                m_pEcgLead[i].Qa = g_pEcgProp.Lead[i].Qa;
                m_pEcgLead[i].Ra1 = g_pEcgProp.Lead[i].Ra1;
                m_pEcgLead[i].Ra2 = g_pEcgProp.Lead[i].Ra2;
                m_pEcgLead[i].Sa1 = g_pEcgProp.Lead[i].Sa1;
                m_pEcgLead[i].Sa2 = g_pEcgProp.Lead[i].Sa2;
                m_pEcgLead[i].Ta1 = g_pEcgProp.Lead[i].Ta1;
                m_pEcgLead[i].Ta2 = g_pEcgProp.Lead[i].Ta2;
                m_pEcgLead[i].Rnotch = g_pEcgProp.Lead[i].Rnotch;
                for (short j = 0; j < 8; j++) m_pEcgLead[i].ST[j] = g_pEcgProp.Lead[i].ST[j];
                for (short j = 0; j < 4; j++) m_pEcgLead[i].STslope[j] = g_pEcgProp.Lead[i].STslope[j];
                Array.Copy(g_pEcgProp.Lead[i].morpho, m_pEcgLead[i].morpho, 8);
            }
            g_vhCode?.SetEcglead(m_pEcgLead);

            m_pEcgParm.RR = g_pEcgProp.GetRR();
            m_pEcgParm.HR = g_pEcgProp.GetHR();
            m_pEcgParm.Pd = g_pEcgProp.Pd_ms();
            m_pEcgParm.PR = g_pEcgProp.GetPR();
            m_pEcgParm.QRS = g_pEcgProp.GetQRS();
            m_pEcgParm.QT = g_pEcgProp.GetQT();
            m_pEcgParm.QTC = g_pEcgProp.GetQTc();
            m_pEcgParm.WPW = g_pEcgProp.WPW();
            m_pEcgParm.QTdis = g_pEcgProp.QTdis();
            m_pEcgParm.QTmax = g_pEcgProp.QTmax();
            m_pEcgParm.QTmin = g_pEcgProp.QTmin();
            m_pEcgParm.QTmaxLead = g_pEcgProp.QTmaxLead();
            m_pEcgParm.QTminLead = g_pEcgProp.QTminLead();
            m_pEcgParm.axisP = g_pEcgProp.Paxis();
            m_pEcgParm.axisQRS = g_pEcgProp.QRSaxis();
            m_pEcgParm.axisT = g_pEcgProp.Taxis();
            m_pEcgParm.uvRV1 = RV1();
            m_pEcgParm.uvRV5 = RV5();
            m_pEcgParm.uvRV6 = RV6();
            m_pEcgParm.uvSV1 = SV1();
            m_pEcgParm.uvSV2 = SV2();
            m_pEcgParm.uvSV5 = SV5();
            g_vhCode?.SetEcgParm(m_pEcgParm);
            if (g_pEcgProp.Parm != null)
            {
                for (short i = 0; i < 6; i++) m_pEcgParm.OnOff[i] = g_pEcgProp.Parm.OnOff[i];
            }

            if (g_pEcgProp.m_pOutPut != null)
            {
                int BeatsNum = g_pEcgProp.m_pOutPut.BeatsNum;
                m_pEcgBeat = null;
                if (BeatsNum > 1)
                {
                    m_pEcgBeat = new VH_ECGbeat[BeatsNum];
                    m_pEcgInfo.Status = (g_pEcgProp.m_pOutPut.Status == (short)ProcStatus.PROC_OK);
                    m_pEcgInfo.AflutAfib = g_pEcgProp.m_pOutPut.AflutAfib;
                    m_pEcgInfo.LeadNo = g_pEcgProp.m_pOutPut.LeadNo;
                    m_pEcgInfo.SubLeadNo = g_pEcgProp.m_pOutPut.SubLeadNo;
                    m_pEcgInfo.Vrate = g_pEcgProp.m_pOutPut.Vrate;
                    m_pEcgInfo.Arate = g_pEcgProp.m_pOutPut.Arate;
                    m_pEcgInfo.BeatsNum = g_pEcgProp.m_pOutPut.BeatsNum;
                    for (short i = 0; i < BeatsNum; i++)
                    {
                        m_pEcgBeat[i] = new VH_ECGbeat();
                        if (g_pEcgProp.m_pOutPut.Beats != null)
                        {
                            m_pEcgBeat[i].Status = g_pEcgProp.m_pOutPut.Beats[i].Status;
                            m_pEcgBeat[i].QRSonset = g_pEcgProp.m_pOutPut.Beats[i].QRSonset;
                            m_pEcgBeat[i].Pos = g_pEcgProp.m_pOutPut.Beats[i].Pos;
                            m_pEcgBeat[i].QRSw = g_pEcgProp.m_pOutPut.Beats[i].QRSw;
                            m_pEcgBeat[i].PR = g_pEcgProp.m_pOutPut.Beats[i].PR;
                            m_pEcgBeat[i].QT = g_pEcgProp.m_pOutPut.Beats[i].QT;
                            m_pEcgBeat[i].Pdir = g_pEcgProp.m_pOutPut.Beats[i].Pdir;
                            m_pEcgBeat[i].QRSdir = g_pEcgProp.m_pOutPut.Beats[i].QRSdir;
                            m_pEcgBeat[i].Tdir = g_pEcgProp.m_pOutPut.Beats[i].Tdir;
                            m_pEcgBeat[i].Udir = g_pEcgProp.m_pOutPut.Beats[i].Udir;
                            m_pEcgBeat[i].Pnum = g_pEcgProp.m_pOutPut.Beats[i].Pnum;
                            m_pEcgBeat[i].SubQRSw = g_pEcgProp.m_pOutPut.Beats[i].SubQRSw;
                            m_pEcgBeat[i].SubQRSdir = g_pEcgProp.m_pOutPut.Beats[i].SubQRSdir;
                        }
                    }
                    m_pEcgInfo.Beats = m_pEcgBeat;
                    m_pEcgInfo.PaceMaker = g_pEcgProp.m_pOutPut.PaceMaker;
                    m_pEcgInfo.SpikesN = g_pEcgProp.m_pOutPut.SpikesN;
                    m_pEcgInfo.SpikesPos = g_pEcgProp.m_pOutPut.SpikesPos;
                    byte[]? pBeatsType = g_pEcgProp.Beats;
                    g_vhCode?.SetEcgInfo(m_pEcgInfo, pBeatsType);
                }
            }
        }
        else
        {
            g_vhCode?.SetEcglead(null);
            g_vhCode?.SetEcgParm(null);
            g_vhCode?.SetEcgInfo(null, null);
        }
    }

    public bool EcgCode(char bySex, short age, short ageYmd = 0)
    {
        g_vhCode!.SetPatientInfo(bySex, age, ageYmd);
        g_vhCode.code();
        return true;
    }

    public void SetPrematurePpercent(short percentPAC, short percentPVC)
    {
        if (g_pEcgProp != null) g_pEcgProp.SetPrematurePpercent(percentPAC, percentPVC);
    }

    public bool CommenManual(short[] OnOff)
    {
        if (g_pEcgProp == null || g_pEcgProp.m_classTemplates == null || g_pEcgProp.Parm == null) return false;

        for (short j = 0; j < 6; j++)
            g_pEcgProp.m_classTemplates.OnOff[j] = g_pEcgProp.Parm.OnOff[j] = OnOff[j];

        g_pEcgProp.m_classTemplates.CommenManual();
        g_pEcgProp.LeadParameters();
        g_pEcgProp.CommParameters();
        SetParameters();

        if (GetTemplChNumber() >= 12)
            g_vhCode?.code();

        return true;
    }

    public bool IndividualManual(short[][] OnOffs)
    {
        if (g_pEcgProp == null || g_pEcgProp.m_classTemplates == null || g_pEcgProp.Lead == null) return false;

        short n = GetTemplChNumber();
        for (short i = 0; i < n; i++)
        {
            for (short j = 0; j < 6; j++)
            {
                if (g_pEcgProp.m_classTemplates.Lead?[i] != null)
                    g_pEcgProp.m_classTemplates.Lead![i]!.OnOff[j] = g_pEcgProp.Lead[i].OnOff[j] = OnOffs[i][j];
            }
        }
        g_pEcgProp.m_classTemplates.IndividualManual();
        g_pEcgProp.LeadParameters();
        g_pEcgProp.CommParameters();
        SetParameters();

        if (GetTemplChNumber() >= 12)
            g_vhCode?.code();

        return true;
    }

    public bool ECGparmManual(VH_ECGparm parm)
    {
        if (g_pEcgProp == null || m_pEcgParm == null || g_pEcgProp.Parm == null) return false;

        // Copy parm to m_pEcgParm
        m_pEcgParm.RR = parm.RR;
        m_pEcgParm.HR = parm.HR;
        m_pEcgParm.Pd = parm.Pd;
        m_pEcgParm.PR = parm.PR;
        m_pEcgParm.QRS = parm.QRS;
        m_pEcgParm.QT = parm.QT;
        m_pEcgParm.QTC = parm.QTC;
        m_pEcgParm.WPW = parm.WPW;
        m_pEcgParm.QTdis = parm.QTdis;
        m_pEcgParm.QTmax = parm.QTmax;
        m_pEcgParm.QTmin = parm.QTmin;
        m_pEcgParm.QTmaxLead = parm.QTmaxLead;
        m_pEcgParm.QTminLead = parm.QTminLead;
        m_pEcgParm.axisP = parm.axisP;
        m_pEcgParm.axisQRS = parm.axisQRS;
        m_pEcgParm.axisT = parm.axisT;
        m_pEcgParm.uvRV5 = parm.uvRV5;
        m_pEcgParm.uvRV6 = parm.uvRV6;
        m_pEcgParm.uvSV1 = parm.uvSV1;
        m_pEcgParm.uvSV2 = parm.uvSV2;
        for (short i = 0; i < 6; i++) m_pEcgParm.OnOff[i] = parm.OnOff[i];
        m_pEcgParm.uvRV1 = parm.uvRV1;
        m_pEcgParm.uvSV5 = parm.uvSV5;

        // Sync back to ECGprop
        g_pEcgProp.Parm.RR = m_pEcgParm.RR;
        g_pEcgProp.Parm.HR = m_pEcgParm.HR;
        g_pEcgProp.Parm.Pd = m_pEcgParm.Pd;
        g_pEcgProp.Parm.PR = m_pEcgParm.PR;
        g_pEcgProp.Parm.QRS = m_pEcgParm.QRS;
        g_pEcgProp.Parm.QT = m_pEcgParm.QT;
        g_pEcgProp.Parm.QTC = m_pEcgParm.QTC;
        g_pEcgProp.Parm.WPW = m_pEcgParm.WPW;
        g_pEcgProp.Parm.QTdis = m_pEcgParm.QTdis;
        g_pEcgProp.Parm.QTmax = m_pEcgParm.QTmax;
        g_pEcgProp.Parm.QTmin = m_pEcgParm.QTmin;
        g_pEcgProp.Parm.QTmaxLead = m_pEcgParm.QTmaxLead;
        g_pEcgProp.Parm.QTminLead = m_pEcgParm.QTminLead;
        g_pEcgProp.Parm.axisP = m_pEcgParm.axisP;
        g_pEcgProp.Parm.axisQRS = m_pEcgParm.axisQRS;
        g_pEcgProp.Parm.axisT = m_pEcgParm.axisT;
        g_pEcgProp.Parm.uvRV5 = m_pEcgParm.uvRV5;
        g_pEcgProp.Parm.uvRV6 = m_pEcgParm.uvRV6;
        g_pEcgProp.Parm.uvSV1 = m_pEcgParm.uvSV1;
        g_pEcgProp.Parm.uvSV2 = m_pEcgParm.uvSV2;
        for (short i = 0; i < 6; i++) g_pEcgProp.Parm.OnOff[i] = m_pEcgParm.OnOff[i];
        g_pEcgProp.Parm.uvRV1 = m_pEcgParm.uvRV1;
        g_pEcgProp.Parm.uvSV5 = m_pEcgParm.uvSV5;

        if (GetTemplChNumber() >= 12)
            g_vhCode?.code();

        return true;
    }

    public void SomeManual(bool bOnOff, bool bOnOffs)
    {
        if (g_pEcgProp == null || g_pEcgProp.m_classTemplates == null) return;
        if (bOnOffs) g_pEcgProp.m_classTemplates.IndividualManual();
        if (bOnOff) g_pEcgProp.m_classTemplates.CommenManual();
        g_pEcgProp.LeadParameters();
        g_pEcgProp.CommParameters();
        SetParameters();

        if (GetTemplChNumber() >= 12)
            g_vhCode?.code();
    }

    // Code access methods
    public short GetFirstMcode(out string? szLeadName)
    {
        szLeadName = null;
        return g_vhCode?.mcCodeGetFirst(out szLeadName) ?? 0;
    }

    public short GetNextMcode(out string? szLeadName)
    {
        szLeadName = null;
        return g_vhCode?.mcCodeGetNext(out szLeadName) ?? 0;
    }

    public short GetFirstRcode(out string? szLeadName)
    {
        szLeadName = null;
        return g_vhCode?.vhCodeGetFirst(out szLeadName) ?? 0;
    }

    public short GetNextRcode(out string? szLeadName)
    {
        szLeadName = null;
        return g_vhCode?.vhCodeGetNext(out szLeadName) ?? 0;
    }

    public short GetMcCodeCount() { return g_vhCode?.mcCodeCount() ?? 0; }
    public short GetVhCodeCount() { return g_vhCode?.vhCodeCount() ?? 0; }

    public static string mcCode(short code) { return CvhCode.mcCodeString((ushort)code); }
    public static string vhCode(short code) { return CvhCode.vhCodeString((ushort)code); }

    public short GetCriticalValue() { return g_vhCode?.GetCriticalValue() ?? 0; }

    // Template
    public short GetTemplChNumber() { return (g_pEcgProp != null) ? g_pEcgProp.GetTemplChN() : (short)0; }
    public VH_ECGinfo? GetEcgInfo() { return m_pEcgInfo; }
    public VH_ECGbeat[]? GetEcgBeats() { return m_pEcgBeat; }
    public short GetTemplLength() { return (g_pEcgProp != null) ? g_pEcgProp.Temp.Length : (short)0; }
    public short[][]? GetTemplData() { return (g_pEcgProp != null) ? g_pEcgProp.Temp.Data : null; }
    public short GetTemplPos() { return (g_pEcgProp != null) ? g_pEcgProp.Temp.Pos : (short)0; }
    public int GetBeatNum() { return (g_pEcgProp != null) ? g_pEcgProp.m_nBeats : 0; }

    public byte[]? GetBeats(out int BeatNum)
    {
        BeatNum = (g_pEcgProp != null) ? g_pEcgProp.m_nBeats : 0;
        return g_pEcgProp?.Beats;
    }

    public byte[]? GetBeatAdd(out int BeatNum)
    {
        BeatNum = (g_pEcgProp != null) ? g_pEcgProp.m_nBeats : 0;
        return g_pEcgProp?.BeatAdd;
    }

    public VH_ECGparm? GetECGparm() { return m_pEcgParm; }
    public VH_ECGlead[]? GetECGlead() { return m_pEcgLead; }

    public bool IsPacedECG() { return (g_pEcgProp != null) && g_pEcgProp.IsPacedECG(); }
    public bool TemplIsOk() { return (g_pEcgProp != null) && g_pEcgProp.TemplIsOk(); }

    public int uvSTvalue(short lead, int MSor123)
    {
        if (g_pEcgProp?.m_classTemplates?.Lead?[lead] != null)
            return g_pEcgProp.m_classTemplates.Lead![lead]!.uvSTvalue(MSor123);
        return 0;
    }

    public float STslope(short lead, int msStep)
    {
        if (g_pEcgProp?.m_classTemplates?.Lead?[lead] != null)
            return g_pEcgProp.m_classTemplates.Lead![lead]!.STslope(msStep);
        return 0;
    }

    public int Samples2ms(int Samples) { return (g_pEcgProp != null) ? Samples * 1000 / g_pEcgProp.GetSampleRate() : 0; }
    public int ms2Samples(int ms) { return (g_pEcgProp != null) ? ms * g_pEcgProp.GetSampleRate() / 1000 : 0; }

    // Boolean lead property checks
    public bool isPositiveQRS(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isPositiveQRS(lead); }
    public bool isNegativeQRS(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isNegativeQRS(lead); }
    public bool isPositiveP(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isPositiveP(lead); }
    public bool isNegativeP(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isNegativeP(lead); }
    public bool isDualP(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isDualP(lead); }
    public bool isPositiveT(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isPositiveT(lead); }
    public bool isNegativeT(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isNegativeT(lead); }
    public bool isDualT(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isDualT(lead); }
    public bool isFlatT(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isFlatT(lead); }
    public bool isQS(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isQS(lead); }
    public bool isQr(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isQr(lead); }
    public bool isrsR(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isrsR(lead); }
    public bool isrsr(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isrsr(lead); }
    public bool isRSrs(short lead) { return (g_pEcgProp != null) && g_pEcgProp.isRSrs(lead); }

    // Individual lead parameters
    public string? QRSmorpho(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.QRSmorpho(lead) : null; }
    public char GetPaceMaker() { return (g_pEcgProp != null) ? g_pEcgProp.GetPaceMaker() : ' '; }
    public short GetAflutAfib() { return (g_pEcgProp != null) ? g_pEcgProp.GetAflutAfib() : (short)0; }

    // uV accessors
    public short uvPa1(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvPa1(lead) : (short)0; }
    public short uvPa2(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvPa2(lead) : (short)0; }
    public short uvQa(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvQa(lead) : (short)0; }
    public short uvRa1(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvRa1(lead) : (short)0; }
    public short uvRa2(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvRa2(lead) : (short)0; }
    public short uvSa1(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvSa1(lead) : (short)0; }
    public short uvSa2(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvSa2(lead) : (short)0; }
    public short uvTa1(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvTa1(lead) : (short)0; }
    public short uvTa2(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvTa2(lead) : (short)0; }
    public short uvQRSa(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvQRSa(lead) : (short)0; }
    public short uvRa(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvRa(lead) : (short)0; }
    public short uvSa(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.uvSa(lead) : (short)0; }

    // ms accessors
    public short msPd(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msPd(lead) : (short)0; }
    public short msQd(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msQd(lead) : (short)0; }
    public short msRd1(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msRd1(lead) : (short)0; }
    public short msRd2(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msRd2(lead) : (short)0; }
    public short msSd1(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msSd1(lead) : (short)0; }
    public short msSd2(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msSd2(lead) : (short)0; }
    public short msTd(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msTd(lead) : (short)0; }
    public short msPR(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msPR(lead) : (short)0; }
    public short msQT(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msQT(lead) : (short)0; }
    public short msQRS(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msQRS(lead) : (short)0; }
    public short msSd(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msSd(lead) : (short)0; }
    public short msRd(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.msRd(lead) : (short)0; }

    // ST accessors
    public short STj(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.STj(lead) : (short)0; }
    public short ST1(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.ST1(lead) : (short)0; }
    public short ST2(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.ST2(lead) : (short)0; }
    public short ST3(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.ST3(lead) : (short)0; }
    public short ST20(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.ST20(lead) : (short)0; }
    public short ST40(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.ST40(lead) : (short)0; }
    public short ST60(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.ST60(lead) : (short)0; }
    public short ST80(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.ST80(lead) : (short)0; }
    public short Rnotch(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.Rnotch(lead) : (short)0; }

    // Derived
    public short positivePa(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.positivePa(lead) : (short)0; }
    public short negativePa(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.negativePa(lead) : (short)0; }
    public short positiveTa(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.positiveTa(lead) : (short)0; }
    public short negativeTa(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.negativeTa(lead) : (short)0; }
    public short positivePd(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.positivePd(lead) : (short)0; }
    public short negativePd(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.negativePd(lead) : (short)0; }
    public short positiveTd(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.positiveTd(lead) : (short)0; }
    public short negativeTd(short lead) { return (g_pEcgProp != null) ? g_pEcgProp.negativeTd(lead) : (short)0; }

    // Common parameters
    public short HR() { return (g_pEcgProp != null) ? g_pEcgProp.GetHR() : (short)0; }
    public short RR() { return (g_pEcgProp != null) ? g_pEcgProp.GetRR() : (short)0; }
    public short Pd() { return (g_pEcgProp != null) ? g_pEcgProp.Pd_ms() : (short)0; }
    public short PR() { return (g_pEcgProp != null) ? g_pEcgProp.GetPR() : (short)0; }
    public short QRS() { return (g_pEcgProp != null) ? g_pEcgProp.GetQRS() : (short)0; }
    public short QT() { return (g_pEcgProp != null) ? g_pEcgProp.GetQT() : (short)0; }
    public short QTc() { return (g_pEcgProp != null) ? g_pEcgProp.GetQTc() : (short)0; }
    public short QTdis() { return (g_pEcgProp != null) ? g_pEcgProp.QTdis() : (short)0; }
    public short QTmax() { return (g_pEcgProp != null) ? g_pEcgProp.QTmax() : (short)0; }
    public short QTmin() { return (g_pEcgProp != null) ? g_pEcgProp.QTmin() : (short)0; }
    public short QTmaxLead() { return (g_pEcgProp != null) ? g_pEcgProp.QTmaxLead() : (short)0; }
    public short QTminLead() { return (g_pEcgProp != null) ? g_pEcgProp.QTminLead() : (short)0; }

    // uV common
    public short RV5()
    {
        if (g_pEcgProp == null || m_nVindex[(int)Vindex.V5] < 0 || m_nVindex[(int)Vindex.V5] >= g_pEcgProp.GetTemplChN()) return 0;
        var lead = g_pEcgProp.Lead;
        if (lead == null) return 0;
        return Math.Max(lead[m_nVindex[(int)Vindex.V5]].Ra1, lead[m_nVindex[(int)Vindex.V5]].Ra2);
    }

    public short RV6()
    {
        if (g_pEcgProp == null || m_nVindex[(int)Vindex.V6] < 0 || m_nVindex[(int)Vindex.V5] >= g_pEcgProp.GetTemplChN()) return 0;
        var lead = g_pEcgProp.Lead;
        if (lead == null) return 0;
        return Math.Max(lead[m_nVindex[(int)Vindex.V6]].Ra1, lead[m_nVindex[(int)Vindex.V6]].Ra2);
    }

    public short SV1()
    {
        short L = m_nVindex[(int)Vindex.V1];
        if (g_pEcgProp == null || L < 0 || m_nVindex[(int)Vindex.V5] >= g_pEcgProp.GetTemplChN()) return 0;
        var lead = g_pEcgProp.Lead;
        if (lead == null) return 0;
        short uvSV = Math.Min(lead[L].Sa1, lead[L].Sa2);
        if (isQS(L)) uvSV = Math.Min(uvSV, lead[L].Qa);
        return uvSV;
    }

    public short SV2()
    {
        short L = m_nVindex[(int)Vindex.V2];
        if (g_pEcgProp == null || L < 0 || m_nVindex[(int)Vindex.V5] >= g_pEcgProp.GetTemplChN()) return 0;
        var lead = g_pEcgProp.Lead;
        if (lead == null) return 0;
        short uvSV = Math.Min(lead[L].Sa1, lead[L].Sa2);
        if (isQS(L)) uvSV = Math.Min(uvSV, lead[L].Qa);
        return uvSV;
    }

    public short SV5()
    {
        short L = m_nVindex[(int)Vindex.V5];
        if (g_pEcgProp == null || L < 0 || m_nVindex[(int)Vindex.V5] >= g_pEcgProp.GetTemplChN()) return 0;
        var lead = g_pEcgProp.Lead;
        if (lead == null) return 0;
        short uvSV = Math.Min(lead[L].Sa1, lead[L].Sa2);
        if (isQS(L)) uvSV = Math.Min(uvSV, lead[L].Qa);
        return uvSV;
    }

    public short SV6()
    {
        short L = m_nVindex[(int)Vindex.V6];
        if (g_pEcgProp == null || L < 0 || m_nVindex[(int)Vindex.V5] >= g_pEcgProp.GetTemplChN()) return 0;
        var lead = g_pEcgProp.Lead;
        if (lead == null) return 0;
        short uvSV = Math.Min(lead[L].Sa1, lead[L].Sa2);
        if (isQS(L)) uvSV = Math.Min(uvSV, lead[L].Qa);
        return uvSV;
    }

    public short RV1()
    {
        if (g_pEcgProp == null || m_nVindex[(int)Vindex.V1] < 0 || m_nVindex[(int)Vindex.V5] >= g_pEcgProp.GetTemplChN()) return 0;
        var lead = g_pEcgProp.Lead;
        if (lead == null) return 0;
        return Math.Max(lead[m_nVindex[(int)Vindex.V1]].Ra1, lead[m_nVindex[(int)Vindex.V1]].Ra2);
    }

    public short RV2()
    {
        if (g_pEcgProp == null || m_nVindex[(int)Vindex.V2] < 0 || m_nVindex[(int)Vindex.V5] >= g_pEcgProp.GetTemplChN()) return 0;
        var lead = g_pEcgProp.Lead;
        if (lead == null) return 0;
        return Math.Max(lead[m_nVindex[(int)Vindex.V2]].Ra1, lead[m_nVindex[(int)Vindex.V2]].Ra2);
    }

    // Axis
    public short Paxis() { return (g_pEcgProp != null) ? g_pEcgProp.Paxis() : (short)0; }
    public short QRSaxis() { return (g_pEcgProp != null) ? g_pEcgProp.QRSaxis() : (short)0; }
    public short Taxis() { return (g_pEcgProp != null) ? g_pEcgProp.Taxis() : (short)0; }
    public char WPW() { return (g_pEcgProp != null) ? g_pEcgProp.WPW() : ' '; }

    // Beat parameters
    public short RR(int index) { return (g_pEcgProp != null) ? g_pEcgProp.RR(index) : (short)0; }
    public short beatPnum(int index) { return (g_pEcgProp != null) ? g_pEcgProp.beatPnum(index) : (short)0; }

    // Static helpers
    public static short HRFromRR(short msRR) { return ECGprop.HRFromRR(msRR); }
    public static short QTcCalc(short msQT, short msRR) { return ECGprop.QTcCalc(msQT, msRR); }
}
