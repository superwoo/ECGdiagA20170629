using AnomalousSoundDetection;

namespace AnomalousSoundDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Anomalous Sound Detection using GMM ===");
            Console.WriteLine();

            // Create synthetic audio data for demonstration
            Console.WriteLine("Generating synthetic audio data...");
            var normalSamples = GenerateNormalSamples(50, 16000, 1.0);
            var anomalousSamples = GenerateAnomalouseSamples(10, 16000, 1.0);

            Console.WriteLine($"Generated {normalSamples.Count} normal samples");
            Console.WriteLine($"Generated {anomalousSamples.Count} anomalous samples");
            Console.WriteLine();

            // Create and train the anomaly detector
            Console.WriteLine("Training GMM anomaly detector...");
            var detector = new GMManomalyDetector(numberOfComponents: 3, sampleRate: 16000);
            detector.Train(normalSamples, percentile: 95.0);
            Console.WriteLine();

            // Test on normal samples
            Console.WriteLine("Testing on normal samples:");
            int normalCorrect = 0;
            for (int i = 0; i < 5; i++)
            {
                var result = detector.GetDetailedResult(normalSamples[i]);
                Console.WriteLine($"  Sample {i + 1}: Score={result.AnomalyScore:F4}, " +
                                $"Threshold={result.Threshold:F4}, " +
                                $"Anomalous={result.IsAnomalous}");
                if (!result.IsAnomalous) normalCorrect++;
            }
            Console.WriteLine($"Normal samples classified correctly: {normalCorrect}/5");
            Console.WriteLine();

            // Test on anomalous samples
            Console.WriteLine("Testing on anomalous samples:");
            int anomalousCorrect = 0;
            for (int i = 0; i < anomalousSamples.Count; i++)
            {
                var result = detector.GetDetailedResult(anomalousSamples[i]);
                Console.WriteLine($"  Sample {i + 1}: Score={result.AnomalyScore:F4}, " +
                                $"Threshold={result.Threshold:F4}, " +
                                $"Anomalous={result.IsAnomalous}");
                if (result.IsAnomalous) anomalousCorrect++;
            }
            Console.WriteLine($"Anomalous samples detected correctly: {anomalousCorrect}/{anomalousSamples.Count}");
            Console.WriteLine();

            // Summary
            Console.WriteLine("=== Detection Summary ===");
            double accuracy = (normalCorrect + anomalousCorrect) / (5.0 + anomalousSamples.Count) * 100;
            Console.WriteLine($"Overall accuracy: {accuracy:F2}%");
            Console.WriteLine();

            Console.WriteLine("Demo completed successfully!");
        }

        /// <summary>
        /// Generate normal sound samples (sine waves with slight variations)
        /// </summary>
        static List<float[]> GenerateNormalSamples(int count, int sampleRate, double duration)
        {
            var samples = new List<float[]>();
            var random = new Random(42);

            for (int i = 0; i < count; i++)
            {
                int length = (int)(sampleRate * duration);
                var sample = new float[length];

                // Base frequency around 440 Hz (A4 note)
                double frequency = 440.0 + random.NextDouble() * 50 - 25;
                double amplitude = 0.3 + random.NextDouble() * 0.2;

                for (int t = 0; t < length; t++)
                {
                    double time = (double)t / sampleRate;
                    // Sine wave with small noise
                    sample[t] = (float)(amplitude * Math.Sin(2 * Math.PI * frequency * time) +
                                      (random.NextDouble() - 0.5) * 0.05);
                }

                samples.Add(sample);
            }

            return samples;
        }

        /// <summary>
        /// Generate anomalous sound samples (different characteristics)
        /// </summary>
        static List<float[]> GenerateAnomalouseSamples(int count, int sampleRate, double duration)
        {
            var samples = new List<float[]>();
            var random = new Random(123);

            for (int i = 0; i < count; i++)
            {
                int length = (int)(sampleRate * duration);
                var sample = new float[length];

                switch (i % 3)
                {
                    case 0:
                        // High frequency noise
                        for (int t = 0; t < length; t++)
                        {
                            sample[t] = (float)((random.NextDouble() - 0.5) * 0.8);
                        }
                        break;

                    case 1:
                        // Very different frequency (much higher)
                        double highFreq = 1500.0 + random.NextDouble() * 500;
                        for (int t = 0; t < length; t++)
                        {
                            double time = (double)t / sampleRate;
                            sample[t] = (float)(0.6 * Math.Sin(2 * Math.PI * highFreq * time));
                        }
                        break;

                    case 2:
                        // Impulse/click sounds
                        for (int t = 0; t < length; t++)
                        {
                            if (t % 1000 == 0)
                            {
                                sample[t] = (float)(random.NextDouble() * 0.9);
                            }
                            else
                            {
                                sample[t] = (float)((random.NextDouble() - 0.5) * 0.1);
                            }
                        }
                        break;
                }

                samples.Add(sample);
            }

            return samples;
        }
    }
}
