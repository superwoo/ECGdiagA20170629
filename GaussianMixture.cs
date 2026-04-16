using System;
using System.Linq;

namespace MachineLearning.Mixture
{
    /// <summary>
    /// Gaussian Mixture Model implementation similar to sklearn.mixture.GaussianMixture
    /// </summary>
    public class GaussianMixture
    {
        private int n_components;
        private string covariance_type;
        private double tol;
        private int max_iter;
        private int n_init;
        private string init_params;
        private int random_state;
        private bool verbose;

        // Fitted parameters
        private double[] weights_;  // Mixture weights
        private double[][] means_;  // Means of each mixture component
        private double[][][] covariances_;  // Covariance matrices
        private double[][][] precisions_cholesky_;  // Cholesky decomposition of precision matrices
        private bool converged_;
        private int n_iter_;
        private double lower_bound_;

        private Random random;

        /// <summary>
        /// Initialize Gaussian Mixture Model
        /// </summary>
        /// <param name="n_components">Number of mixture components</param>
        /// <param name="covariance_type">Type of covariance parameters (full, tied, diag, spherical)</param>
        /// <param name="tol">Convergence threshold</param>
        /// <param name="max_iter">Maximum number of EM iterations</param>
        /// <param name="n_init">Number of initializations to perform</param>
        /// <param name="init_params">Method for initialization (kmeans, random)</param>
        /// <param name="random_state">Random seed for reproducibility</param>
        /// <param name="verbose">Enable verbose output</param>
        public GaussianMixture(
            int n_components = 1,
            string covariance_type = "full",
            double tol = 1e-3,
            int max_iter = 100,
            int n_init = 1,
            string init_params = "kmeans",
            int random_state = -1,
            bool verbose = false)
        {
            this.n_components = n_components;
            this.covariance_type = covariance_type;
            this.tol = tol;
            this.max_iter = max_iter;
            this.n_init = n_init;
            this.init_params = init_params;
            this.random_state = random_state;
            this.verbose = verbose;

            this.random = random_state >= 0 ? new Random(random_state) : new Random();
        }

        /// <summary>
        /// Fit the Gaussian Mixture Model to the data
        /// </summary>
        public void Fit(double[][] X)
        {
            int n_samples = X.Length;
            int n_features = X[0].Length;

            double best_lower_bound = double.NegativeInfinity;

            for (int init = 0; init < n_init; init++)
            {
                // Initialize parameters
                InitializeParameters(X, n_features);

                // Run EM algorithm
                double lower_bound = double.NegativeInfinity;
                converged_ = false;

                for (int iter = 0; iter < max_iter; iter++)
                {
                    double prev_lower_bound = lower_bound;

                    // E-step: compute responsibilities
                    double[][] responsibilities = EStep(X);

                    // M-step: update parameters
                    MStep(X, responsibilities);

                    // Compute log-likelihood
                    lower_bound = ComputeLowerBound(X, responsibilities);

                    // Check convergence
                    double change = lower_bound - prev_lower_bound;
                    if (verbose)
                    {
                        Console.WriteLine($"Iteration {iter + 1}: log-likelihood = {lower_bound:F4}, change = {change:F6}");
                    }

                    if (Math.Abs(change) < tol)
                    {
                        converged_ = true;
                        n_iter_ = iter + 1;
                        break;
                    }
                }

                if (!converged_)
                {
                    n_iter_ = max_iter;
                }

                // Keep the best result across multiple initializations
                if (lower_bound > best_lower_bound)
                {
                    best_lower_bound = lower_bound;
                    lower_bound_ = lower_bound;
                }
            }
        }

