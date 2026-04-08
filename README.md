# ECG Diagnostic Library for .NET 10

This is a C# conversion of the ECGdiagA20170629 C++ library for ECG (Electrocardiogram) diagnostic analysis, targeting .NET 10.

## Project Structure

```
ECGDiag/
├── ECGDiag.csproj              # .NET 10 project file
├── DataStructures/
│   └── EcgDiagDefines.cs      # Core data structures, enums, and constants
├── Utilities/
│   ├── MyMath.cs               # Mathematical utilities (Max, Min, Average, Correlation)
│   └── CodeEx2.cs              # Diagnostic code management (linked list)
├── Filters/
│   ├── Filters.cs              # Lowpass/Highpass filters and sorting utilities
│   └── HspecgFilters.cs        # Notch filters and frequency interpolation
├── Core/                       # (To be converted)
│   ├── EcgCodeBase.cs         # Base diagnostic code class
│   ├── VhEcgDiag.cs           # Main diagnostic interface
│   ├── EcgCodeMC.cs           # Minnesota code diagnostics
│   └── EcgCodeVH.cs           # VH code diagnostics
└── Processing/                 # (To be converted)
    ├── ECGBeats.cs            # Beat detection and analysis
    ├── ECGTemplate.cs         # Template processing
    ├── ECGPropertyEx.cs       # Property extraction
    └── DoubleSampleRate.cs    # Sample rate conversion
```

## Key Conversion Changes

### From C++ to C#

1. **Memory Management**
   - C++ raw pointers → C# managed arrays and nullable references
   - Manual `new`/`delete` → Automatic garbage collection
   - `char*` strings → `string` type

2. **Data Types**
   - `BOOL` → `bool`
   - `BYTE` → `byte`
   - `DWORD` (uint32_t) → `uint`
   - `short` → `short`
   - `long` → `int` (for array indices) or `long` (for large values)
   - `double` → `double`

3. **Language Features**
   - C++ macros → C# static methods or constants
   - C++ inline functions → C# regular methods (JIT optimized)
   - C++ structs with unions → C# structs with `[StructLayout]` and `[FieldOffset]`
   - C++ destructors → C# finalizers (used sparingly, GC handles most cleanup)

4. **Coding Conventions**
   - PascalCase for public methods and properties
   - camelCase with underscore prefix for private fields
   - Nullable reference types enabled (`#nullable enable`)
   - Modern C# 12 features (pattern matching, target-typed new, etc.)

## Building the Project

### Prerequisites
- .NET 10 SDK or later
- C# 12 compiler

### Build Commands

```bash
# Build the project
dotnet build ECGDiag.csproj

# Build in Release mode
dotnet build ECGDiag.csproj -c Release

# Run tests (when available)
dotnet test
```

## Usage Example

```csharp
using ECGDiag.DataStructures;
using ECGDiag.Filters;
using ECGDiag.Utilities;

// Create a lowpass filter
var lpFilter = new LowpassFilter(freq: 40.0, sampleRate: 500);
int filteredValue = lpFilter.Filter(rawEcgSample);

// Use mathematical utilities
var maxFinder = new CMax();
short maxValue = maxFinder.Maximum(ecgData, ecgData.Length);
int maxIndex = maxFinder.GetIndex();

// Work with ECG lead information
var leadInfo = InitialEcgLeadInfo.Data[(int)EcgLeadIndex.V1];
Console.WriteLine($"Lead: {leadInfo.Name}, Mask: 0x{leadInfo.Mask:X}");
```

## Original C++ Project Information

- **Authors**: Various contributors including HuSheping, Du Xiaodong, VH Medical
- **Original Purpose**: ECG diagnostic analysis with Minnesota and VH coding systems
- **Key Features**:
  - Multi-lead ECG analysis (up to 18 leads)
  - Beat detection and classification
  - Template generation and matching
  - Signal filtering (lowpass, highpass, notch)
  - Diagnostic code generation (Minnesota codes, VH codes)
  - Atrial fibrillation/flutter detection
  - Pacemaker detection

## Conversion Status

- [x] Project structure and .csproj
- [x] Data structures (EcgDiagDefines)
- [x] Utilities (MyMath, CodeEx2)
- [x] Filters (Lowpass, Highpass, Notch filters)
- [ ] Core diagnostic classes (EcgCodeBase, VhEcgDiag, etc.)
- [ ] Processing classes (ECGBeats, ECGTemplate, etc.)
- [ ] Unit tests
- [ ] Documentation

## License

Original copyright notices from the C++ source files have been preserved in the converted files.

## Notes

- This is a direct port focusing on functional equivalence with the C++ version
- Some optimizations specific to .NET (like using `Span<T>`) may be added in future versions
- All Chinese comments from the original source code have been preserved
- XML documentation comments should be added for public APIs
