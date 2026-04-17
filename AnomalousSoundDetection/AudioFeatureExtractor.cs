using Accord.Audio;
using Accord.Audio.Filters;
using Accord.Math;

namespace AnomalousSoundDetection
{
    /// <summary>
    /// Extracts audio features for anomaly detection
    /// </summary>
    public class AudioFeatureExtractor
    {
        private readonly int sampleRate;
        private readonly int frameSize;
        private readonly int hopSize;

        public AudioFeatureExtractor(int sampleRate = 16000, int frameSize = 512, int hopSize = 256)
        {
            this.sampleRate = sampleRate;
            this.frameSize = frameSize;
            this.hopSize = hopSize;
        }

        /// <summary>
        /// Extract features from audio signal
        /// </summary>
        /// <param name="signal">Audio signal samples</param>
        /// <returns>Feature matrix where each row is a feature vector</returns>
        public double[][] ExtractFeatures(float[] signal)
        {
            var frames = CreateFrames(signal);
            var features = new List<double[]>();

            foreach (var frame in frames)
            {
                var featureVector = ExtractFrameFeatures(frame);
                features.Add(featureVector);
            }

            return features.ToArray();
        }

        /// <summary>
        /// Create overlapping frames from signal
        /// </summary>
        private List<float[]> CreateFrames(float[] signal)
        {
            var frames = new List<float[]>();

            for (int i = 0; i <= signal.Length - frameSize; i += hopSize)
            {
                var frame = new float[frameSize];
                Array.Copy(signal, i, frame, 0, frameSize);
                frames.Add(frame);
            }

            return frames;
        }

        /// <summary>
        /// Extract features from a single frame
        /// </summary>
        private double[] ExtractFrameFeatures(float[] frame)
        {
            // Apply Hamming window
            var windowedFrame = ApplyHammingWindow(frame);

            // Extract various features
            var features = new List<double>
            {
                // Energy
                ComputeEnergy(windowedFrame),
                // Zero Crossing Rate
                ComputeZeroCrossingRate(frame),
                // Spectral Centroid
                ComputeSpectralCentroid(windowedFrame),
                // Spectral Rolloff
                ComputeSpectralRolloff(windowedFrame),
                // RMS
                ComputeRMS(windowedFrame)
            };

            // Add spectral features
            var spectralFeatures = ComputeSpectralFeatures(windowedFrame);
            features.AddRange(spectralFeatures);

            return features.ToArray();
        }

        /// <summary>
        /// Apply Hamming window to frame
        /// </summary>
        private float[] ApplyHammingWindow(float[] frame)
        {
            var windowed = new float[frame.Length];
            for (int i = 0; i < frame.Length; i++)
            {
                double window = 0.54 - 0.46 * Math.Cos(2.0 * Math.PI * i / (frame.Length - 1));
                windowed[i] = frame[i] * (float)window;
            }
            return windowed;
        }

        /// <summary>
        /// Compute energy of frame
        /// </summary>
        private double ComputeEnergy(float[] frame)
        {
            double energy = 0;
            foreach (var sample in frame)
            {
                energy += sample * sample;
            }
            return Math.Log10(energy + 1e-10);
        }

        /// <summary>
        /// Compute zero crossing rate
        /// </summary>
        private double ComputeZeroCrossingRate(float[] frame)
        {
            int crossings = 0;
            for (int i = 1; i < frame.Length; i++)
            {
                if ((frame[i] >= 0 && frame[i - 1] < 0) || (frame[i] < 0 && frame[i - 1] >= 0))
                {
                    crossings++;
                }
            }
            return (double)crossings / frame.Length;
        }

        /// <summary>
        /// Compute spectral centroid
        /// </summary>
        private double ComputeSpectralCentroid(float[] frame)
        {
            var spectrum = ComputeFFTMagnitude(frame);

            double weightedSum = 0;
            double totalSum = 0;

            for (int i = 0; i < spectrum.Length; i++)
            {
                weightedSum += i * spectrum[i];
                totalSum += spectrum[i];
            }

            return totalSum > 0 ? weightedSum / totalSum : 0;
        }

        /// <summary>
        /// Compute spectral rolloff
        /// </summary>
        private double ComputeSpectralRolloff(float[] frame)
        {
            var spectrum = ComputeFFTMagnitude(frame);
            double totalEnergy = spectrum.Sum();
            double threshold = 0.85 * totalEnergy;

            double cumulativeEnergy = 0;
            for (int i = 0; i < spectrum.Length; i++)
            {
                cumulativeEnergy += spectrum[i];
                if (cumulativeEnergy >= threshold)
                {
                    return (double)i / spectrum.Length;
                }
            }

            return 1.0;
        }

        /// <summary>
        /// Compute RMS (Root Mean Square)
        /// </summary>
        private double ComputeRMS(float[] frame)
        {
            double sum = 0;
            foreach (var sample in frame)
            {
                sum += sample * sample;
            }
            return Math.Sqrt(sum / frame.Length);
        }

        /// <summary>
        /// Compute spectral features (first 10 frequency bins)
        /// </summary>
        private double[] ComputeSpectralFeatures(float[] frame)
        {
            var spectrum = ComputeFFTMagnitude(frame);
            int numBins = Math.Min(10, spectrum.Length);
            var features = new double[numBins];

            for (int i = 0; i < numBins; i++)
            {
                features[i] = Math.Log10(spectrum[i] + 1e-10);
            }

            return features;
        }

        /// <summary>
        /// Compute FFT magnitude spectrum
        /// </summary>
        private double[] ComputeFFTMagnitude(float[] frame)
        {
            int n = frame.Length;
            var real = new double[n];
            var imag = new double[n];

            // Copy frame to real part
            for (int i = 0; i < n; i++)
            {
                real[i] = frame[i];
            }

            // Simple DFT for demonstration (in production, use FFT library)
            var magnitude = new double[n / 2];
            for (int k = 0; k < n / 2; k++)
            {
                double realSum = 0;
                double imagSum = 0;

                for (int t = 0; t < n; t++)
                {
                    double angle = -2.0 * Math.PI * k * t / n;
                    realSum += real[t] * Math.Cos(angle);
                    imagSum += real[t] * Math.Sin(angle);
                }

                magnitude[k] = Math.Sqrt(realSum * realSum + imagSum * imagSum);
            }

            return magnitude;
        }
    }
}