        /// <summary>
        /// Initialize parameters using k-means++ or random initialization
        /// </summary>
        private void InitializeParameters(double[][] X, int n_features)
        {
            int n_samples = X.Length;

            // Initialize weights uniformly
            weights_ = new double[n_components];
            for (int k = 0; k < n_components; k++)
            {
                weights_[k] = 1.0 / n_components;
            }

            // Initialize means
            means_ = new double[n_components][];

            if (init_params == "kmeans")
            {
                // Simple k-means++ initialization
                InitializeKMeansPlusPlus(X, n_features);
            }
            else
            {
                // Random initialization
                for (int k = 0; k < n_components; k++)
                {
                    int idx = random.Next(n_samples);
                    means_[k] = (double[])X[idx].Clone();
                }
            }

            // Initialize covariances to identity
            covariances_ = new double[n_components][][];
            for (int k = 0; k < n_components; k++)
            {
                covariances_[k] = new double[n_features][];
                for (int i = 0; i < n_features; i++)
                {
                    covariances_[k][i] = new double[n_features];
                    covariances_[k][i][i] = 1.0;
                }
            }
        }

        /// <summary>
        /// K-means++ initialization for means
        /// </summary>
        private void InitializeKMeansPlusPlus(double[][] X, int n_features)
        {
            int n_samples = X.Length;

            // Choose first center randomly
            int first_idx = random.Next(n_samples);
            means_[0] = (double[])X[first_idx].Clone();

            // Choose remaining centers
            for (int k = 1; k < n_components; k++)
            {
                double[] distances = new double[n_samples];
                double sum_distances = 0;

                // Compute distance to nearest existing center
                for (int i = 0; i < n_samples; i++)
                {
                    double min_dist = double.MaxValue;
                    for (int j = 0; j < k; j++)
                    {
                        double dist = EuclideanDistanceSquared(X[i], means_[j]);
                        if (dist < min_dist)
                        {
                            min_dist = dist;
                        }
                    }
                    distances[i] = min_dist;
                    sum_distances += min_dist;
                }

                // Choose next center with probability proportional to distance squared
                double rand_val = random.NextDouble() * sum_distances;
                double cumsum = 0;
                int selected_idx = 0;

                for (int i = 0; i < n_samples; i++)
                {
                    cumsum += distances[i];
                    if (cumsum >= rand_val)
                    {
                        selected_idx = i;
                        break;
                    }
                }

                means_[k] = (double[])X[selected_idx].Clone();
            }
        }

        /// <summary>
        /// E-step: Compute responsibilities (posterior probabilities)
        /// </summary>
        private double[][] EStep(double[][] X)
        {
            int n_samples = X.Length;
            double[][] responsibilities = new double[n_samples][];

            for (int i = 0; i < n_samples; i++)
            {
                responsibilities[i] = new double[n_components];
                double[] log_prob = new double[n_components];
                double max_log_prob = double.NegativeInfinity;

                // Compute log probability for each component
                for (int k = 0; k < n_components; k++)
                {
                    log_prob[k] = Math.Log(weights_[k]) + LogGaussianProbability(X[i], means_[k], covariances_[k]);
                    if (log_prob[k] > max_log_prob)
                    {
                        max_log_prob = log_prob[k];
                    }
                }

                // Normalize using log-sum-exp trick for numerical stability
                double sum_exp = 0;
                for (int k = 0; k < n_components; k++)
                {
                    responsibilities[i][k] = Math.Exp(log_prob[k] - max_log_prob);
                    sum_exp += responsibilities[i][k];
                }

                for (int k = 0; k < n_components; k++)
                {
                    responsibilities[i][k] /= sum_exp;
                }
            }

            return responsibilities;
        }

