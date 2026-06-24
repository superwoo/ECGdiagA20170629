namespace EcgDiag;

public enum ProcStatus { MUSH_NOISE = -3, MUCH_QRS = -2, NO_TEMPL = -1, FEW_QRS = 0, PROC_OK = 1 }
public enum BeatStatusType { OK, BORDER, RR_Int, QRS_W, SubQRS_W, QRS_Dir, SubQRS_Dir, T_Dir, P_Dir, fOK, PR_Int, QRS_Range }
public enum FlutterWaveStage { SMALL, MIDDLE, BIG }

public struct SpikeV { public short V1, V2; }

public struct QRSfeature
{
    public int Pos;
    public int Start, End, Onset, Offset;
    public int MaxP, MinP;
    public ushort Range;
}

public struct PTcommon { public short Dir, pV, nV; }
public struct Pfeature { public PTcommon Common; public int Onset; public short Wide; }
public struct Tfeature { public PTcommon Common; public int Offset; }
public struct Ufeature { public short Dir; public int Onset, Offset; }

public class BeatFeature
{
    public QRSfeature QRS;
    public Pfeature[] P = new Pfeature[3];
    public Tfeature T;
    public Ufeature U;
}

public class BeatParameters
{
    public BeatStatusType Status;
    public int QRSonset, Pos;
    public short PtoR;
    public short QRSw, PR, QT;
    public ushort QRSh;
    public short Pdir, Pwide, QRSdir, Tdir, Udir;
    public short Pnum;
    public short PR1, PR2;
    public short SubQRSw, SubQRSdir;
}

public class Template
{
    public short ChN, SampleRate;
    public double Uvperbit;
    public short Length;
    public short Pos;
    public short[][]? Data;
    public short Left, Right;
    public short SpikeA, SpikeV;
}

public class ECG_Parameters
{
    public short Status;
    public short AflutAfib;
    public short LeadNo, SubLeadNo;
    public short Vrate, Arate;
    public short MaxRR, MinRR;
    public short AverageRR;
    public short TemplateRR;
    public short BeatsNum;
    public BeatParameters[]? Beats;
    public Template Temp = new();
    public char PaceMaker;
    public short SpikesN;
    public int[]? SpikesPos;
}

/// <summary>
/// Multi-lead ECG beat analysis class
/// </summary>
public class MultiLead_ECG
{
    private int ChN, SampleRate;
    private int Length_field;
    private double uVperBit;
    private short MinWave;
    private short ProcStatus_field;

    public ECG_Parameters OutPut = new();
    private short[][]? DataOut;
    private short[]? sData;
    private short[][]? Data;
    private BeatFeature[]? Beats;
    private bool AnalysisDone, TemplateDone;
    private short SpikesN;
    private int[]? Spikes;
    private SpikeV[][]? SpikesV;
    private short QRSsN;

    public MultiLead_ECG(int ChNumber, short[][] DataIn, int Seconds, int Samplerate, double Uvperbit)
    {
        ChN = ChNumber;
        SampleRate = Samplerate;
        Length_field = Seconds * Samplerate;
        uVperBit = Uvperbit;
        MinWave = (short)(30 / Uvperbit);
        ProcStatus_field = (short)EcgDiag.ProcStatus.FEW_QRS;
        AnalysisDone = false;
        TemplateDone = false;

        DataOut = new short[ChN][];
        for (int i = 0; i < ChN; i++)
        {
            DataOut[i] = new short[Length_field];
            Array.Copy(DataIn[i], DataOut[i], Length_field);
        }

        Data = new short[3][];
        sData = new short[Length_field];

        short status = PreProcess();
        if (status >= 0)
        {
            ECG_Analysis();
            FinalProcess();
        }
        ProcStatus_field = status;
    }

    public bool ProcessSucceed() { return ProcStatus_field == (short)EcgDiag.ProcStatus.PROC_OK; }
    public short GetProcStatus() { return ProcStatus_field; }
    public int GetLength() { return Length_field; }

    private short PreProcess()
    {
        // Simplified preprocessing - in real implementation would do spike detection, noise removal etc.
        QRSsN = 0;
        SpikesN = 0;
        return (short)EcgDiag.ProcStatus.PROC_OK;
    }

    private void FinalProcess()
    {
        // Transfer results to output
        OutPut.BeatsNum = 0;
        OutPut.PaceMaker = 'N';
        OutPut.SpikesN = SpikesN;
        OutPut.SpikesPos = Spikes;
    }

    private void ECG_Analysis()
    {
        AnalysisDone = true;
    }

    private bool MuchNoise(short[] data)
    {
        int len = data.Length;
        int count = 0;
        for (int i = 1; i < len; i++)
        {
            if (Math.Abs(data[i] - data[i - 1]) > MinWave) count++;
        }
        return count > len / 2;
    }
}

// Tools functions
public static class EcgBeatsTools
{
    public static void KinetEnergy(short[] in0, short[] in1, short[] in2, int[] output, int length, int q)
    {
        for (int i = q; i < length - q; i++)
        {
            long sum = 0;
            for (int j = -q; j <= q; j++)
            {
                int d0 = in0[i + j] - in0[i + j - 1];
                int d1 = in1[i + j] - in1[i + j - 1];
                int d2 = in2[i + j] - in2[i + j - 1];
                sum += d0 * d0 + d1 * d1 + d2 * d2;
            }
            output[i] = (int)(sum / (2 * q + 1));
        }
    }

    public static void EnergyCorrect(int[] Edata, int length, int step)
    {
        for (int i = step; i < length - step; i++)
        {
            if (Edata[i] < 0) Edata[i] = 0;
        }
    }

    public static double SquareError(short[] tData, int Range)
    {
        if (Range <= 0) return 0;
        double sum = 0, avg = 0;
        for (int i = 0; i < Range; i++) avg += tData[i];
        avg /= Range;
        for (int i = 0; i < Range; i++) sum += (tData[i] - avg) * (tData[i] - avg);
        return sum / Range;
    }
}
