using ECGDiag.DataStructures;
using ECGDiag.Utilities;
using static ECGDiag.DataStructures.EcgDiagDefines;

namespace ECGDiag.Core;

/// <summary>
/// Mean calculation utility
/// </summary>
public class CMean
{
    private int _i;
    private double _m;

    public CMean()
    {
        Init();
    }

    public void Init()
    {
        _i = 0;
        _m = 0;
    }

    public double Mean(double x)
    {
        _i++;
        _m = x / _i + (_i - 1.0) / _i * _m;
        return _m;
    }
}

/// <summary>
/// Base class for ECG diagnostic coding
/// ECG诊断编码基类
/// </summary>
public class CEcgCodeBase
{
    // ECG data information
    protected short m_fs;              // Sample rate
    protected short m_chnum;           // Number of channels
    protected short m_templpos;        // Template position
    protected short m_templen;         // Template length
    protected short[][]? m_templ;      // Template data
    protected short[][]? m_data;       // ECG data
    protected short m_seconds;         // Duration in seconds
    protected int m_length;            // Data length
    protected double m_uVpb;           // Microvolts per bit

    // Lead information
    protected VH_EcgLeadInfo[] m_infoLead = new VH_EcgLeadInfo[VH_EcgMaxLeads];

    // ECG parameters and analysis results
    protected VH_ECGparm? m_pEcgParm;
    protected VH_ECGlead[]? m_pEcgLead;
    protected VH_ECGbeat[]? m_pEcgBeat;
    protected VH_ECGinfo? m_pEcgInfo;
    protected string m_szLeadName = string.Empty;
    protected byte[]? m_pBeatsType;    // Beat types: N:normal, V:ventricular, B:border, P:paced

    // Patient information
    protected char m_sex = 'M';        // 'M' or 'F'
    protected short m_ageD, m_ageM, m_ageY = 35;

    // Statistical parameters
    protected double m_meanNN, m_SDNN;
    protected short JT, JTI, QT, QTI, QRS, HR;

    // Conversion constants
    protected short m_uv25, m_uv50, m_uv100, m_uv200, m_uv500;
    protected short m_ms20, m_ms120, m_ms180;

    public CEcgCodeBase()
    {
        m_fs = 0;
        m_chnum = 0;
        m_length = 0;
        m_seconds = 0;
        m_data = null;
        m_templpos = 0;
        m_templen = 0;
        m_templ = null;
        m_sex = 'M';
        m_ageD = 0;
        m_ageM = 0;
        m_ageY = 35;
        m_pEcgParm = null;
        m_pEcgLead = null;
        m_pEcgBeat = null;
        m_pEcgInfo = null;
        m_pBeatsType = null;
        m_meanNN = 0;
        m_SDNN = 0;

        // Initialize lead information from defaults
        for (short i = 0; i < VH_EcgMaxLeads; i++)
        {
            m_infoLead[i] = InitialEcgLeadInfo.Data[i];
        }
    }

    public void SetEcgDataInfo(short fs, short chnum, double uVpb)
    {
        m_fs = fs;
        m_chnum = chnum;
        m_uVpb = uVpb;

        m_uv25 = (short)(25 / m_uVpb + 0.5);
        m_uv50 = (short)(50 / m_uVpb + 0.5);
        m_uv100 = (short)(100 / m_uVpb + 0.5);
        m_uv200 = (short)(200 / m_uVpb + 0.5);
        m_uv500 = (short)(500 / m_uVpb + 0.5);

        m_ms20 = (short)(20 * m_fs / 1000);
        m_ms120 = (short)(120 * m_fs / 1000);
        m_ms180 = (short)(180 * m_fs / 1000);
    }

    public void SetEcgData(short[][] data, int length)
    {
        m_data = data;
        m_length = length;
        m_seconds = (short)(m_length / m_fs);
    }

