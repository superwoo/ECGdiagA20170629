using System.Runtime.InteropServices;

namespace ECGDiag.DataStructures;

/// <summary>
/// ECG diagnostic data structures and constants
/// </summary>
public static class EcgDiagDefines
{
    public const int VH_EcgMaxLeads = 18;

    // Math utility methods
    public static T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) >= 0 ? a : b;
    public static T Max3<T>(T a, T b, T c) where T : IComparable<T> => Max(Max(a, b), c);
    public static T Min<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) <= 0 ? a : b;
    public static T Min3<T>(T a, T b, T c) where T : IComparable<T> => Min(Min(a, b), c);
    public static T Abs<T>(T a) where T : IComparable<T>, IConvertible
    {
        if (a.CompareTo(default(T)!) >= 0) return a;
        dynamic val = a;
        return -val;
    }
    public static double Square(double a) => a * a;
}

/// <summary>
/// ECG lead indices
/// </summary>
public enum EcgLeadIndex
{
    I = 0, II, III, aVR, aVL, aVF, V1, V2, V3, V4, V5, V6,
    V3R, V4R, V5R, V7, V8, V9
}

/// <summary>
/// ECG lead information
/// </summary>
public struct VH_EcgLeadInfo
{
    public short Lead;
    public short Chn;      // only this value will be changed
    public uint Mask;
    public string Name;    // char[4] in C++
    public short Quality;  // -2:no channel, -1:unknown, 0:normal, 1:VF, 2:Vf, 3:no signal

    public VH_EcgLeadInfo(short lead, short chn, uint mask, string name, short quality = -1)
    {
        Lead = lead;
        Chn = chn;
        Mask = mask;
        Name = name;
        Quality = quality;
    }
}

/// <summary>
/// ECG parameters (in ms and uV)
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 128)]
public struct VH_ECGparm
{
    [FieldOffset(0)] public short RR;
    [FieldOffset(2)] public short HR;
    [FieldOffset(4)] public short Pd;
    [FieldOffset(6)] public short PR;
    [FieldOffset(8)] public short QRS;
    [FieldOffset(10)] public short QT;
    [FieldOffset(12)] public short QTC;
    [FieldOffset(14)] public byte WPW;      // A/a B/b W/w  A型, B型, W型, 小写: 可疑, 其它: 无WPW
    [FieldOffset(15)] public byte Empty;
    [FieldOffset(16)] public short QTdis;
    [FieldOffset(18)] public short QTmax;
    [FieldOffset(20)] public short QTmin;
    [FieldOffset(22)] public short QTmaxLead;
    [FieldOffset(24)] public short QTminLead;
    [FieldOffset(26)] public short AxisP;      // Degree, for ChNumber==12 only
    [FieldOffset(28)] public short AxisQRS;    // Degree, for ChNumber==12 only
    [FieldOffset(30)] public short AxisT;      // Degree, for ChNumber==12 only
    [FieldOffset(32)] public short UvRV5;      // uV, for ChNumber==12 only
    [FieldOffset(34)] public short UvRV6;      // uV, for ChNumber==12 only
    [FieldOffset(36)] public short UvSV1;      // uV, for ChNumber==12 only
    [FieldOffset(38)] public short UvSV2;      // uV, for ChNumber==12 only
    [FieldOffset(40)] public short OnOff0;
    [FieldOffset(42)] public short OnOff1;
    [FieldOffset(44)] public short OnOff2;
    [FieldOffset(46)] public short OnOff3;
    [FieldOffset(48)] public short OnOff4;
    [FieldOffset(50)] public short OnOff5;
    [FieldOffset(52)] public short UvRV1;
    [FieldOffset(54)] public short UvSV5;

    public short[] OnOff
    {
        get => new[] { OnOff0, OnOff1, OnOff2, OnOff3, OnOff4, OnOff5 };
        set
        {
            if (value.Length >= 6)
            {
                OnOff0 = value[0];
                OnOff1 = value[1];
                OnOff2 = value[2];
                OnOff3 = value[3];
                OnOff4 = value[4];
                OnOff5 = value[5];
            }
        }
    }
}

