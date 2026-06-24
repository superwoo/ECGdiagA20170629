namespace EcgDiag;

public class CMean
{
    private int i;
    private double m;

    public CMean() { Init(); }

    public void Init() { i = 0; m = 0; }

    public double Mean(double x)
    {
        i++;
        m = x / i + (i - 1.0) / i * m;
        return m;
    }
}

public class CEcgCodeBase
{
    public short m_fs, m_chnum;
    public short m_templpos, m_templen;
    public short[][]? m_templ;
    public short[][]? m_data;
    public short m_seconds;
    public int m_length;
    public double m_uVpb;
    public VH_EcgLeadInfo[] m_infoLead;
    public VH_ECGparm? m_pEcgParm;
    public VH_ECGlead[]? m_pEcgLead;
    public VH_ECGbeat[]? m_pEcgBeat;
    public VH_ECGinfo? m_pEcgInfo;
    public string m_szLeadName = "";
    public byte[]? m_pBeatsType; //心搏类型。目前只有N:正常，V:室性，B:边界无法判定，P:起搏
    public char m_sex;  //'M','F'
    public short m_ageD, m_ageM, m_ageY;
    public double m_meanNN, m_SDNN;
    public short JT, JTI, QT, QTI, QRS, HR;

    protected short m_uv25, m_uv50, m_uv100, m_uv200, m_uv500;
    protected short m_ms20, m_ms120, m_ms180;

    public CEcgCodeBase()
    {
        m_fs = 0; m_chnum = 0; m_length = 0; m_seconds = 0;
        m_data = null; m_templpos = 0; m_templen = 0; m_templ = null;
        m_sex = 'M'; m_ageD = 0; m_ageM = 0; m_ageY = 35;
        m_pEcgParm = null; m_pEcgLead = null; m_pEcgBeat = null; m_pEcgInfo = null;
        m_pBeatsType = null; m_meanNN = 0; m_SDNN = 0;
        m_infoLead = new VH_EcgLeadInfo[EcgDiagConstants.VH_EcgMaxLeads];
        for (short i = 0; i < EcgDiagConstants.VH_EcgMaxLeads; i++)
            m_infoLead[i] = EcgDiagConstants.InitialEcgLeadInfo[i];
    }

    public void SetEcgDataInfo(short fs, short chnum, double uVpb)
    {
        m_fs = fs; m_chnum = chnum; m_uVpb = uVpb;
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
        m_data = data; m_length = length;
        m_seconds = (short)(m_length / m_fs);
    }

    public void SetEcgTempl(short templpos, short templen, short[][] templ)
    {
        m_templpos = templpos; m_templen = templen; m_templ = templ;
    }

    public void SetEcgParm(VH_ECGparm pEcgParm) { m_pEcgParm = pEcgParm; }
    public void SetEcglead(VH_ECGlead[] pEcgLead) { m_pEcgLead = pEcgLead; }

    public void SetEcgInfo(VH_ECGinfo pEcgInfo, byte[] pBeatsType)
    {
        m_pEcgInfo = pEcgInfo;
        m_pEcgBeat = m_pEcgInfo.Beats;
        m_pBeatsType = pBeatsType;

        m_meanNN = 0; m_SDNN = 0;
        int j = 0, n = m_pEcgInfo.BeatsNum;
        double NN = 0, preNN = 0;
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
        j = 0; preNN = 0;
        for (int i = 1; i < n; i++)
        {
            if (m_pBeatsType[i] == (byte)'N' && m_pBeatsType[i - 1] == (byte)'N')
            {
                NN = m_pEcgBeat![i].Pos - m_pEcgBeat[i - 1].Pos;
                if (preNN > 0 && (NN >= 0.75 * preNN && NN <= 1.25 * preNN))
                {
                    j++;
                    m_SDNN = (NN - m_meanNN) * (NN - m_meanNN) / j + m_SDNN * (j - 1.0) / j;
                }
                preNN = NN;
            }
        }
        if (j > n / 2) m_SDNN = Math.Sqrt(j * m_SDNN / (j - 1)); else m_SDNN = -1;
        m_meanNN = 1000 * m_meanNN / m_fs;
        m_SDNN = 1000 * m_SDNN / m_fs;

        QT = msQT(); QRS = msQRS(); HR = bpmHR();
        JT = (short)(QT - QRS);
        QTI = (short)((HR + 100) * QT / 656);
        JTI = (short)((HR + 100) * JT / 518);
    }

