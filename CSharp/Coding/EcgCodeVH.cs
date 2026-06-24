namespace EcgDiag;

/// <summary>
/// VH Code analysis class - extends Minnesota Code
/// </summary>
public class CvhCode : CmcCode
{
    public struct STT_SCORE
    {
        public short score;
        public uint dwLeads;
    }

    protected STT_SCORE m_stQscore;
    protected STT_SCORE m_stSTDscore;
    protected STT_SCORE m_stSTEscore;
    protected STT_SCORE m_stTNscore;
    protected STT_SCORE m_stPLMIscore;
    protected bool m_bPolymorphicPVC;
    protected bool m_bAf;

    protected CodeMgr vh = new();
    protected Code cv_code = new();

    protected ushort vhcode(short g, short c, short i) { return (ushort)(g * 1000 + c * 100 + i); }

    public CvhCode() { }

    public ushort SetvhCode(ushort nCode, uint dwLeads = 0)
    {
        Code code = new() { nCode = nCode, nLeads = dwLeads, nClass = 0, nSort = 0 };
        vh.Add(code);
        return nCode;
    }

    public short vhCodeCount() { return (short)vh.GetCount(); }

    public short vhCodeGetFirst(string? szLeadName = null)
    {
        var p = vh.GetFirst();
        if (p == null) return -1;
        if (szLeadName != null) LeadNames(p.Value.nLeads);
        return (short)p.Value.nCode;
    }

    public short vhCodeGetNext(string? szLeadName = null)
    {
        var p = vh.GetNext();
        if (p == null) return -1;
        if (szLeadName != null) LeadNames(p.Value.nLeads);
        return (short)p.Value.nCode;
    }

    public ushort vhCodeFrommcCode(ushort mcCode, ushort vhCode, ref uint dwLeads)
    {
        var p = mc.Found(mcCode);
        if (p != null) { dwLeads = p.Value.nLeads; return SetvhCode(vhCode, dwLeads); }
        return 0;
    }

    public static string vhCodeString(ushort vhcode)
    {
        return vhcode.ToString();
    }

    public void SetCriticalValue(short cv, uint dwLeads = 0)
    {
        cv_code.nCode = (ushort)cv;
        cv_code.nLeads = dwLeads;
    }

    public short GetCriticalValue() { return (short)cv_code.nCode; }
    public short GetCriticalValue(out uint dwLeads) { dwLeads = cv_code.nLeads; return (short)cv_code.nCode; }

    // Scoring methods
    protected short Qscore(ref uint dwLeads) { dwLeads = m_stQscore.dwLeads; return m_stQscore.score; }
    protected short STDscore(ref uint dwLeads) { dwLeads = m_stSTDscore.dwLeads; return m_stSTDscore.score; }
    protected short STEscore(ref uint dwLeads) { dwLeads = m_stSTEscore.dwLeads; return m_stSTEscore.score; }
    protected short TNscore(ref uint dwLeads) { dwLeads = m_stTNscore.dwLeads; return m_stTNscore.score; }
    protected short plMIscore(ref uint dwLeads) { dwLeads = m_stPLMIscore.dwLeads; return m_stPLMIscore.score; }
    protected bool isPolymorphicPVC() { return m_bPolymorphicPVC; }

    // Main code entry point
    public void code()
    {
        mcCode();
        vhCode_internal();
    }

    public short vhCode()
    {
        return vhCodeCount();
    }

    private void vhCode_internal()
    {
        vh.Reset();
        m_stQscore = new STT_SCORE();
        m_stSTDscore = new STT_SCORE();
        m_stSTEscore = new STT_SCORE();
        m_stTNscore = new STT_SCORE();
        m_stPLMIscore = new STT_SCORE();
        m_bPolymorphicPVC = false;
        m_bAf = false;

        if (vhCode0()) return;
        vhCode1(); vhCode2(); vhCode3(); vhCode4();
        vhCode5(); vhCode6(); vhCode7(); vhCode8();
    }

