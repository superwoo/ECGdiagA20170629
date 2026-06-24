namespace EcgDiag;

/// <summary>
/// ECG Lead indices
/// </summary>
public enum EcgLeadIndex
{
    I = 0, II, III, aVR, aVL, aVF, V1, V2, V3, V4, V5, V6, V3R, V4R, V5R, V7, V8, V9
}

public struct VH_EcgLeadInfo
{
    public short Lead;
    public short Chn;
    public uint Mask;
    public string Name;
    public short Quality; //-2:no channel,-1:unknown,0:normal,1:VF,2:Vf,3:no signal

    public VH_EcgLeadInfo(short lead, short chn, uint mask, string name, short quality)
    {
        Lead = lead;
        Chn = chn;
        Mask = mask;
        Name = name;
        Quality = quality;
    }
}

public class VH_ECGparm
{
    public short RR, HR;
    public short Pd, PR, QRS, QT, QTC;
    public char WPW; //return: A/a B/b W/w  A型, B型, W型, 小写: 可疑, 其它: 无WPW
    public short QTdis, QTmax, QTmin;
    public short QTmaxLead, QTminLead;
    public short axisP, axisQRS, axisT;
    public short uvRV5, uvRV6, uvSV1, uvSV2;
    public short[] OnOff;
    public short uvRV1, uvSV5;

    public VH_ECGparm()
    {
        OnOff = new short[6];
    }
}

public class VH_ECGlead
{
    public short[] OnOff; //Pb,Pe,QRSb,QRSe,Tb,Te; //特征点位置
    public short Pstatus;  //0: none, 1: +, 2: -; 3: +-; 4: -+
    public short Tstatus;
    public short Pd, Qd, Rd1, Rd2, Sd1, Sd2, Td, PR, QT, QRS;
    public short Pa1, Pa2, Qa, Ra1, Ra2, Sa1, Sa2, Ta1, Ta2;
    public short Rnotch; //0,1,2,3: none,上升边，下降边，两边
    public short[] ST;   //STj,ST1,ST2,ST3,ST20,ST40,ST60,ST80
    public float[] STslope;
    public byte[] morpho;

    public VH_ECGlead()
    {
        OnOff = new short[6];
        ST = new short[8];
        STslope = new float[4];
        morpho = new byte[8];
    }
}

public class VH_ECGbeat
{
    public bool Status;  //0,1 是否叠加
    public int QRSonset, Pos;
    public short QRSw, PR, QT;    //mS
    public short Pdir, QRSdir, Tdir, Udir;  //0: none, 1: +, -1: -, 2: +- (+>-), -2: +- (->+)
    public short Pnum;    //0, 1, 2 (P0,P1)
    public short SubQRSw, SubQRSdir;
}

public class VH_ECGinfo
{
    public bool Status;    //模板是否有效
    public short AflutAfib;    //0:none, 1:Aflut(房扑), 2:Afib(房颤)
    public short LeadNo, SubLeadNo;
    public short Vrate, Arate;
    public short BeatsNum;
    public VH_ECGbeat[]? Beats;
    public char PaceMaker; //'N': none, 'A': A-Type, 'V': V-type, 'B': Both
    public short SpikesN;
    public int[]? SpikesPos;
}

public class VH_Template
{
    public short ChN, SampleRate; //通道数，采样频率
    public double Uvperbit;       //每位微伏数
    public short Length;           //length of template data
    public short Pos;              //叠加位置
    public short[][]? Data;        //template data (8通道)
    public short Left, Right;      //analysis range is from Left to Right
    public short SpikeA, SpikeV;   // 房室起搏钉位置
}

public static class EcgDiagConstants
{
    public const short VH_EcgMaxLeads = 18;

    public static readonly VH_EcgLeadInfo[] InitialEcgLeadInfo = new VH_EcgLeadInfo[]
    {
        new(0, 0, 0x000001, "I", -1),
        new(1, 1, 0x000002, "II", -1),
        new(2, 2, 0x000004, "III", -1),
        new(3, 3, 0x000008, "aVR", -1),
        new(4, 4, 0x000010, "aVL", -1),
        new(5, 5, 0x000020, "aVF", -1),
        new(6, 6, 0x000040, "V1", -1),
        new(7, 7, 0x000080, "V2", -1),
        new(8, 8, 0x000100, "V3", -1),
        new(9, 9, 0x000200, "V4", -1),
        new(10, 10, 0x000400, "V5", -1),
        new(11, 11, 0x000800, "V6", -1),
        new(12, 12, 0x001000, "V3R", -1),
        new(13, 13, 0x002000, "V4R", -1),
        new(14, 14, 0x004000, "V5R", -1),
        new(15, 15, 0x008000, "V7", -1),
        new(16, 16, 0x010000, "V8", -1),
        new(17, 17, 0x020000, "V9", -1)
    };

    public static readonly uint MI_L = InitialEcgLeadInfo[(int)EcgLeadIndex.aVL].Mask
                                     | InitialEcgLeadInfo[(int)EcgLeadIndex.I].Mask
                                     | InitialEcgLeadInfo[(int)EcgLeadIndex.aVR].Mask;

    public static readonly uint MI_I = InitialEcgLeadInfo[(int)EcgLeadIndex.II].Mask
                                     | InitialEcgLeadInfo[(int)EcgLeadIndex.aVF].Mask
                                     | InitialEcgLeadInfo[(int)EcgLeadIndex.III].Mask;

    public static readonly uint MI_S = InitialEcgLeadInfo[(int)EcgLeadIndex.V1].Mask
                                     | InitialEcgLeadInfo[(int)EcgLeadIndex.V2].Mask;

    public static readonly uint MI_A = InitialEcgLeadInfo[(int)EcgLeadIndex.V3].Mask
                                     | InitialEcgLeadInfo[(int)EcgLeadIndex.V3].Mask
                                     | InitialEcgLeadInfo[(int)EcgLeadIndex.V5].Mask
                                     | InitialEcgLeadInfo[(int)EcgLeadIndex.V6].Mask;
}