    public void SetEcgTempl(short templpos, short templen, short[][] templ)
    {
        m_templpos = templpos;
        m_templen = templen;
        m_templ = templ;
    }

    public void SetEcgParm(VH_ECGparm pEcgParm)
    {
        m_pEcgParm = pEcgParm;
    }

    public void SetEcglead(VH_ECGlead[] pEcgLead)
    {
        m_pEcgLead = pEcgLead;
    }

    public void SetEcgInfo(VH_ECGinfo pEcgInfo, byte[] pBeatsType)
    {
        m_pEcgInfo = pEcgInfo;
        m_pEcgBeat = m_pEcgInfo.Beats;
        m_pBeatsType = pBeatsType;

        m_meanNN = 0;
        m_SDNN = 0;
        int n = m_pEcgInfo.BeatsNum;
        double NN = 0, preNN = 0;
        int j = 0;

        // Calculate mean NN interval
        for (int i = 1; i < n; i++)
        {
            if (m_pBeatsType[i] == (byte)'N' && m_pBeatsType[i - 1] == (byte)'N')
            {
                NN = m_pEcgBeat![i].Pos - m_pEcgBeat[i - 1].Pos;
                if (preNN > 0 && (NN >= 0.75 * preNN && NN <= 1.25 * preNN))
                {
                    j++;
                    m_meanNN = NN / j + m_meanNN * (j - 1.0) / j;
                }
                preNN = NN;
            }
        }

        // Calculate SDNN
        j = 0;
        preNN = 0;
        for (int i = 1; i < n; i++)
        {
            if (m_pBeatsType[i] == (byte)'N' && m_pBeatsType[i - 1] == (byte)'N')
            {
                NN = m_pEcgBeat![i].Pos - m_pEcgBeat[i - 1].Pos;
                if (preNN > 0 && (NN >= 0.75 * preNN && NN <= 1.25 * preNN))
                {
                    j++;
                    m_SDNN = Square(NN - m_meanNN) / j + m_SDNN * (j - 1.0) / j;
                }
                preNN = NN;
            }
        }

        if (j > n / 2)
            m_SDNN = Math.Sqrt(j * m_SDNN / (j - 1));
        else
            m_SDNN = -1;

        m_meanNN = 1000 * m_meanNN / m_fs;
        m_SDNN = 1000 * m_SDNN / m_fs;

        QT = msQT();
        QRS = msQRS();
        HR = bpmHR();
        JT = (short)(QT - QRS);
        QTI = (short)((HR + 100) * QT / 656);
        JTI = (short)((HR + 100) * JT / 518);
    }

    public void SetEcgLeadInfo(short leadidx, short chnidx)
    {
        m_infoLead[leadidx].Chn = chnidx;
    }

    /// <summary>
    /// Set patient information
    /// 设置病人信息
    /// </summary>
    /// <param name="sex">Patient sex: 'F' for female, other for male</param>
    /// <param name="age">Patient age</param>
    /// <param name="ageYmd">Age unit: 0=years, 1=months, 2=days</param>
    public void SetPatientInfo(char sex, short age, short ageYmd = 0)
    {
        m_sex = sex;
        if (ageYmd == 0)
        {
            m_ageY = age;
            m_ageM = 0;
            m_ageD = 0;
        }
        else if (ageYmd == 1)
        {
            m_ageY = 0;
            m_ageM = age;
            m_ageD = 0;
        }
        else
        {
            m_ageY = 0;
            m_ageM = 0;
            m_ageD = age;
        }
    }

    public void CheckQuality()
    {
        if (m_pEcgLead == null) return;

        for (short lead = 0; lead < VH_EcgMaxLeads; lead++)
        {
            CheckQuality(lead);
        }
    }

    protected short CheckQuality(short lead)
    {
        // Quality check implementation
        // Returns quality status
        return 0;
    }

