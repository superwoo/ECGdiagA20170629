# C++ to C# Conversion Guide for ECGdiagA20170629

## Overview
This document provides detailed guidance for converting the remaining C++ files to C# for .NET 10.

## Completed Conversions

### ✅ Data Structures
- `EcgDiagDefines.h` → `DataStructures/EcgDiagDefines.cs`
  - Converted structs with unions using `[StructLayout(LayoutKind.Explicit)]`
  - Converted macros to static methods
  - Converted enums to C# enums
  - Array properties for accessing union fields

### ✅ Utilities
- `MyMath.cpp/.h` → `Utilities/MyMath.cs`
  - CMax, CMin, CAverage, CRootMeanSquare classes
  - Linear correlation utilities
  - All overloaded methods for different types

- `CodeEx2.cpp/.h` → `Utilities/CodeEx2.cs`
  - Linked list implementation converted to C# nullable references
  - CodeMgr class for diagnostic code management

### ✅ Filters
- `Filters.cpp/.h` → `Filters/Filters.cs`
  - LowpassFilter and HighpassFilter classes
  - BubbleSort utilities with C# tuples for swapping

- `hspecgFilters.cpp/.h` → `Filters/HspecgFilters.cs`
  - CBaseNotchFilter, CBlNotchFilter
  - FreqInterpretate for frequency interpolation
  - Multi-channel filter classes

### ✅ Core (Partial)
- `EcgCodeBase.h` → `Core/EcgCodeBase.cs` (base implementation)
  - CMean helper class
  - Protected inline methods converted to protected methods
  - Data access and quality check methods

## Remaining Conversions

### 🔄 Core Classes (Large Files)

#### 1. vhEcgDiag.cpp/.h → Core/VhEcgDiag.cs
**File Size**: ~1000 lines
**Complexity**: Medium
**Key Patterns**:
```csharp
public class CvhEcgDiag
{
    // Convert member variables
    protected short[] m_nVindex = new short[12];
    protected VH_ECGparm? m_pEcgParm;
    protected VH_ECGlead[]? m_pEcgLead;

    // Convert methods
    public bool CreateEcgDiag(short chNumber, short[][] dataIn,
                              short seconds, short samplerate, double uVperbit)
    {
        // Implementation
    }

    public bool EcgCode(byte sex, short age, short ageYmd = 0)
    {
        // Implementation
    }

    // Accessor methods
    public short GetFirstMcode(string? szLeadName = null) { }
    public short GetNextMcode(string? szLeadName = null) { }

    // Static methods
    public static string McCode(short code) { }
    public static string VhCode(short code) { }
}
```

#### 2. EcgCodeMC.cpp/.h → Core/EcgCodeMC.cs
**File Size**: ~2000 lines
**Complexity**: High (lots of diagnostic logic)
**Key Patterns**:
- Inherits from CEcgCodeBase
- Minnesota code detection and classification
- Multiple conditional checks for different ECG patterns
```csharp
public class CEcgCodeMC : CEcgCodeBase
{
    protected CodeMgr m_mcCode = new();

    protected void MCcode()
    {
        // Implement Minnesota coding logic
    }

    protected short MC_RhythmCode() { }
    protected short MC_AtrialCode() { }
    // ... many more diagnostic methods
}
```

#### 3. EcgCodeVH.cpp/.h → Core/EcgCodeVH.cs
**File Size**: ~3000+ lines
**Complexity**: Very High (complex diagnostic algorithms)
**Key Patterns**:
- Inherits from CEcgCodeMC
- VH-specific diagnostic codes
- Complex waveform analysis
```csharp
public class CEcgCodeVH : CEcgCodeMC
{
    protected CodeMgr m_vhCode = new();

    protected void VHcode()
    {
        // Implement VH coding logic
    }

    // Numerous diagnostic helper methods
    protected bool IsMyocardialInfarction() { }
    protected bool IsIschemia() { }
    // ... hundreds more methods
}
```

### 🔄 Processing Classes

#### 4. ECGbeats.cpp/.h → Processing/ECGBeats.cs
**File Size**: ~2200 lines
**Complexity**: Very High
**Key Patterns**:
```csharp
// Enums
public enum PROC_STATUS
{
    MUSH_NOISE = -3,
    MUCH_QRS = -2,
    NO_TEMPL = -1,
    FEW_QRS = 0,
    PROC_OK = 1
}

public enum BEAT_STATUS_TYPE
{
    OK, BORDER, RR_Int, QRS_W, SubQRS_W,
    QRS_Dir, SubQRS_Dir, T_Dir, P_Dir,
    fOK, PR_Int, QRS_Range
}

// Structures
public struct QRSfeature
{
    public int Pos;
    public int Start, End, Onset, Offset;
    public int MaxP, MinP;
    public ushort Range;
}

public struct BeatFeature
{
    public QRSfeature QRS;
    public Pfeature[] P;  // Array of 3
    public Tfeature T;
    public Ufeature U;
}

// Main class
public class MultiLead_ECG
{
    private int ChN, SampleRate;
    private int Length;
    private double uVperBit;
    private short[][]? DataOut;

    public MultiLead_ECG(int chNumber, short[][] dataIn,
                         int seconds, int sampleRate, double uvperbit)
    {
        // Constructor implementation
    }

    public ECG_Parameters OutPut { get; set; }

    private void PreProcess() { }
    private void ECG_Analysis() { }
    private void QRSanalysis(int QRSi) { }
    private void PTUanalysis(int QRSi) { }
}
```