    // Code 0 - Baseline ECG Suppression
    public bool vhCode0() { return false; }
    public short vhCode010() { return 0; }
    public short vhCode021() { return 0; }
    public short vhCode022() { return 0; }
    public short vhCode023() { return 0; }
    public short vhCode031() { return 0; }
    public short vhCode0321() { return 0; }
    public short vhCode0322() { return 0; }
    public short vhCode0323() { return 0; }
    public short vhCode033_4() { return 0; }
    public short vhCode033() { return 0; }
    public short vhCode034() { return 0; }
    public short vhCode035() { return 0; }
    public short vhCode040() { return 0; }
    public short vhCode050() { return 0; }
    public short vhCode061_2() { return 0; }

    // Code 1 - Rhythm
    public void vhCode1() { }
    public short vhCode100_4() { return 0; }
    public short vhCode101x() { return 0; }
    public short vhCode1020() { return 0; }
    public short vhCode103x() { return 0; }
    public short vhCode1040() { return 0; }
    public short vhCode1050() { return 0; }
    public short vhCode1060() { return 0; }
    public short vhCode110() { return 0; }
    public short vhCode12x() { return 0; }
    public short vhCode13x() { return 0; }
    public short vhCode134() { return 0; }
    public short vhCode14x() { return 0; }
    public short vhCode150() { return 0; }
    public short vhCode151() { return 0; }
    public short vhCode152() { return 0; }
    public short vhCode153() { return 0; }
    public short vhCode153x() { return 0; }
    public short vhCode154() { return 0; }
    public short vhCode16x() { return 0; }
    public short vhCode170_2() { return 0; }
    public short vhCode173_4() { return 0; }
    public short vhCode181() { return 0; }
    public short vhCode182() { return 0; }
    public short vhCode190() { return 0; }

    // Code 2 - AV Conduction
    public void vhCode2() { }
    public short vhCode210() { return 0; }
    public short vhCode220() { return 0; }
    public short vhCode221() { return 0; }
    public short vhCode222() { return 0; }
    public short vhCode223() { return 0; }
    public short vhCode231x() { return 0; }
    public short vhCode240() { return 0; }
    public short vhCode250() { return 0; }

    // Code 3 - Conduction Disturbances
    public void vhCode3() { }
    public short vhCode31x() { return 0; }
    public short vhCode32x() { return 0; }
    public short vhCode33x() { return 0; }
    public short vhCode341() { return 0; }
    public short vhCode342() { return 0; }
    public short vhCode350_1() { return 0; }
    public short vhCode352() { return 0; }
    public short vhCode371() { return 0; }
    public short vhCode372() { return 0; }
    public short vhCode373() { return 0; }
    public short vhCode380() { return 0; }

    // Code 4 - Repolarization
    public void vhCode4() { }
    public short vhCode41x() { return 0; }
    public short vhCode421() { return 0; }
    public short vhCode422() { return 0; }

    // Code 5 - MI/Ischemia
    public void vhCode5() { }
    public short vhCode501() { return 0; }
    public short vhCode502() { return 0; }
    public short vhCode510() { return 0; }
    public short vhCode520_30() { return 0; }
    public short vhCode540() { return 0; }
    public short vhCode550_6x() { return 0; }
    public short vhCode570() { return 0; }
    public short vhCode580() { return 0; }

    // Code 6 - Hypertrophy
    public void vhCode6() { }
    public short vhCode61x() { return 0; }
    public short vhCode621() { return 0; }
    public short vhCode631() { return 0; }
    public short vhCode641() { return 0; }

    // Code 7 - Axis
    public void vhCode7() { }
    public short vhCode710() { return 0; }
    public short vhCode720() { return 0; }
    public short vhCode730() { return 0; }
    public short vhCode740() { return 0; }
    public short vhCode750() { return 0; }
    public short vhCode76x() { return 0; }

    // Code 8 - Other
    public void vhCode8() { }
    public short vhCode810() { return 0; }
    public short vhCode82x() { return 0; }
    public short vhCode831() { return 0; }
    public short vhCode832() { return 0; }
    public short vhCode840() { return 0; }
    public short vhCode850() { return 0; }

    // Critical Values enum
    public const int CV_VSTOP = 11;
    public const int CV_VF = 12;
    public const int CV_VT = 13;
    public const int CV_PAUSE = 14;
    public const int CV_HVR = 15;
    public const int CV_LHR = 16;
    public const int CV_STD = 17;
    public const int CV_STE_HT = 18;
    public const int CV_STE_IT = 19;
    public const int CV_MPVC = 20;
    public const int CV_FPVC = 21;
    public const int CV_AVB = 22;
}