    public void SetEcgLeadInfo(short leadidx, short chnidx) { m_infoLead[leadidx].Chn = chnidx; }

    public void SetPatientInfo(char sex, short age, short ageYmd = 0)
    {
        if (sex != 'M' && sex != 'F') sex = 'M';
        m_sex = sex;
        if (age < 0) { age = 35; ageYmd = 0; }
        switch (ageYmd)
        {
            case 1: case (short)'M': case (short)'m': m_ageD = (short)(age * 30); m_ageM = age; m_ageY = (short)(age / 12); break;
            case 2: case (short)'D': case (short)'d': m_ageD = age; m_ageM = (short)(age / 30); m_ageY = (short)(age / 365); break;
            default: m_ageD = (short)(age * 365); m_ageM = (short)(age / 12); m_ageY = age; break;
        }
    }

    public void CheckQuality()
    {
        for (short i = 0; i < EcgDiagConstants.VH_EcgMaxLeads; i++)
        {
            if (m_infoLead[i].Quality >= 0) continue;
            m_infoLead[i].Quality = CheckQualityLead(i);
        }
    }

    private short CheckQualityLead(short lead)
    {
        if (m_length <= 0) return -1;
        short ch = m_infoLead[lead].Chn;
        if (ch < 0 || ch >= m_chnum) return -2;
        short[] data = m_data![ch];
        int seconds = m_length / m_fs;
        int w = m_ms180, w1 = w / 2, length = m_length - w1, w0 = w / 10;
        if (w0 < 1) w0 = 1;
        int high = 0;
        for (int i = length / 4; i < length; i++)
        {
            if (data[i] < -m_uv100 || data[i] > m_uv100) high++;
        }
        if (high < length / 1000) return 3; //no signal
        if (lead >= (short)EcgLeadIndex.V1)
        {
            high = 0; int total = 0;
            short[] dataII = m_data[m_infoLead[(int)EcgLeadIndex.II].Chn];
            short[] dataIII = m_data[m_infoLead[(int)EcgLeadIndex.III].Chn];
            for (int i = length / 4; i < length; i++)
            {
                if (Math.Abs(dataII[i]) > m_uv50 && Math.Abs(dataIII[i]) > m_uv50)
                {
                    total++;
                    if (Math.Abs(data[i] - (dataII[i] + dataIII[i]) / 3) > m_uv25) high++;
                }
            }
            if (high < total / 20) return 3;
        }
        if (m_templ != null && m_templen > 0)
        {
            short[] tdata = m_templ[ch];
            int tlen = m_templen;
            high = 0;
            for (int i = 0; i < tlen; i++)
            {
                if (tdata[i] < -m_uv25 || tdata[i] > m_uv25) high++;
            }
            if (high < m_ms20) return 3;
            if (lead >= (short)EcgLeadIndex.V1)
            {
                high = 0;
                short[] dataII = m_data[m_infoLead[(int)EcgLeadIndex.II].Chn];
                short[] dataIII = m_data[m_infoLead[(int)EcgLeadIndex.III].Chn];
                for (int i = 0; i < tlen; i++)
                {
                    if (Math.Abs(tdata[i] - (dataII[i] + dataIII[i]) / 3) > m_uv25) high++;
                }
                if (high < m_ms20) return 3;
            }
        }
        else
        {
            int maxp0 = 0, maxp = 0, minp;
            int VF = 0;
            for (int i = length / 4; i < length; i++)
            {
                if (data[i] - data[i - w1] > m_uv200 && data[i] - data[i + w1] > m_uv200
                    && data[i] > data[i - w0] && data[i] > data[i + w0] && data[i] > m_uv50)
                {
                    if (maxp0 == 0) maxp0 = i;
                    else if (i > maxp0)
                    {
                        maxp = i;
                        int k = 16 * (maxp - maxp0);
                        if (k >= 2 * m_fs && k <= 8 * m_fs)
                        {
                            minp = maxp0;
                            for (int j2 = maxp0 + 1; j2 < maxp - 1; j2++)
                            {
                                if (data[j2] - data[j2 - w1] < -m_uv200 && data[j2] - data[j2 + w1] < -m_uv200
                                    && data[j2] < data[j2 - w0] && data[j2] < data[j2 + w0] && data[j2] < -m_uv50)
                                {
                                    minp = j2; break;
                                }
                            }
                            k = (maxp - maxp0) / 3;
                            if (minp - maxp0 > k && maxp - minp > k) VF++;
                        }
                        maxp0 = maxp;
                    }
                    i += w1;
                }
            }
            if (seconds > 0 && VF / seconds > 0)
            {
                high = 0; int total = 0;
                for (int i = length / 4; i < length; i++)
                {
                    if (Math.Abs(data[i]) > m_uv200)
                    {
                        total++;
                        if (Math.Abs(data[i] - data[i - 1]) > m_uv100) high++;
                    }
                }
                if (total < length / 20 || high < total / 20) return 2; //Vf
                else return 1; //VF
            }
        }
        return -1;
    }