#### 5. ECGTempl.cpp → Processing/ECGTemplate.cs
**File Size**: ~3800 lines
**Complexity**: Very High (complex template matching)
**Conversion Notes**:
- Contains sophisticated ECG template generation algorithms
- Wave detection and measurement
- Requires careful conversion of pointer arithmetic to array indexing

#### 6. ECGpropEx.cpp/.h → Processing/ECGPropertyEx.cs
**File Size**: ~700 lines
**Complexity**: Medium
**Key Patterns**:
- Property extraction from ECG data
- QRS, P, T wave measurements
- Axis calculations

### 🔄 Additional Filter Classes

#### 7. CFilters.mm/.h → Filters/CFilters.cs
**Notes**: Objective-C++ file, needs special attention
**Complexity**: Medium

#### 8. DoubleSampleRate.mm/.h → Processing/DoubleSampleRate.cs
**Notes**: Objective-C++ file for sample rate conversion
**Complexity**: Low

## Conversion Best Practices

### 1. Array and Pointer Conversion
```csharp
// C++: short **data
// C#: short[][]? data  (jagged array)

// C++: short *data[3]
// C#: short[][] data = new short[3][]

// C++: pointer arithmetic data[i][j+offset]
// C#: array indexing data[i][j+offset] (same syntax, but safer)
```

### 2. String Handling
```csharp
// C++: char szName[64]; strcpy(szName, "text");
// C#: string szName = "text";

// C++: char *pName = "text";
// C#: string pName = "text";

// C++: strlen(str)
// C#: str.Length

// C++: strcmp(str1, str2)
// C#: str1 == str2  or  string.Compare(str1, str2)
```

### 3. Boolean Values
```csharp
// C++: BOOL value = TRUE;
// C#: bool value = true;

// C++: if(pointer) { }
// C#: if(pointer != null) { }
```

### 4. Inline Functions
```csharp
// C++ inline functions → C# regular methods
// The JIT compiler will inline automatically when appropriate
// For performance-critical paths, use:
[MethodImpl(MethodImplOptions.AggressiveInlining)]
protected short SomeMethod() { }
```

### 5. Class Member Initialization
```csharp
// C++: Constructor initialization list
// CEcgCodeBase::CEcgCodeBase() : m_fs(0), m_chnum(0) { }

// C#: Field initializers or constructor body
public class CEcgCodeBase
{
    protected short m_fs = 0;
    protected short m_chnum = 0;

    // Or in constructor:
    public CEcgCodeBase()
    {
        m_fs = 0;
        m_chnum = 0;
    }
}
```

### 6. Const Methods and Parameters
```csharp
// C++: void Method(const int value) const { }
// C#: void Method(int value) { }
// (C# parameters are const by default, no const methods)
```

### 7. Mathematical Operations
```csharp
// C++: #include <math.h>
// C#: using System;

// C++: sqrt(x), pow(x,y), abs(x)
// C#: Math.Sqrt(x), Math.Pow(x,y), Math.Abs(x)

// C++: fmax(a,b), fmin(a,b)
// C#: Math.Max(a,b), Math.Min(a,b)
```

## Testing Strategy

After conversion, create unit tests for:

1. **Data Structure Tests**
   - Verify struct layouts match expected sizes
   - Test union field access
   - Validate initial values

2. **Filter Tests**
   - Compare filter output with C++ version
   - Test edge cases (zero input, max input)
   - Verify frequency response

3. **Diagnostic Code Tests**
   - Test with known ECG patterns
   - Verify Minnesota code generation
   - Check VH code accuracy

4. **Integration Tests**
   - Full ECG analysis pipeline
   - Compare results with C++ version on same data

## Performance Considerations

For performance-critical sections:

1. **Use Span<T> for array operations**
```csharp
public void ProcessECG(ReadOnlySpan<short> data)
{
    // More efficient than short[] for large arrays
}
```

2. **Use stackalloc for small temporary buffers**
```csharp
Span<short> tempBuffer = stackalloc short[100];
```

3. **Consider SIMD operations**
```csharp
using System.Numerics;
// Vector operations for parallel processing
```

## Documentation

Add XML documentation comments:
```csharp
/// <summary>
/// Analyzes ECG data and generates diagnostic codes
/// </summary>
/// <param name="data">ECG signal data</param>
/// <param name="sampleRate">Sampling frequency in Hz</param>
/// <returns>True if analysis succeeded</returns>
public bool AnalyzeECG(short[][] data, int sampleRate)
{
    // Implementation
}
```

## Build and Package

Create a NuGet package:
```xml
<PropertyGroup>
  <PackageId>ECGDiag</PackageId>
  <Version>1.0.0</Version>
  <Authors>VH Medical</Authors>
  <Description>ECG Diagnostic Library for .NET</Description>
  <PackageTags>ECG;Medical;Diagnostics;Cardiology</PackageTags>
</PropertyGroup>
```

## Next Steps

1. Complete Core classes (VhEcgDiag, EcgCodeMC, EcgCodeVH)
2. Convert Processing classes (ECGBeats, ECGTemplate, ECGPropertyEx)
3. Convert remaining Filter classes
4. Create comprehensive unit tests
5. Add XML documentation
6. Performance profiling and optimization
7. Create sample application demonstrating usage