    // Accessor methods
    public char GetWPW() => (char)(m_pEcgParm?.WPW ?? (byte)' ');
    public short GetBeatsNum() => m_pEcgInfo?.BeatsNum ?? 0;
    public VH_ECGbeat[]? GetBeats() => m_pEcgBeat;
    public short GetArate() => m_pEcgInfo?.Arate ?? 0;
    public short GetVrate() => m_pEcgInfo?.Vrate ?? 0;
    public short GetAflutAfib() => m_pEcgInfo?.AflutAfib ?? 0;

    public (byte Type, short SpikesN, int[]? SpikesPos) GetPaceMaker()
    {
        if (m_pEcgInfo != null)
        {
            return (m_pEcgInfo.PaceMaker, m_pEcgInfo.SpikesN, m_pEcgInfo.SpikesPos);
        }
        return ((byte)'N', 0, null);
    }

    public byte[]? GetBeatsType() => m_pBeatsType;

    // Protected inline helper methods
    protected string LeadNames(uint dwLeads)
    {
        m_szLeadName = string.Empty;
        for (short i = 0; i < VH_EcgMaxLeads; i++)
        {
            if ((dwLeads & m_infoLead[i].Mask) != 0)
            {
                if (m_szLeadName.Length > 0)
                    m_szLeadName += ",";
                m_szLeadName += m_infoLead[i].Name;
            }
        }
        return m_szLeadName;
    }

    protected short LeadIndexFromChn(short chn)
    {
        for (short i = 0; i < VH_EcgMaxLeads; i++)
        {
            if (chn == m_infoLead[i].Chn)
                return i;
        }
        return -1;
    }

    protected string LeadNameFromChn(short chn)
    {
        for (short i = 0; i < VH_EcgMaxLeads; i++)
        {
            if (chn == m_infoLead[i].Chn)
                return m_infoLead[i].Name;
        }
        return string.Empty;
    }

    protected short HRfromSamples(int samples) => (short)(60 * m_fs / samples);
    protected short msfromSamples(int samples) => (short)(1000 * samples / m_fs);

    protected int MaxnPos(short[] data, int n)
    {
        int pos = 0;
        for (int i = 1; i < n; i++)
        {
            if (data[i] > data[pos]) pos = i;
        }
        return pos;
    }

    protected int MinnPos(short[] data, int n)
    {
        int pos = 0;
        for (int i = 1; i < n; i++)
        {
            if (data[i] < data[pos]) pos = i;
        }
        return pos;
    }

    protected short PpValue(short[] data, int n)
    {
        int pmax = 0, pmin = 0;
        for (int i = 1; i < n; i++)
        {
            if (data[i] > data[pmax]) pmax = i;
            if (data[i] < data[pmin]) pmin = i;
        }
        return (short)(data[pmax] - data[pmin]);
    }

    // ECG parameter access methods
    protected short Pb() => m_pEcgParm?.OnOff[0] ?? 0;
    protected short Pb(short lead) => m_pEcgLead?[m_infoLead[lead].Chn].OnOff[0] ?? 0;
    protected short Pe() => m_pEcgParm?.OnOff[1] ?? 0;
    protected short Pe(short lead) => m_pEcgLead?[m_infoLead[lead].Chn].OnOff[1] ?? 0;
    protected short Qb() => m_pEcgParm?.OnOff[2] ?? 0;
    protected short Qb(short lead) => m_pEcgLead?[m_infoLead[lead].Chn].OnOff[2] ?? 0;
    protected short Se() => m_pEcgParm?.OnOff[3] ?? 0;
    protected short Se(short lead) => m_pEcgLead?[m_infoLead[lead].Chn].OnOff[3] ?? 0;
    protected short Tb() => m_pEcgParm?.OnOff[4] ?? 0;
    protected short Tb(short lead) => m_pEcgLead?[m_infoLead[lead].Chn].OnOff[4] ?? 0;
    protected short Te() => m_pEcgParm?.OnOff[5] ?? 0;
    protected short Te(short lead) => m_pEcgLead?[m_infoLead[lead].Chn].OnOff[5] ?? 0;
    protected short J() => m_pEcgParm?.OnOff[3] ?? 0;
    protected short J(short lead) => m_pEcgLead?[m_infoLead[lead].Chn].OnOff[3] ?? 0;