    // Public property accessors
    public char GetWPW() { return (m_pEcgParm != null) ? m_pEcgParm.WPW : ' '; }
    public short GetBeatsNum() { return (m_pEcgInfo != null) ? m_pEcgInfo.BeatsNum : (short)0; }
    public VH_ECGbeat[]? GetBeats() { return m_pEcgBeat; }
    public short GetArate() { return (m_pEcgInfo != null) ? m_pEcgInfo.Arate : (short)0; }
    public short GetVrate() { return (m_pEcgInfo != null) ? m_pEcgInfo.Vrate : (short)0; }
    public short GetAflutAfib() { return (m_pEcgInfo != null) ? m_pEcgInfo.AflutAfib : (short)0; }

    public int[]? GetPaceMaker(out char Type, out short SpikesN)
    {
        if (m_pEcgInfo != null)
        {
            Type = m_pEcgInfo.PaceMaker;
            SpikesN = m_pEcgInfo.SpikesN;
            return m_pEcgInfo.SpikesPos;
        }
        Type = 'N'; SpikesN = 0;
        return null;
    }

    public byte[]? GetBeatsType() { return m_pBeatsType; }

    // Protected inline method equivalents
    protected string LeadNames(uint dwLeads)
    {
        m_szLeadName = "";
        for (short i = 0; i < EcgDiagConstants.VH_EcgMaxLeads; i++)
        {
            if ((dwLeads & m_infoLead[i].Mask) != 0)
            {
                if (m_szLeadName.Length > 0) m_szLeadName += ",";
                m_szLeadName += m_infoLead[i].Name;
            }
        }
        return m_szLeadName;
    }

    protected short LeadIndexFromChn(short chn)
    {
        for (short i = 0; i < EcgDiagConstants.VH_EcgMaxLeads; i++)
        {
            if (chn == m_infoLead[i].Chn) return i;
        }
        return -1;
    }

    protected string LeadNameFromChn(short chn)
    {
        for (short i = 0; i < EcgDiagConstants.VH_EcgMaxLeads; i++)
        {
            if (chn == m_infoLead[i].Chn) return m_infoLead[i].Name;
        }
        return "";
    }

    protected short HRfromSamples(int samples) { return (short)(60 * m_fs / samples); }
    protected short msfromSamples(int samples) { return (short)(1000 * samples / m_fs); }

    protected int maxnpos(short[] data, int n)
    {
        int pos = 0;
        for (int i = 1; i < n; i++) { if (data[i] > data[pos]) pos = i; }
        return pos;
    }

    protected int minnpos(short[] data, int n)
    {
        int pos = 0;
        for (int i = 1; i < n; i++) { if (data[i] < data[pos]) pos = i; }
        return pos;
    }

