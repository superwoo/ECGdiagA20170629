using System;
using MachineLearning.Mixture;

namespace GaussianMixtureExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Gaussian Mixture Model Example ===\n");

            // Example 1: Simple 2D clustering
            Example1_Simple2DClustering();

            Console.WriteLine("\n" + new string('=', 50) + "\n");

            // Example 2: 3-component mixture
            Example2_ThreeComponentMixture();

            Console.WriteLine("\n" + new string('=', 50) + "\n");

            // Example 3: Sample generation
            Example3_SampleGeneration();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static void Example1_Simple2DClustering()
        {
            Console.WriteLine("Example 1: Simple 2D Clustering");
            Console.WriteLine("--------------------------------");

            // Create sample data: two Gaussian clusters
            double[][] X = new double[][]
            {
                new double[] { 1.0, 2.0 },
                new double[] { 1.5, 1.8 },
                new double[] { 1.2, 2.1 },
                new double[] { 0.8, 1.9 },
                new double[] { 1.3, 2.2 },
                new double[] { 5.0, 6.0 },
                new double[] { 5.5, 5.8 },
                new double[] { 5.2, 6.1 },
                new double[] { 4.8, 5.9 },
                new double[] { 5.3, 6.2 },
            };

            // Fit Gaussian Mixture with 2 components
            GaussianMixture gmm = new GaussianMixture(
                n_components: 2,
                covariance_type: "full",
                max_iter: 100,
                random_state: 42,
                verbose: true
            );

            gmm.Fit(X);

            Console.WriteLine($"\nConverged: {gmm.Converged}");
            Console.WriteLine($"Number of iterations: {gmm.NumIterations}");
            Console.WriteLine($"Log-likelihood: {gmm.LowerBound:F4}");

            // Print means
            Console.WriteLine("\nMeans:");
            for (int k = 0; k < gmm.Means.Length; k++)
            {
                Console.Write($"Component {k}: [");
                for (int i = 0; i < gmm.Means[k].Length; i++)
                {
                    Console.Write($"{gmm.Means[k][i]:F3}");
                    if (i < gmm.Means[k].Length - 1) Console.Write(", ");
                }
                Console.WriteLine("]");
            }

            // Print weights
            Console.WriteLine("\nWeights:");
            for (int k = 0; k < gmm.Weights.Length; k++)
            {
                Console.WriteLine($"Component {k}: {gmm.Weights[k]:F4}");
            }

            // Predict cluster labels
            int[] labels = gmm.Predict(X);
            Console.WriteLine("\nPredicted labels:");
            for (int i = 0; i < labels.Length; i++)
            {
                Console.WriteLine($"Sample {i}: [{X[i][0]:F1}, {X[i][1]:F1}] -> Cluster {labels[i]}");
            }

            // Compute score
            double score = gmm.Score(X);
            Console.WriteLine($"\nAverage log-likelihood: {score:F4}");
        }

        static void Example2_ThreeComponentMixture()
        {
            Console.WriteLine("Example 2: Three-Component Mixture");
            Console.WriteLine("-----------------------------------");

            // Create sample data: three Gaussian clusters
            double[][] X = new double[][]
            {
                // Cluster 1
                new double[] { 0.0, 0.0 },
                new double[] { 0.2, 0.1 },
                new double[] { 0.1, 0.2 },
                new double[] { -0.1, 0.0 },
                // Cluster 2
                new double[] { 3.0, 3.0 },
                new double[] { 3.2, 3.1 },
                new double[] { 3.1, 3.2 },
                new double[] { 2.9, 3.0 },
                // Cluster 3
                new double[] { 0.0, 3.0 },
                new double[] { 0.1, 3.2 },
                new double[] { 0.2, 3.1 },
                new double[] { 0.0, 2.9 },
            };

            // Fit Gaussian Mixture with 3 components
            GaussianMixture gmm = new GaussianMixture(
                n_components: 3,
                max_iter: 100,
                random_state: 42
            );

            gmm.Fit(X);

            Console.WriteLine($"Converged: {gmm.Converged}");
            Console.WriteLine($"Number of iterations: {gmm.NumIterations}");

            // Predict probabilities
            double[][] proba = gmm.PredictProba(X);
            Console.WriteLine("\nPosterior probabilities for first 3 samples:");
            for (int i = 0; i < Math.Min(3, X.Length); i++)
            {
                Console.Write($"Sample {i}: ");
                for (int k = 0; k < proba[i].Length; k++)
                {
                    Console.Write($"C{k}={proba[i][k]:F4} ");
                }
                Console.WriteLine();
            }

            // Score samples
            double[] scores = gmm.ScoreSamples(X);
            Console.WriteLine("\nLog-likelihood for first 3 samples:");
            for (int i = 0; i < Math.Min(3, X.Length); i++)
            {
                Console.WriteLine($"Sample {i}: {scores[i]:F4}");
            }
        }

        static void Example3_SampleGeneration()
        {
            Console.WriteLine("Example 3: Sample Generation");
            Console.WriteLine("----------------------------");

            // Create and fit a simple model
            double[][] X = new double[][]
            {
                new double[] { 1.0, 1.0 },
                new double[] { 1.1, 1.2 },
                new double[] { 0.9, 1.0 },
                new double[] { 5.0, 5.0 },
                new double[] { 5.1, 5.2 },
                new double[] { 4.9, 5.0 },
            };

            GaussianMixture gmm = new GaussianMixture(
                n_components: 2,
                random_state: 42
            );

            gmm.Fit(X);

            Console.WriteLine("Fitted model with 2 components");
            Console.WriteLine("\nGenerating 5 new samples:");

            // Generate new samples
            double[][] samples = gmm.Sample(5);
            for (int i = 0; i < samples.Length; i++)
            {
                Console.Write($"Sample {i}: [");
                for (int j = 0; j < samples[i].Length; j++)
                {
                    Console.Write($"{samples[i][j]:F3}");
                    if (j < samples[i].Length - 1) Console.Write(", ");
                }
                Console.WriteLine("]");
            }

            // Predict which component the new samples likely came from
            int[] predicted_labels = gmm.Predict(samples);
            Console.WriteLine("\nPredicted component for generated samples:");
            for (int i = 0; i < predicted_labels.Length; i++)
            {
                Console.WriteLine($"Sample {i}: Component {predicted_labels[i]}");
            }
        }
    }
}
