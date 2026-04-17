using Accord.MachineLearning;
using Accord.Statistics.Distributions.Multivariate;

namespace AnomalousSoundDetection
{
    /// <summary>
    /// GMM-based anomaly detector for sound analysis
    /// </summary>
    public class GMManomalyDetector
    {
        private GaussianMixtureModel? gmm;
        private double threshold;
        private readonly int numberOfComponents;
        private readonly AudioFeatureExtractor featureExtractor;

        public GMManomalyDetector(int numberOfComponents = 3, int sampleRate = 16000)
        {
            this.numberOfComponents = numberOfComponents;
            this.threshold = 0.0;
            this.featureExtractor = new AudioFeatureExtractor(sampleRate);
        }

        /// <summary>
        /// Train the GMM on normal sound data
        /// </summary>
        /// <param name="normalSamples">List of normal sound samples</param>
        /// <param name="percentile">Percentile for threshold (default 95%)</param>
        public void Train(List<float[]> normalSamples, double percentile = 95.0)
        {
            Console.WriteLine("Extracting features from training data...");

            // Extract features from all normal samples
            var allFeatures = new List<double[]>();
            foreach (var sample in normalSamples)
            {
                var features = featureExtractor.ExtractFeatures(sample);
                allFeatures.AddRange(features);
            }

            Console.WriteLine($"Extracted {allFeatures.Count} feature vectors");

            // Convert to array
            double[][] featureMatrix = allFeatures.ToArray();

            // Train GMM
            Console.WriteLine($"Training GMM with {numberOfComponents} components...");
            gmm = new GaussianMixtureModel(numberOfComponents);
            gmm.Learn(featureMatrix);

            Console.WriteLine("GMM training completed");

            // Calculate threshold based on training data
            SetThreshold(featureMatrix, percentile);
        }

        /// <summary>
        /// Set anomaly detection threshold based on training data
        /// </summary>
        private void SetThreshold(double[][] trainingData, double percentile)
        {
            if (gmm == null)
            {
                throw new InvalidOperationException("GMM must be trained before setting threshold");
            }

            Console.WriteLine("Calculating anomaly threshold...");

            // Calculate log-likelihoods for training data
            var logLikelihoods = new List<double>();
            foreach (var feature in trainingData)
            {
                double logLikelihood = ComputeLogLikelihood(feature);
                logLikelihoods.Add(logLikelihood);
            }

            // Sort and find percentile threshold
            logLikelihoods.Sort();
            int index = (int)((100.0 - percentile) / 100.0 * logLikelihoods.Count);
            threshold = logLikelihoods[Math.Max(0, Math.Min(index, logLikelihoods.Count - 1))];

            Console.WriteLine($"Threshold set to: {threshold:F4}");
        }

        /// <summary>
        /// Compute log likelihood for a feature vector
        /// </summary>
        private double ComputeLogLikelihood(double[] feature)
        {
            if (gmm == null)
            {
                throw new InvalidOperationException("GMM must be trained before computing likelihood");
            }

            // Manually compute likelihood using Gaussians collection
            double likelihood = 0;
            for (int i = 0; i < gmm.Gaussians.Count; i++)
            {
                var cluster = gmm.Gaussians[i];
                // Compute the Mahalanobis distance manually
                double[] mean = cluster.Mean;
                double[,] covariance = cluster.Covariance;

                // Compute probability using multivariate normal formula
                int d = feature.Length;
                double[] diff = new double[d];
                for (int j = 0; j < d; j++)
                {
                    diff[j] = feature[j] - mean[j];
                }

                // Simple probability approximation (assuming diagonal covariance for simplicity)
                double exponent = 0;
                for (int j = 0; j < d; j++)
                {
                    exponent += diff[j] * diff[j] / (covariance[j, j] + 1e-10);
                }

                double det = 1.0;
                for (int j = 0; j < d; j++)
                {
                    det *= covariance[j, j];
                }

                double prob = Math.Exp(-0.5 * exponent) / Math.Sqrt(Math.Pow(2 * Math.PI, d) * (det + 1e-10));
                likelihood += cluster.Proportion * prob;
            }

            return Math.Log(likelihood + 1e-300); // Add small constant to avoid log(0)
        }

        /// <summary>
        /// Detect if a sound sample is anomalous
        /// </summary>
        /// <param name="testSample">Audio sample to test</param>
        /// <returns>True if anomalous, false if normal</returns>
        public bool IsAnomalous(float[] testSample)
        {
            if (gmm == null)
            {
                throw new InvalidOperationException("GMM must be trained before detection");
            }

            var anomalyScore = GetAnomalyScore(testSample);
            return anomalyScore < threshold;
        }

        /// <summary>
        /// Get anomaly score for a sound sample (lower = more anomalous)
        /// </summary>
        /// <param name="testSample">Audio sample to test</param>
        /// <returns>Anomaly score (average log-likelihood)</returns>
        public double GetAnomalyScore(float[] testSample)
        {
            if (gmm == null)
            {
                throw new InvalidOperationException("GMM must be trained before scoring");
            }

            // Extract features
            var features = featureExtractor.ExtractFeatures(testSample);

            // Calculate average log-likelihood
            double totalLogLikelihood = 0;
            foreach (var feature in features)
            {
                totalLogLikelihood += ComputeLogLikelihood(feature);
            }

            return totalLogLikelihood / features.Length;
        }

        /// <summary>
        /// Get detailed anomaly information
        /// </summary>
        /// <param name="testSample">Audio sample to test</param>
        /// <returns>Tuple of (isAnomalous, anomalyScore, threshold)</returns>
        public (bool IsAnomalous, double AnomalyScore, double Threshold) GetDetailedResult(float[] testSample)
        {
            if (gmm == null)
            {
                throw new InvalidOperationException("GMM must be trained before detection");
            }

            var anomalyScore = GetAnomalyScore(testSample);
            bool isAnomalous = anomalyScore < threshold;

            return (isAnomalous, anomalyScore, threshold);
        }

        /// <summary>
        /// Get the trained GMM model
        /// </summary>
        public GaussianMixtureModel? GetModel()
        {
            return gmm;
        }

        /// <summary>
        /// Get current threshold
        /// </summary>
        public double GetThreshold()
        {
            return threshold;
        }
    }
}