    protected short ppvalue(short[] data, int n)
    {
        int pmax = 0, pmin = 0;
        for (int i = 1; i < n; i++)
        {
            if (data[i] > data[pmax]) pmax = i;
            if (data[i] < data[pmin]) pmin = i;
        }
        return (short)(data[pmax] - data[pmin]);
    }

    // Feature point accessors
    protected short Pb() { return (m_pEcgParm != null) ? m_pEcgParm.OnOff[0] : (short)0; }
    protected short Pb(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].OnOff[0] : (short)0; }
    protected short Pe() { return (m_pEcgParm != null) ? m_pEcgParm.OnOff[1] : (short)0; }
    protected short Pe(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].OnOff[1] : (short)0; }
    protected short Qb() { return (m_pEcgParm != null) ? m_pEcgParm.OnOff[2] : (short)0; }
    protected short Qb(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].OnOff[2] : (short)0; }
    protected short Se() { return (m_pEcgParm != null) ? m_pEcgParm.OnOff[3] : (short)0; }
    protected short Se(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].OnOff[3] : (short)0; }
    protected short Tb() { return (m_pEcgParm != null) ? m_pEcgParm.OnOff[4] : (short)0; }
    protected short Tb(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].OnOff[4] : (short)0; }
    protected short Te() { return (m_pEcgParm != null) ? m_pEcgParm.OnOff[5] : (short)0; }
    protected short Te(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].OnOff[5] : (short)0; }
    protected short J() { return (m_pEcgParm != null) ? m_pEcgParm.OnOff[3] : (short)0; }
    protected short J(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].OnOff[3] : (short)0; }

    // Common parameter accessors
    protected short msRR() { return (m_pEcgParm != null) ? m_pEcgParm.RR : (short)0; }
    protected short bpmHR() { return (m_pEcgParm != null) ? m_pEcgParm.HR : (short)0; }
    protected short msPd() { return (m_pEcgParm != null) ? m_pEcgParm.Pd : (short)0; }
    protected short msPR() { return (m_pEcgParm != null) ? m_pEcgParm.PR : (short)0; }
    protected short msQRS() { return (m_pEcgParm != null) ? m_pEcgParm.QRS : (short)0; }
    protected short msQT() { return (m_pEcgParm != null) ? m_pEcgParm.QT : (short)0; }
    protected short msQTc() { return (m_pEcgParm != null) ? m_pEcgParm.QTC : (short)0; }
    protected char WPW() { return (m_pEcgParm != null) ? m_pEcgParm.WPW : '\0'; }
    protected short msQTdis() { return (m_pEcgParm != null) ? m_pEcgParm.QTdis : (short)0; }
    protected short msQTmax() { return (m_pEcgParm != null) ? m_pEcgParm.QTmax : (short)0; }
    protected short msQTmin() { return (m_pEcgParm != null) ? m_pEcgParm.QTmin : (short)0; }
    protected string QTmaxLead() { return (m_pEcgParm != null) ? LeadNameFromChn(m_pEcgParm.QTmaxLead) : ""; }
    protected string QTminLead() { return (m_pEcgParm != null) ? LeadNameFromChn(m_pEcgParm.QTminLead) : ""; }
    protected short axisP() { return (m_pEcgParm != null) ? m_pEcgParm.axisP : (short)0; }
    protected short axisQRS() { return (m_pEcgParm != null) ? m_pEcgParm.axisQRS : (short)0; }
    protected short axisT() { return (m_pEcgParm != null) ? m_pEcgParm.axisT : (short)0; }

    protected short uvRV(short lead)
    {
        return (m_pEcgLead != null) ? Math.Max(m_pEcgLead[m_infoLead[lead].Chn].Ra1, m_pEcgLead[m_infoLead[lead].Chn].Ra2) : (short)0;
    }
    protected short uvSV(short lead)
    {
        short SV = 0;
        if (m_pEcgLead != null)
        {
            SV = Math.Min(m_pEcgLead[m_infoLead[lead].Chn].Sa1, m_pEcgLead[m_infoLead[lead].Chn].Sa2);
            if (uvRV(lead) <= 0) SV = Math.Min(SV, m_pEcgLead[m_infoLead[lead].Chn].Qa);
        }
        return SV;
    }
    protected short uvRV5() { return uvRV((short)EcgLeadIndex.V5); }
    protected short uvRV6() { return uvRV((short)EcgLeadIndex.V6); }
    protected short uvSV1() { return uvSV((short)EcgLeadIndex.V1); }
    protected short uvSV2() { return uvSV((short)EcgLeadIndex.V2); }
    protected short uvRV1() { return uvRV((short)EcgLeadIndex.V1); }
    protected short uvSV5() { return uvSV((short)EcgLeadIndex.V5); }

    // Individual lead parameter accessors
    protected short msP(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Pd : (short)0; }
    protected short msNegaP(short lead)
    {
        if (m_pEcgLead != null)
        {
            short P1 = Math.Abs(uvP1(lead));
            short P2 = Math.Abs(uvP2(lead));
            if (uvP1(lead) <= 0 && uvP2(lead) <= 0) return m_pEcgLead[m_infoLead[lead].Chn].Pd;
            else if (uvP1(lead) < 0) return (short)(m_pEcgLead[m_infoLead[lead].Chn].Pd * P1 / (P1 + P2));
            else if (uvP2(lead) < 0) return (short)(m_pEcgLead[m_infoLead[lead].Chn].Pd * P2 / (P1 + P2));
        }
        return 0;
    }

    protected short msQ(short lead)
    {
        if (m_pEcgLead != null)
        {
            short chn = m_infoLead[lead].Chn;
            if (isQS(lead)) return Math.Max(m_pEcgLead[chn].Qd, (short)(m_pEcgLead[chn].Sd1 + m_pEcgLead[chn].Sd2));
            else return m_pEcgLead[chn].Qd;
        }
        return 0;
    }

    protected short msQd(short lead)
    {
        if (m_pEcgLead != null)
        {
            short chn = m_infoLead[lead].Chn;
            if (lead != (short)EcgLeadIndex.aVR)
            {
                if (isQS(lead)) return Math.Max(m_pEcgLead[chn].Qd, (short)(m_pEcgLead[chn].Sd1 + m_pEcgLead[chn].Sd2));
                else return m_pEcgLead[chn].Qd;
            }
            else
            {
                short Qd = 0;
                if (uvR2(lead) > 0) Qd = msR1(lead);
                else { if (uvQ(lead) < 0) Qd = 0; else Qd = msR1(lead); }
                return Qd;
            }
        }
        return 0;
    }

    protected short msR1(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Rd1 : (short)0; }
    protected short msR2(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Rd2 : (short)0; }
    protected short msR(short lead) { return (m_pEcgLead != null) ? (short)(m_pEcgLead[m_infoLead[lead].Chn].Rd1 + m_pEcgLead[m_infoLead[lead].Chn].Rd2) : (short)0; }
    protected short msS1(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Sd1 : (short)0; }
    protected short msS2(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Sd2 : (short)0; }
    protected short msS(short lead)
    {
        if (m_pEcgLead != null)
        {
            short chn = m_infoLead[lead].Chn;
            if (isQS(lead)) return Math.Max(m_pEcgLead[chn].Qd, (short)(m_pEcgLead[chn].Sd1 + m_pEcgLead[chn].Sd2));
            else return (short)(m_pEcgLead[chn].Sd1 + m_pEcgLead[chn].Sd2);
        }
        return 0;
    }
    protected short msT(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Td : (short)0; }
    protected short msPR(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].PR : (short)0; }
    protected short msQT(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].QT : (short)0; }
    protected short msQRS(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].QRS : (short)0; }

    protected short msRpeak(short lead)
    {
        if (m_templ != null)
        {
            short qb = Qb(lead), se = (short)(Se(lead) - 2), rp = qb;
            short start = 0;
            if (se > qb)
            {
                short[] templ = m_templ[m_infoLead[lead].Chn];
                for (short i = (short)(qb + 2); i < se; i++)
                {
                    if (start != 0) { if (templ[i] > templ[rp]) rp = i; }
                    else { if ((templ[i] - templ[qb]) > m_uv50) start = (short)(i - 1); }
                }
            }
            return (start > 0 && rp > start) ? (short)(1000 * (rp - start) / m_fs) : (short)0;
        }
        return 0;
    }

    protected short msQRSjudge(uint dwLeads, short msjudge = 120)
    {
        short count = 0;
        if (m_pEcgLead != null)
        {
            for (short i = 0; i < EcgDiagConstants.VH_EcgMaxLeads; i++)
            {
                if (dwLeads != 0 && m_infoLead[i].Mask != 0)
                {
                    if (msQRS(i) >= msjudge) count++;
                }
            }
        }
        return count;
    }

    protected short msQRSjudge(short[] Lead, short n, short msjudge = 120)
    {
        short count = 0;
        if (m_pEcgLead != null)
        {
            for (short i = 0; i < n; i++) { if (msQRS(Lead[i]) >= msjudge) count++; }
        }
        return count;
    }

    protected short msPRjudge(short[] Lead, short n, short msjudge = 120)
    {
        short count = 0;
        if (m_pEcgLead != null)
        {
            for (short i = 0; i < n; i++) { if (msPR(Lead[i]) >= msjudge) count++; }
        }
        return count;
    }

    protected short uvP1(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Pa1 : (short)0; }
    protected short uvP2(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Pa2 : (short)0; }
    protected short uvP(short lead)
    {
        if (m_pEcgLead != null)
        {
            if (uvPosiP(lead) >= -uvNegaP(lead)) return uvPosiP(lead);
            else return uvNegaP(lead);
        }
        return 0;
    }
    protected short uvQ(short lead)
    {
        if (m_pEcgLead != null)
        {
            short chn = m_infoLead[lead].Chn;
            if (isQS(lead)) return Math.Min(m_pEcgLead[chn].Qa, Math.Min(m_pEcgLead[chn].Sa1, m_pEcgLead[chn].Sa2));
            else return m_pEcgLead[chn].Qa;
        }
        return 0;
    }
    protected short uvQd(short lead)
    {
        if (m_pEcgLead != null)
        {
            short chn = m_infoLead[lead].Chn;
            if (lead != (short)EcgLeadIndex.aVR)
            {
                if (isQS(lead)) return Math.Min(m_pEcgLead[chn].Qa, Math.Min(m_pEcgLead[chn].Sa1, m_pEcgLead[chn].Sa2));
                else return m_pEcgLead[chn].Qa;
            }
            else
            {
                short val = 0;
                if (uvR2(lead) > 0) val = (short)(-uvR1(lead));
                else { if (uvQ(lead) < 0) val = 0; else val = (short)(-uvR1(lead)); }
                return val;
            }
        }
        return 0;
    }
    protected short uvR1(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Ra1 : (short)0; }
    protected short uvR2(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Ra2 : (short)0; }
    protected short uvR(short lead) { return Math.Max(uvR1(lead), uvR2(lead)); }
    protected short uvRd(short lead)
    {
        if (lead != (short)EcgLeadIndex.aVR) return uvR(lead);
        else return (short)(-Math.Min(uvQ(lead), uvS(lead)));
    }
    protected short uvS1(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Sa1 : (short)0; }
    protected short uvS2(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Sa2 : (short)0; }
    protected short uvS(short lead)
    {
        if (m_pEcgLead != null)
        {
            short chn = m_infoLead[lead].Chn;
            if (isQS(lead)) return Math.Min(m_pEcgLead[chn].Qa, Math.Min(m_pEcgLead[chn].Sa1, m_pEcgLead[chn].Sa2));
            else return Math.Min(m_pEcgLead[chn].Sa1, m_pEcgLead[chn].Sa2);
        }
        return 0;
    }
    protected short uvT1(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Ta1 : (short)0; }
    protected short uvT2(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].Ta2 : (short)0; }
    protected short uvT(short lead)
    {
        if (m_pEcgLead != null)
        {
            if (uvPosiT(lead) >= -uvNegaT(lead)) return uvPosiT(lead);
            else return uvNegaT(lead);
        }
        return 0;
    }
    protected short uvTpp(short lead) { return (short)(uvPosiT(lead) - uvNegaT(lead)); }
    protected short uvQS(short lead)
    {
        if (m_pEcgLead != null)
        {
            short chn = m_infoLead[lead].Chn;
            return Math.Min(m_pEcgLead[chn].Qa, Math.Min(m_pEcgLead[chn].Sa1, m_pEcgLead[chn].Sa2));
        }
        return 0;
    }
    protected short uvST(short lead, short msJ = 0)
    {
        if (m_templ != null)
        {
            short i = m_infoLead[lead].Chn;
            short j = (short)(J(lead) + msJ * m_fs / 1000);
            if (j > 0 && j < m_templen)
            {
                short z = -1;
                if (Pe() > 0 && Qb() > Pe()) z = (short)((Pe() + Qb()) / 2);
                else if (Qb() > 0) z = (short)(Qb() - 20 * m_fs / 1000);
                if (z > 0) return (short)((m_templ[i][j] - m_templ[i][z]) * m_uVpb);
            }
        }
        return 0;
    }
    protected short uvSTj(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].ST[0] : (short)0; }
    protected short uvST1(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].ST[1] : (short)0; }
    protected short uvST2(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].ST[2] : (short)0; }
    protected short uvST3(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].ST[3] : (short)0; }
    protected short uvST20(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].ST[4] : (short)0; }
    protected short uvST40(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].ST[5] : (short)0; }
    protected short uvST60(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].ST[6] : (short)0; }
    protected short uvST80(short lead) { return (m_pEcgLead != null) ? m_pEcgLead[m_infoLead[lead].Chn].ST[7] : (short)0; }
    protected short uvSTe(short lead)
    {
        if (m_templ != null)
        {
            short i = m_infoLead[lead].Chn;
            short j = Tb(lead);
            if (j > 0 && j < m_templen)
            {
                short z = -1;
                if (Pe() > 0 && Qb() > Pe()) z = (short)((Pe() + Qb()) / 2);
                else if (Qb() > 0) z = (short)(Qb() - 20 * m_fs / 1000);
                if (z > 0) return (short)((m_templ[i][j] - m_templ[i][z]) * m_uVpb);
            }
        }
        return 0;
    }
    protected short uvNegaP(short lead) { short Pmin = Math.Min(uvP1(lead), uvP2(lead)); return (Pmin < 0) ? Pmin : (short)0; }
    protected short uvPosiP(short lead) { short Pmax = Math.Max(uvP1(lead), uvP2(lead)); return (Pmax > 0) ? Pmax : (short)0; }
    protected short uvNegaT(short lead) { short Tmin = Math.Min(uvT1(lead), uvT2(lead)); return (Tmin < 0) ? Tmin : (short)0; }
    protected short uvPosiT(short lead) { short Tmax = Math.Max(uvT1(lead), uvT2(lead)); return (Tmax > 0) ? Tmax : (short)0; }
    protected short uvQRSpp(short lead) { return (short)(uvR(lead) - Math.Min(uvQ(lead), uvS(lead))); }
    protected short uvQRSnet(short lead) { return (short)(uvR(lead) + Math.Min(uvQ(lead), uvS(lead))); }
    protected short bpmHR(short beat)
    {
        if (m_pEcgBeat == null || beat < 1 || beat >= GetBeatsNum()) return 0;
        return (short)(60 * (m_pEcgBeat[beat].Pos - m_pEcgBeat[beat - 1].Pos) / m_fs);
    }

    // Boolean tests
    protected bool isR(short lead)
    {
        if (m_pEcgLead != null)
        {
            if (uvR(lead) >= 100 && uvQ(lead) > -100 && uvS(lead) > -100) return true;
        }
        return false;
    }
    protected bool isQS(short lead)
    {
        if (m_pEcgLead != null)
        {
            short chn = m_infoLead[lead].Chn;
            short Ra = Math.Max(m_pEcgLead[chn].Ra1, m_pEcgLead[chn].Ra2);
            short Qa = m_pEcgLead[chn].Qa;
            short Sa = Math.Min(m_pEcgLead[chn].Sa1, m_pEcgLead[chn].Sa2);
            short msR1v = m_pEcgLead[chn].Rd1;
            short msR2v = m_pEcgLead[chn].Rd2;
            if (Ra >= 50) return false;
            if (Ra >= 25 && (msR1v >= 18 || msR2v >= 18)) return false;
            if (Qa <= -100 || Sa <= -100) return true;
        }
        return false;
    }
    protected bool isDownwardST(short lead)
    {
        if (!isUtypeST(lead))
        {
            if (m_pEcgLead != null) { if (uvST1(lead) - uvST2(lead) > 25) return true; }
        }
        return false;
    }
    protected bool isUpwardST(short lead)
    {
        if (m_pEcgLead != null) { if (uvST2(lead) - uvST1(lead) > 25) return true; }
        return false;
    }
    protected bool isHorizontalST(short lead)
    {
        if (!isUtypeST(lead))
        {
            if (m_pEcgLead != null) { if (Math.Abs(uvST2(lead) - uvST1(lead)) <= 25) return true; }
        }
        return false;
    }
    protected bool isUtypeST(short lead)
    {
        if (m_templ != null)
        {
            short i = m_infoLead[lead].Chn;
            short j = Se(lead), k = Tb(lead);
            if (k < j) k = (short)(j + 200 * m_fs / 1000);
            short b = (short)((j + k) / 2);
            if (m_templ[i][j] - m_templ[i][b] > 50 && m_templ[i][k] - m_templ[i][b] > 50) return true;
        }
        return false;
    }
    protected bool isNegativeT(short lead)
    {
        if (m_pEcgLead != null) { if (uvPosiT(lead) == 0 && uvNegaT(lead) < 0) return true; }
        return false;
    }
    protected bool isPositiveT(short lead)
    {
        if (m_pEcgLead != null) { if (uvNegaT(lead) == 0 && uvPosiT(lead) > 0) return true; }
        return false;
    }
    protected bool isDualT(short lead)
    {
        if (m_pEcgLead != null) { if (uvT1(lead) * uvT2(lead) < 0) return true; }
        return false;
    }
    protected bool isFlatT(short lead)
    {
        if (m_pEcgLead != null) { if (Math.Abs(uvT1(lead)) < 100 && Math.Abs(uvT2(lead)) < 100) return true; }
        return false;
    }
    protected bool isNegativeP(short lead)
    {
        if (m_pEcgLead != null) { if (uvPosiP(lead) == 0 && uvNegaP(lead) < 0) return true; }
        return false;
    }
    protected bool isPositiveP(short lead)
    {
        if (m_pEcgLead != null) { if (uvNegaP(lead) == 0 && uvPosiP(lead) > 0) return true; }
        return false;
    }
    protected bool notPositiveP(short lead)
    {
        if (m_pEcgLead != null) { if (uvPosiP(lead) == 0 && uvNegaP(lead) <= 0) return true; }
        return false;
    }
    protected bool isSinalP()
    {
        if (isPositiveP((short)EcgLeadIndex.II) && notPositiveP((short)EcgLeadIndex.aVR)) return true;
        return false;
    }
    protected bool noQ(short lead) { return (msQ(lead) > 0) ? false : true; }
    protected bool noS(short lead) { return (msS(lead) > 0) ? false : true; }
    protected bool noS1(short lead) { return (msS1(lead) > 0) ? false : true; }
    protected bool noS2(short lead) { return (msS2(lead) > 0) ? false : true; }
    protected float ratioRS(short lead)
    {
        if (m_pEcgLead != null)
        {
            short ch = m_infoLead[lead].Chn;
            if (ch < 0 || ch >= m_chnum) return -1;
            float R = uvR(lead), S = Math.Abs(uvS(lead));
            if (S > 0) return R / S;
        }
        return -1;
    }
}