    protected short msRR() => m_pEcgParm?.RR ?? 0;
    protected short bpmHR() => m_pEcgParm?.HR ?? 0;
    protected short msPd() => m_pEcgParm?.Pd ?? 0;
    protected short msPR() => m_pEcgParm?.PR ?? 0;
    protected short msQRS() => m_pEcgParm?.QRS ?? 0;
    protected short msQT() => m_pEcgParm?.QT ?? 0;
    protected short msQTc() => m_pEcgParm?.QTC ?? 0;
    protected byte WPW() => m_pEcgParm?.WPW ?? 0;

    // Additional helper methods for lead-specific parameters
    protected short uvRV(short lead)
    {
        if (m_pEcgLead == null) return 0;
        return Math.Max(m_pEcgLead[m_infoLead[lead].Chn].Ra1,
                       m_pEcgLead[m_infoLead[lead].Chn].Ra2);
    }

    protected short uvSV(short lead)
    {
        if (m_pEcgLead == null) return 0;
        short sv = Math.Min(m_pEcgLead[m_infoLead[lead].Chn].Sa1,
                           m_pEcgLead[m_infoLead[lead].Chn].Sa2);
        if (uvRV(lead) <= 0)
            sv = Math.Min(sv, m_pEcgLead[m_infoLead[lead].Chn].Qa);
        return sv;
    }

    protected bool isQS(short lead)
    {
        if (m_pEcgLead == null) return false;
        short chn = m_infoLead[lead].Chn;
        return m_pEcgLead[chn].Qa < 0 &&
               m_pEcgLead[chn].Ra1 <= 0 &&
               m_pEcgLead[chn].Ra2 <= 0;
    }

    protected bool isR(short lead)
    {
        if (m_pEcgLead == null) return false;
        short chn = m_infoLead[lead].Chn;
        return (m_pEcgLead[chn].Ra1 > 0 || m_pEcgLead[chn].Ra2 > 0) &&
               m_pEcgLead[chn].Qa >= 0 &&
               (m_pEcgLead[chn].Sa1 >= 0 && m_pEcgLead[chn].Sa2 >= 0);
    }

    protected bool isPositiveP(short lead)
    {
        if (m_pEcgLead == null) return false;
        short chn = m_infoLead[lead].Chn;
        short pStatus = m_pEcgLead[chn].Pstatus;
        return pStatus == 1 || pStatus == 3;
    }

    protected bool isNegativeP(short lead)
    {
        if (m_pEcgLead == null) return false;
        short chn = m_infoLead[lead].Chn;
        short pStatus = m_pEcgLead[chn].Pstatus;
        return pStatus == 2 || pStatus == 4;
    }

    protected bool isPositiveT(short lead)
    {
        if (m_pEcgLead == null) return false;
        short chn = m_infoLead[lead].Chn;
        short tStatus = m_pEcgLead[chn].Tstatus;
        return tStatus == 1 || tStatus == 3;
    }

    protected bool isNegativeT(short lead)
    {
        if (m_pEcgLead == null) return false;
        short chn = m_infoLead[lead].Chn;
        short tStatus = m_pEcgLead[chn].Tstatus;
        return tStatus == 2 || tStatus == 4;
    }

    protected bool isDualT(short lead)
    {
        if (m_pEcgLead == null) return false;
        short chn = m_infoLead[lead].Chn;
        short tStatus = m_pEcgLead[chn].Tstatus;
        return tStatus == 3 || tStatus == 4;
    }

    protected bool isFlatT(short lead)
    {
        if (m_pEcgLead == null) return false;
        short chn = m_infoLead[lead].Chn;
        return m_pEcgLead[chn].Tstatus == 0;
    }
}