/// <summary>
/// ECG lead-specific parameters (in ms and uV)
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 128)]
public struct VH_ECGlead
{
    [FieldOffset(0)] public short OnOff0;     // Pb
    [FieldOffset(2)] public short OnOff1;     // Pe
    [FieldOffset(4)] public short OnOff2;     // QRSb
    [FieldOffset(6)] public short OnOff3;     // QRSe
    [FieldOffset(8)] public short OnOff4;     // Tb
    [FieldOffset(10)] public short OnOff5;    // Te
    [FieldOffset(12)] public short Pstatus;   // 0: none, 1: +, 2: -, 3: +-, 4: -+
    [FieldOffset(14)] public short Tstatus;
    [FieldOffset(16)] public short Pd;
    [FieldOffset(18)] public short Qd;
    [FieldOffset(20)] public short Rd1;
    [FieldOffset(22)] public short Rd2;
    [FieldOffset(24)] public short Sd1;
    [FieldOffset(26)] public short Sd2;
    [FieldOffset(28)] public short Td;
    [FieldOffset(30)] public short PR;
    [FieldOffset(32)] public short QT;
    [FieldOffset(34)] public short QRS;
    [FieldOffset(36)] public short Pa1;
    [FieldOffset(38)] public short Pa2;
    [FieldOffset(40)] public short Qa;
    [FieldOffset(42)] public short Ra1;
    [FieldOffset(44)] public short Ra2;
    [FieldOffset(46)] public short Sa1;
    [FieldOffset(48)] public short Sa2;
    [FieldOffset(50)] public short Ta1;
    [FieldOffset(52)] public short Ta2;
    [FieldOffset(54)] public short Rnotch;    // 0,1,2,3: none, 上升边, 下降边, 两边
    [FieldOffset(56)] public short ST0;       // STj
    [FieldOffset(58)] public short ST1;
    [FieldOffset(60)] public short ST2;
    [FieldOffset(62)] public short ST3;
    [FieldOffset(64)] public short ST4;       // ST20
    [FieldOffset(66)] public short ST5;       // ST40
    [FieldOffset(68)] public short ST6;       // ST60
    [FieldOffset(70)] public short ST7;       // ST80
    [FieldOffset(72)] public float STslope0;
    [FieldOffset(76)] public float STslope1;
    [FieldOffset(80)] public float STslope2;
    [FieldOffset(84)] public float STslope3;

    public short[] OnOff
    {
        get => new[] { OnOff0, OnOff1, OnOff2, OnOff3, OnOff4, OnOff5 };
        set
        {
            if (value.Length >= 6)
            {
                OnOff0 = value[0];
                OnOff1 = value[1];
                OnOff2 = value[2];
                OnOff3 = value[3];
                OnOff4 = value[4];
                OnOff5 = value[5];
            }
        }
    }

    public short[] ST
    {
        get => new[] { ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7 };
        set
        {
            if (value.Length >= 8)
            {
                ST0 = value[0];
                ST1 = value[1];
                ST2 = value[2];
                ST3 = value[3];
                ST4 = value[4];
                ST5 = value[5];
                ST6 = value[6];
                ST7 = value[7];
            }
        }
    }

    public float[] STslope
    {
        get => new[] { STslope0, STslope1, STslope2, STslope3 };
        set
        {
            if (value.Length >= 4)
            {
                STslope0 = value[0];
                STslope1 = value[1];
                STslope2 = value[2];
                STslope3 = value[3];
            }
        }
    }
}

/// <summary>
/// ECG beat parameters (similar to BeatParameters)
/// </summary>
public struct VH_ECGbeat
{
    public bool Status;        // 是否叠加
    public int QRSonset;
    public int Pos;
    public short QRSw;         // ms
    public short PR;           // ms
    public short QT;           // ms
    public short Pdir;         // 0: none, 1: +, -1: -, 2: +- (+>-), -2: +- (->+)
    public short QRSdir;
    public short Tdir;
    public short Udir;
    public short Pnum;         // 0, 1, 2 (P0,P1)
    public short SubQRSw;
    public short SubQRSdir;
}