        /// <summary>
        /// M-step: Update parameters based on responsibilities
        /// </summary>
        private void MStep(double[][] X, double[][] responsibilities)
        {
            int n_samples = X.Length;
            int n_features = X[0].Length;

            double reg_covar = 1e-6;  // Regularization for covariance

            // Update weights and means
            for (int k = 0; k < n_components; k++)
            {
                // Compute effective number of points assigned to component k
                double nk = 0;
                for (int i = 0; i < n_samples; i++)
                {
                    nk += responsibilities[i][k];
                }

                // Update weight
                weights_[k] = nk / n_samples;

                // Update mean
                double[] new_mean = new double[n_features];
                for (int i = 0; i < n_samples; i++)
                {
                    for (int j = 0; j < n_features; j++)
                    {
                        new_mean[j] += responsibilities[i][k] * X[i][j];
                    }
                }

                for (int j = 0; j < n_features; j++)
                {
                    new_mean[j] /= nk;
                }
                means_[k] = new_mean;

                // Update covariance
                double[][] new_covariance = new double[n_features][];
                for (int i = 0; i < n_features; i++)
                {
                    new_covariance[i] = new double[n_features];
                }

                for (int i = 0; i < n_samples; i++)
                {
                    double[] diff = new double[n_features];
                    for (int j = 0; j < n_features; j++)
                    {
                        diff[j] = X[i][j] - means_[k][j];
                    }

                    for (int j1 = 0; j1 < n_features; j1++)
                    {
                        for (int j2 = 0; j2 < n_features; j2++)
                        {
                            new_covariance[j1][j2] += responsibilities[i][k] * diff[j1] * diff[j2];
                        }
                    }
                }

                for (int j1 = 0; j1 < n_features; j1++)
                {
                    for (int j2 = 0; j2 < n_features; j2++)
                    {
                        new_covariance[j1][j2] /= nk;
                    }
                    // Add regularization to diagonal
                    new_covariance[j1][j1] += reg_covar;
                }

                covariances_[k] = new_covariance;
            }
        }

        /// <summary>
        /// Compute log-likelihood lower bound
        /// </summary>
        private double ComputeLowerBound(double[][] X, double[][] responsibilities)
        {
            int n_samples = X.Length;
            double log_likelihood = 0;

            for (int i = 0; i < n_samples; i++)
            {
                double log_prob_sum = 0;

                for (int k = 0; k < n_components; k++)
                {
                    double log_prob = Math.Log(weights_[k]) + LogGaussianProbability(X[i], means_[k], covariances_[k]);
                    log_prob_sum += Math.Exp(log_prob);
                }

                log_likelihood += Math.Log(log_prob_sum);
            }

            return log_likelihood;
        }

        /// <summary>
        /// Compute log probability of a point under a Gaussian distribution
        /// </summary>
        private double LogGaussianProbability(double[] x, double[] mean, double[][] covariance)
        {
            int n_features = x.Length;

            // Compute (x - mean)^T * Sigma^-1 * (x - mean)
            double[] diff = new double[n_features];
            for (int i = 0; i < n_features; i++)
            {
                diff[i] = x[i] - mean[i];
            }

            // Compute inverse of covariance and its determinant
            double[][] inv_cov = InvertMatrix(covariance);
            double det_cov = DeterminantMatrix(covariance);

            if (det_cov <= 0)
            {
                det_cov = 1e-10;  // Prevent log(0)
            }

            double mahalanobis = 0;
            for (int i = 0; i < n_features; i++)
            {
                for (int j = 0; j < n_features; j++)
                {
                    mahalanobis += diff[i] * inv_cov[i][j] * diff[j];
                }
            }

            // Log probability: -0.5 * (k*log(2π) + log(det) + mahalanobis)
            double log_prob = -0.5 * (n_features * Math.Log(2 * Math.PI) + Math.Log(det_cov) + mahalanobis);

            return log_prob;
        }

        /// <summary>
        /// Predict cluster labels for samples
        /// </summary>
        public int[] Predict(double[][] X)
        {
            double[][] responsibilities = EStep(X);
            int n_samples = X.Length;
            int[] labels = new int[n_samples];

            for (int i = 0; i < n_samples; i++)
            {
                int max_idx = 0;
                double max_resp = responsibilities[i][0];

                for (int k = 1; k < n_components; k++)
                {
                    if (responsibilities[i][k] > max_resp)
                    {
                        max_resp = responsibilities[i][k];
                        max_idx = k;
                    }
                }

                labels[i] = max_idx;
            }

            return labels;
        }

