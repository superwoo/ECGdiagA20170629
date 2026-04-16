using System;
using System.Diagnostics;
using MachineLearning.Mixture;

namespace GaussianMixturePerformanceTest
{
    class PerformanceComparison
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Gaussian Mixture Model Performance Comparison ===");
            Console.WriteLine("Comparing Original vs SIMD-Optimized Implementation\n");

            // Test with different dataset sizes
            int[] datasetSizes = { 100, 500, 1000, 5000 };
            int[] featureSizes = { 2, 5, 10, 20 };

            Console.WriteLine("Running performance tests...\n");

            foreach (int n_features in featureSizes)
            {
                Console.WriteLine($"\n{'=',60}");
                Console.WriteLine($"Feature Dimension: {n_features}");
                Console.WriteLine($"{'=',60}\n");

                foreach (int n_samples in datasetSizes)
                {
                    Console.WriteLine($"Dataset: {n_samples} samples × {n_features} features");

                    // Generate synthetic data
                    double[][] X = GenerateSyntheticData(n_samples, n_features, seed: 42);

                    // Test original implementation
                    long originalTime = TestOriginalImplementation(X);

                    // Test optimized implementation
                    long optimizedTime = TestOptimizedImplementation(X);

                    // Calculate speedup
                    double speedup = (double)originalTime / optimizedTime;

                    Console.WriteLine($"  Original:  {originalTime,6} ms");
                    Console.WriteLine($"  Optimized: {optimizedTime,6} ms");
                    Console.WriteLine($"  Speedup:   {speedup,6:F2}x faster");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("\n{'=',60}");
            Console.WriteLine("Performance Test Complete");
            Console.WriteLine($"{'=',60}\n");

            // Detailed comparison example
            DetailedComparisonExample();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static long TestOriginalImplementation(double[][] X)
        {
            Stopwatch sw = Stopwatch.StartNew();

            GaussianMixture gmm = new GaussianMixture(
                n_components: 3,
                max_iter: 50,
                random_state: 42,
                verbose: false
            );

            gmm.Fit(X);
            int[] labels = gmm.Predict(X);

            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        static long TestOptimizedImplementation(double[][] X)
        {
            Stopwatch sw = Stopwatch.StartNew();

            GaussianMixtureOptimized gmm = new GaussianMixtureOptimized(
                n_components: 3,
                max_iter: 50,
                random_state: 42,
                verbose: false
            );

            gmm.Fit(X);
            int[] labels = gmm.Predict(X);

            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        static void DetailedComparisonExample()
        {
            Console.WriteLine("=== Detailed Comparison Example ===\n");

            int n_samples = 1000;
            int n_features = 10;
            double[][] X = GenerateSyntheticData(n_samples, n_features, seed: 42);

            Console.WriteLine($"Dataset: {n_samples} samples, {n_features} features");
            Console.WriteLine($"Components: 3\n");

            // Original implementation
            Console.WriteLine("Running Original Implementation...");
            Stopwatch sw1 = Stopwatch.StartNew();
            GaussianMixture gmm1 = new GaussianMixture(
                n_components: 3,
                max_iter: 100,
                random_state: 42,
                verbose: false
            );
            gmm1.Fit(X);
            double score1 = gmm1.Score(X);
            sw1.Stop();

            Console.WriteLine($"  Converged: {gmm1.Converged}");
            Console.WriteLine($"  Iterations: {gmm1.NumIterations}");
            Console.WriteLine($"  Score: {score1:F4}");
            Console.WriteLine($"  Time: {sw1.ElapsedMilliseconds} ms\n");

            // Optimized implementation
            Console.WriteLine("Running SIMD-Optimized Implementation...");
            Stopwatch sw2 = Stopwatch.StartNew();
            GaussianMixtureOptimized gmm2 = new GaussianMixtureOptimized(
                n_components: 3,
                max_iter: 100,
                random_state: 42,
                verbose: false
            );
            gmm2.Fit(X);
            double score2 = gmm2.Score(X);
            sw2.Stop();

            Console.WriteLine($"  Converged: {gmm2.Converged}");
            Console.WriteLine($"  Iterations: {gmm2.NumIterations}");
            Console.WriteLine($"  Score: {score2:F4}");
            Console.WriteLine($"  Time: {sw2.ElapsedMilliseconds} ms\n");

            // Compare results
            Console.WriteLine("Comparison:");
            Console.WriteLine($"  Score difference: {Math.Abs(score1 - score2):E6}");
            Console.WriteLine($"  Speedup: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F2}x");
            Console.WriteLine($"  Time saved: {sw1.ElapsedMilliseconds - sw2.ElapsedMilliseconds} ms");
        }

        static double[][] GenerateSyntheticData(int n_samples, int n_features, int seed = 42)
        {
            Random rand = new Random(seed);
            double[][] X = new double[n_samples][];

            // Generate 3 clusters
            int samples_per_cluster = n_samples / 3;

            for (int i = 0; i < n_samples; i++)
            {
                X[i] = new double[n_features];
                int cluster = i / samples_per_cluster;
                if (cluster > 2) cluster = 2;

                double[] center = new double[n_features];
                for (int j = 0; j < n_features; j++)
                {
                    center[j] = cluster * 5.0;
                }

                for (int j = 0; j < n_features; j++)
                {
                    // Box-Muller transform for Gaussian noise
                    double u1 = rand.NextDouble();
                    double u2 = rand.NextDouble();
                    double noise = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                    X[i][j] = center[j] + noise;
                }
            }

            return X;
        }
    }
}
