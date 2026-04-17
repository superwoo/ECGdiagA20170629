# Anomalous Sound Detection using Gaussian Mixture Model (GMM)

This project implements anomalous sound detection using Accord.NET's GaussianMixtureModel for machine learning-based anomaly detection.

## Overview

The system uses Gaussian Mixture Models (GMM) to model the distribution of normal sound patterns and detect anomalies when sounds deviate significantly from the learned distribution.

## Components

### 1. AudioFeatureExtractor.cs
Extracts audio features from sound samples including:
- **Energy**: Log energy of the signal
- **Zero Crossing Rate (ZCR)**: Rate at which signal changes sign
- **Spectral Centroid**: Center of mass of the spectrum
- **Spectral Rolloff**: Frequency below which 85% of energy is contained
- **RMS (Root Mean Square)**: Overall amplitude
- **Spectral Features**: First 10 frequency bins from FFT

### 2. GMManomalyDetector.cs
Main anomaly detection class that:
- Trains a Gaussian Mixture Model on normal sound samples
- Uses log-likelihood as anomaly score
- Sets threshold based on percentile of training data
- Provides methods to detect anomalies and get detailed results

### 3. Program.cs
Demonstration program that:
- Generates synthetic normal and anomalous sound samples
- Trains the GMM detector on normal samples
- Tests detection accuracy on both normal and anomalous samples
- Reports overall performance

## Key Features

- **Feature-based Analysis**: Extracts multiple audio features for robust detection
- **Unsupervised Learning**: Only requires normal samples for training
- **Configurable GMM**: Adjustable number of mixture components
- **Threshold Tuning**: Percentile-based threshold setting for controlling sensitivity
- **Comprehensive Scoring**: Provides both binary classification and continuous anomaly scores

## Usage

### Training the Detector

```csharp
// Create detector with 3 mixture components
var detector = new GMManomalyDetector(numberOfComponents: 3, sampleRate: 16000);

// Train on normal sound samples
List<float[]> normalSamples = LoadNormalSamples();
detector.Train(normalSamples, percentile: 95.0);
```

### Detecting Anomalies

```csharp
// Simple detection
float[] testSample = LoadTestSample();
bool isAnomalous = detector.IsAnomalous(testSample);

// Detailed results
var result = detector.GetDetailedResult(testSample);
Console.WriteLine($"Anomalous: {result.IsAnomalous}");
Console.WriteLine($"Score: {result.AnomalyScore}");
Console.WriteLine($"Threshold: {result.Threshold}");
```

## Building and Running

```bash
cd AnomalousSoundDetection
dotnet build
dotnet run
```

## Dependencies

- Accord.MachineLearning (3.8.0)
- Accord.Audio (3.8.0)
- Accord.Math (3.8.0)
- .NET 8.0

## How It Works

1. **Feature Extraction**: Audio samples are divided into overlapping frames, and multiple acoustic features are extracted from each frame.

2. **Model Training**: A Gaussian Mixture Model learns the distribution of feature vectors from normal sound samples.

3. **Anomaly Detection**: For new samples, the GMM calculates the log-likelihood of the features. Samples with log-likelihood below the threshold are classified as anomalous.

4. **Threshold Setting**: The threshold is set based on a percentile of training data log-likelihoods (e.g., 95th percentile means 5% of training samples would be flagged).

## Applications

- Industrial equipment monitoring
- Environmental sound monitoring
- Security systems
- Quality control in manufacturing
- Healthcare monitoring (e.g., breathing sounds, heart sounds)

## Customization

### Adjusting Sensitivity

- **Higher percentile** (e.g., 99): More sensitive, catches more anomalies but may have false positives
- **Lower percentile** (e.g., 90): Less sensitive, fewer false positives but may miss some anomalies

### Feature Engineering

Add or modify features in `AudioFeatureExtractor.cs`:
- MFCCs (Mel-frequency cepstral coefficients)
- Chroma features
- Temporal features
- Domain-specific features

### GMM Configuration

- **More components**: Better fit to complex distributions but slower training
- **Fewer components**: Faster but may not capture all variations

## License

This implementation uses Accord.NET Framework which is licensed under LGPL v2.1.