        /// <summary>
        /// Predict posterior probability of each component for samples
        /// </summary>
        public double[][] PredictProba(double[][] X)
        {
            return EStep(X);
        }

        /// <summary>
        /// Compute the per-sample average log-likelihood
        /// </summary>
        public double Score(double[][] X)
        {
            int n_samples = X.Length;
            double total_log_likelihood = 0;

            for (int i = 0; i < n_samples; i++)
            {
                double log_prob_sum = double.NegativeInfinity;
                double max_log_prob = double.NegativeInfinity;

                // Find max for numerical stability
                double[] log_probs = new double[n_components];
                for (int k = 0; k < n_components; k++)
                {
                    log_probs[k] = Math.Log(weights_[k]) + LogGaussianProbability(X[i], means_[k], covariances_[k]);
                    if (log_probs[k] > max_log_prob)
                    {
                        max_log_prob = log_probs[k];
                    }
                }

                // Log-sum-exp
                double sum_exp = 0;
                for (int k = 0; k < n_components; k++)
                {
                    sum_exp += Math.Exp(log_probs[k] - max_log_prob);
                }
                log_prob_sum = max_log_prob + Math.Log(sum_exp);

                total_log_likelihood += log_prob_sum;
            }

            return total_log_likelihood / n_samples;
        }

        /// <summary>
        /// Compute the weighted log probabilities for each sample
        /// </summary>
        public double[] ScoreSamples(double[][] X)
        {
            int n_samples = X.Length;
            double[] scores = new double[n_samples];

            for (int i = 0; i < n_samples; i++)
            {
                double max_log_prob = double.NegativeInfinity;
                double[] log_probs = new double[n_components];

                for (int k = 0; k < n_components; k++)
                {
                    log_probs[k] = Math.Log(weights_[k]) + LogGaussianProbability(X[i], means_[k], covariances_[k]);
                    if (log_probs[k] > max_log_prob)
                    {
                        max_log_prob = log_probs[k];
                    }
                }

                double sum_exp = 0;
                for (int k = 0; k < n_components; k++)
                {
                    sum_exp += Math.Exp(log_probs[k] - max_log_prob);
                }

                scores[i] = max_log_prob + Math.Log(sum_exp);
            }

            return scores;
        }

        /// <summary>
        /// Generate random samples from the fitted Gaussian mixture
        /// </summary>
        public double[][] Sample(int n_samples)
        {
            double[][] samples = new double[n_samples][];
            int n_features = means_[0].Length;

            for (int i = 0; i < n_samples; i++)
            {
                // Choose a component based on weights
                double rand_val = random.NextDouble();
                double cumsum = 0;
                int component = 0;

                for (int k = 0; k < n_components; k++)
                {
                    cumsum += weights_[k];
                    if (cumsum >= rand_val)
                    {
                        component = k;
                        break;
                    }
                }

                // Generate sample from chosen component
                samples[i] = SampleFromGaussian(means_[component], covariances_[component]);
            }

            return samples;
        }

        /// <summary>
        /// Generate a sample from a multivariate Gaussian distribution
        /// </summary>
        private double[] SampleFromGaussian(double[] mean, double[][] covariance)
        {
            int n_features = mean.Length;

            // Generate standard normal samples using Box-Muller transform
            double[] z = new double[n_features];
            for (int i = 0; i < n_features; i += 2)
            {
                double u1 = random.NextDouble();
                double u2 = random.NextDouble();

                z[i] = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                if (i + 1 < n_features)
                {
                    z[i + 1] = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                }
            }

            // Compute Cholesky decomposition of covariance
            double[][] L = CholeskyDecomposition(covariance);

            // Transform: x = mean + L * z
            double[] sample = new double[n_features];
            for (int i = 0; i < n_features; i++)
            {
                sample[i] = mean[i];
                for (int j = 0; j <= i; j++)
                {
                    sample[i] += L[i][j] * z[j];
                }
            }

            return sample;
        }

