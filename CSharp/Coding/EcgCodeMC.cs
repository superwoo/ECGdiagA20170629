namespace EcgDiag;

/// <summary>
/// Minnesota Code analysis class
/// </summary>
public class CmcCode : CEcgCodeBase
{
    protected CodeMgr mc = new();

    public CmcCode() { }

    public short mcCodeCount() { return (short)mc.GetCount(); }

    public short mcCodeGetFirst(string? szLeadName = null)
    {
        var p = mc.GetFirst();
        if (p == null) return -1;
        if (szLeadName != null) LeadNames(p.Value.nLeads);
        return (short)p.Value.nCode;
    }

    public short mcCodeGetNext(string? szLeadName = null)
    {
        var p = mc.GetNext();
        if (p == null) return -1;
        if (szLeadName != null) LeadNames(p.Value.nLeads);
        return (short)p.Value.nCode;
    }

    protected ushort SetmcCode(ushort nCode, uint dwLeads = 0)
    {
        Code code = new() { nCode = nCode, nLeads = dwLeads, nClass = 0, nSort = 0 };
        mc.Add(code);
        return nCode;
    }

    public void mcCode()
    {
        mc.Reset();
        mcCode1(); mcCode2(); mcCode3(); mcCode4(); mcCode5();
        mcCode6(); mcCode7(); mcCode8(); mcCode9(); mcCode10();
    }

    //3.1 Code 1, Q and QS Patterns
    protected virtual void mcCode1() { }
    //3.2 Code 2, QRS Axis Deviation
    protected virtual void mcCode2() { }
    //3.3 Code 3, High Amplitude R Waves
    protected virtual void mcCode3() { }
    //3.4 Code 4, ST Junction and Segment Depression
    protected virtual void mcCode4() { }
    //3.5 Code 5, T-Wave Items
    protected virtual void mcCode5() { }
    //3.6 Code 6, A-V Conduction Defect
    protected virtual void mcCode6() { }
    //3.7 Code 7, Ventricular Conduction Defect
    protected virtual void mcCode7() { }
    //3.8 Code 8, Arrhythmias
    protected virtual void mcCode8() { }
    //3.9 Code 9, ST Segment Elevation
    protected virtual void mcCode9() { }
    //3.10 Code 10, Miscellaneous Items
    protected virtual void mcCode10() { }
}