/// <summary>
/// ECG information (similar to ECG_Parameters)
/// </summary>
public class VH_ECGinfo
{
    public bool Status;            // 模板是否有效
    public short AflutAfib;        // 0:none, 1:Aflut(房扑), 2:Afib(房颤)
    public short LeadNo;
    public short SubLeadNo;
    public short Vrate;
    public short Arate;
    public short BeatsNum;         // Number of Beats
    public VH_ECGbeat[]? Beats;
    public byte PaceMaker;         // 'N': none, 'A': A-Type, 'V': V-type, 'B': Both
    public short SpikesN;
    public int[]? SpikesPos;

    public VH_ECGinfo()
    {
        Status = false;
        AflutAfib = 0;
        LeadNo = 0;
        SubLeadNo = 0;
        Vrate = 0;
        Arate = 0;
        BeatsNum = 0;
        PaceMaker = (byte)'N';
        SpikesN = 0;
    }
}

/// <summary>
/// Template data structure
/// </summary>
public class VH_Template
{
    public short ChN;              // 通道数
    public short SampleRate;       // 采样频率
    public double Uvperbit;        // 每位微伏数
    public short Length;           // length of template data
    public short Pos;              // 叠加位置
    public short[][]? Data;        // template data (8通道)
    public short Left;             // analysis range is from Left to Right
    public short Right;
    public short SpikeA;           // 房起搏钉位置
    public short SpikeV;           // 室起搏钉位置

    public VH_Template()
    {
        ChN = 0;
        SampleRate = 0;
        Uvperbit = 0;
        Length = 0;
        Pos = 0;
        Left = 0;
        Right = 0;
        SpikeA = 0;
        SpikeV = 0;
    }
}

/// <summary>
/// Initial ECG lead information
/// </summary>
public static class InitialEcgLeadInfo
{
    public static readonly VH_EcgLeadInfo[] Data = new[]
    {
        new VH_EcgLeadInfo((short)EcgLeadIndex.I,    0, 0x000001, "I"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.II,   1, 0x000002, "II"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.III,  2, 0x000004, "III"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.aVR,  3, 0x000008, "aVR"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.aVL,  4, 0x000010, "aVL"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.aVF,  5, 0x000020, "aVF"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V1,   6, 0x000040, "V1"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V2,   7, 0x000080, "V2"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V3,   8, 0x000100, "V3"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V4,   9, 0x000200, "V4"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V5,  10, 0x000400, "V5"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V6,  11, 0x000800, "V6"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V3R, 12, 0x001000, "V3R"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V4R, 13, 0x002000, "V4R"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V5R, 14, 0x004000, "V5R"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V7,  15, 0x008000, "V7"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V8,  16, 0x010000, "V8"),
        new VH_EcgLeadInfo((short)EcgLeadIndex.V9,  17, 0x020000, "V9")
    };

    public static readonly uint MI_L = Data[(int)EcgLeadIndex.aVL].Mask | Data[(int)EcgLeadIndex.I].Mask | Data[(int)EcgLeadIndex.aVR].Mask;
    public static readonly uint MI_I = Data[(int)EcgLeadIndex.II].Mask | Data[(int)EcgLeadIndex.aVF].Mask | Data[(int)EcgLeadIndex.III].Mask;
    public static readonly uint MI_S = Data[(int)EcgLeadIndex.V1].Mask | Data[(int)EcgLeadIndex.V2].Mask;
    public static readonly uint MI_A = Data[(int)EcgLeadIndex.V3].Mask | Data[(int)EcgLeadIndex.V4].Mask | Data[(int)EcgLeadIndex.V5].Mask | Data[(int)EcgLeadIndex.V6].Mask;
}