        // Properties to access fitted parameters
        public double[] Weights => weights_;
        public double[][] Means => means_;
        public double[][][] Covariances => covariances_;
        public bool Converged => converged_;
        public int NumIterations => n_iter_;
        public double LowerBound => lower_bound_;

        // ===== Matrix operations =====

        private double EuclideanDistanceSquared(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                double diff = a[i] - b[i];
                sum += diff * diff;
            }
            return sum;
        }

        /// <summary>
        /// Compute matrix inverse using Gauss-Jordan elimination
        /// </summary>
        private double[][] InvertMatrix(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] result = new double[n][];
            double[][] temp = new double[n][];

            // Initialize result as identity and temp as copy of matrix
            for (int i = 0; i < n; i++)
            {
                result[i] = new double[n];
                temp[i] = new double[n];
                result[i][i] = 1.0;
                for (int j = 0; j < n; j++)
                {
                    temp[i][j] = matrix[i][j];
                }
            }

            // Gauss-Jordan elimination
            for (int i = 0; i < n; i++)
            {
                // Find pivot
                double pivot = temp[i][i];
                if (Math.Abs(pivot) < 1e-10)
                {
                    pivot = 1e-10;  // Regularization
                }

                // Scale row
                for (int j = 0; j < n; j++)
                {
                    temp[i][j] /= pivot;
                    result[i][j] /= pivot;
                }

                // Eliminate column
                for (int k = 0; k < n; k++)
                {
                    if (k != i)
                    {
                        double factor = temp[k][i];
                        for (int j = 0; j < n; j++)
                        {
                            temp[k][j] -= factor * temp[i][j];
                            result[k][j] -= factor * result[i][j];
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Compute matrix determinant using LU decomposition
        /// </summary>
        private double DeterminantMatrix(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] temp = new double[n][];

            // Copy matrix
            for (int i = 0; i < n; i++)
            {
                temp[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    temp[i][j] = matrix[i][j];
                }
            }

            double det = 1.0;

            // LU decomposition
            for (int i = 0; i < n; i++)
            {
                // Find pivot
                int pivot_row = i;
                double max_val = Math.Abs(temp[i][i]);
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(temp[k][i]) > max_val)
                    {
                        max_val = Math.Abs(temp[k][i]);
                        pivot_row = k;
                    }
                }

                // Swap rows if needed
                if (pivot_row != i)
                {
                    double[] temp_row = temp[i];
                    temp[i] = temp[pivot_row];
                    temp[pivot_row] = temp_row;
                    det *= -1;
                }

                double pivot = temp[i][i];
                if (Math.Abs(pivot) < 1e-10)
                {
                    return 1e-10;  // Nearly singular
                }

                det *= pivot;

                // Eliminate column
                for (int k = i + 1; k < n; k++)
                {
                    double factor = temp[k][i] / pivot;
                    for (int j = i; j < n; j++)
                    {
                        temp[k][j] -= factor * temp[i][j];
                    }
                }
            }

            return det;
        }

        /// <summary>
        /// Compute Cholesky decomposition of a positive definite matrix
        /// </summary>
        private double[][] CholeskyDecomposition(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] L = new double[n][];

            for (int i = 0; i < n; i++)
            {
                L[i] = new double[n];
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    double sum = 0;

                    if (j == i)
                    {
                        for (int k = 0; k < j; k++)
                        {
                            sum += L[j][k] * L[j][k];
                        }
                        double val = matrix[j][j] - sum;
                        L[j][j] = val > 0 ? Math.Sqrt(val) : 1e-10;
                    }
                    else
                    {
                        for (int k = 0; k < j; k++)
                        {
                            sum += L[i][k] * L[j][k];
                        }
                        L[i][j] = (matrix[i][j] - sum) / L[j][j];
                    }
                }
            }

            return L;
        }
    }
}
