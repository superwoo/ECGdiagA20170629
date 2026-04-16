using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace MachineLearning.Mixture
{
    /// <summary>
    /// Optimized Gaussian Mixture Model using System.Numerics.Vector for SIMD operations
    /// Provides significant performance improvements for vector operations
    /// </summary>
    public class GaussianMixtureOptimized
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
        private double[] weights_;
        private double[][] means_;
        private double[][][] covariances_;
        private bool converged_;
        private int n_iter_;
        private double lower_bound_;

        private Random random;

        public GaussianMixtureOptimized(
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

        public void Fit(double[][] X)
        {
            int n_samples = X.Length;
            int n_features = X[0].Length;

            double best_lower_bound = double.NegativeInfinity;

            for (int init = 0; init < n_init; init++)
            {
                InitializeParameters(X, n_features);

                double lower_bound = double.NegativeInfinity;
                converged_ = false;

                for (int iter = 0; iter < max_iter; iter++)
                {
                    double prev_lower_bound = lower_bound;

                    double[][] responsibilities = EStep(X);
                    MStep(X, responsibilities);
                    lower_bound = ComputeLowerBound(X, responsibilities);

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

                if (lower_bound > best_lower_bound)
                {
                    best_lower_bound = lower_bound;
                    lower_bound_ = lower_bound;
                }
            }
        }

        private void InitializeParameters(double[][] X, int n_features)
        {
            int n_samples = X.Length;

            weights_ = new double[n_components];
            for (int k = 0; k < n_components; k++)
            {
                weights_[k] = 1.0 / n_components;
            }

            means_ = new double[n_components][];

            if (init_params == "kmeans")
            {
                InitializeKMeansPlusPlus(X, n_features);
            }
            else
            {
                for (int k = 0; k < n_components; k++)
                {
                    int idx = random.Next(n_samples);
                    means_[k] = (double[])X[idx].Clone();
                }
            }

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

        private void InitializeKMeansPlusPlus(double[][] X, int n_features)
        {
            int n_samples = X.Length;

            int first_idx = random.Next(n_samples);
            means_[0] = (double[])X[first_idx].Clone();

            for (int k = 1; k < n_components; k++)
            {
                double[] distances = new double[n_samples];
                double sum_distances = 0;

                for (int i = 0; i < n_samples; i++)
                {
                    double min_dist = double.MaxValue;
                    for (int j = 0; j < k; j++)
                    {
                        double dist = EuclideanDistanceSquaredSIMD(X[i], means_[j]);
                        if (dist < min_dist)
                        {
                            min_dist = dist;
                        }
                    }
                    distances[i] = min_dist;
                    sum_distances += min_dist;
                }

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

        private double[][] EStep(double[][] X)
        {
            int n_samples = X.Length;
            double[][] responsibilities = new double[n_samples][];

            for (int i = 0; i < n_samples; i++)
            {
                responsibilities[i] = new double[n_components];
                double[] log_prob = new double[n_components];
                double max_log_prob = double.NegativeInfinity;

                for (int k = 0; k < n_components; k++)
                {
                    log_prob[k] = Math.Log(weights_[k]) + LogGaussianProbability(X[i], means_[k], covariances_[k]);
                    if (log_prob[k] > max_log_prob)
                    {
                        max_log_prob = log_prob[k];
                    }
                }

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

        private void MStep(double[][] X, double[][] responsibilities)
        {
            int n_samples = X.Length;
            int n_features = X[0].Length;
            double reg_covar = 1e-6;

            for (int k = 0; k < n_components; k++)
            {
                // Compute effective number of points
                double nk = 0;
                for (int i = 0; i < n_samples; i++)
                {
                    nk += responsibilities[i][k];
                }

                weights_[k] = nk / n_samples;

                // Update mean using SIMD
                double[] new_mean = new double[n_features];
                UpdateMeanSIMD(X, responsibilities, k, new_mean, nk);
                means_[k] = new_mean;

                // Update covariance using optimized computation
                double[][] new_covariance = new double[n_features][];
                for (int i = 0; i < n_features; i++)
                {
                    new_covariance[i] = new double[n_features];
                }

                UpdateCovarianceSIMD(X, responsibilities, k, new_mean, new_covariance, nk);

                // Add regularization
                for (int j = 0; j < n_features; j++)
                {
                    new_covariance[j][j] += reg_covar;
                }

                covariances_[k] = new_covariance;
            }
        }

        /// <summary>
        /// Optimized mean update using SIMD operations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMeanSIMD(double[][] X, double[][] responsibilities, int k, double[] new_mean, double nk)
        {
            int n_samples = X.Length;
            int n_features = new_mean.Length;

            // Use Vector<double> for SIMD operations when possible
            int vectorSize = Vector<double>.Count;
            int vectorizedLength = n_features - (n_features % vectorSize);

            for (int i = 0; i < n_samples; i++)
            {
                double resp = responsibilities[i][k];

                // Vectorized portion
                int j = 0;
                for (; j < vectorizedLength; j += vectorSize)
                {
                    var xVec = new Vector<double>(X[i], j);
                    var meanVec = new Vector<double>(new_mean, j);
                    var result = meanVec + xVec * resp;
                    result.CopyTo(new_mean, j);
                }

                // Handle remaining elements
                for (; j < n_features; j++)
                {
                    new_mean[j] += resp * X[i][j];
                }
            }

            // Normalize
            int v = 0;
            for (; v < vectorizedLength; v += vectorSize)
            {
                var meanVec = new Vector<double>(new_mean, v);
                var result = meanVec / nk;
                result.CopyTo(new_mean, v);
            }

            for (; v < n_features; v++)
            {
                new_mean[v] /= nk;
            }
        }

        /// <summary>
        /// Optimized covariance update using SIMD operations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateCovarianceSIMD(double[][] X, double[][] responsibilities, int k,
            double[] mean, double[][] new_covariance, double nk)
        {
            int n_samples = X.Length;
            int n_features = mean.Length;

            for (int i = 0; i < n_samples; i++)
            {
                double resp = responsibilities[i][k];

                // Compute difference vector
                double[] diff = new double[n_features];
                VectorSubtractSIMD(X[i], mean, diff);

                // Outer product with SIMD optimization
                for (int j1 = 0; j1 < n_features; j1++)
                {
                    double diff_j1 = diff[j1];
                    double resp_diff_j1 = resp * diff_j1;

                    int vectorSize = Vector<double>.Count;
                    int vectorizedLength = n_features - (n_features % vectorSize);

                    int j2 = 0;
                    for (; j2 < vectorizedLength; j2 += vectorSize)
                    {
                        var diffVec = new Vector<double>(diff, j2);
                        var covVec = new Vector<double>(new_covariance[j1], j2);
                        var result = covVec + diffVec * resp_diff_j1;
                        result.CopyTo(new_covariance[j1], j2);
                    }

                    for (; j2 < n_features; j2++)
                    {
                        new_covariance[j1][j2] += resp_diff_j1 * diff[j2];
                    }
                }
            }

            // Normalize covariance
            for (int j1 = 0; j1 < n_features; j1++)
            {
                int vectorSize = Vector<double>.Count;
                int vectorizedLength = n_features - (n_features % vectorSize);

                int j2 = 0;
                for (; j2 < vectorizedLength; j2 += vectorSize)
                {
                    var covVec = new Vector<double>(new_covariance[j1], j2);
                    var result = covVec / nk;
                    result.CopyTo(new_covariance[j1], j2);
                }

                for (; j2 < n_features; j2++)
                {
                    new_covariance[j1][j2] /= nk;
                }
            }
        }

        /// <summary>
        /// SIMD-optimized vector subtraction
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void VectorSubtractSIMD(double[] a, double[] b, double[] result)
        {
            int length = a.Length;
            int vectorSize = Vector<double>.Count;
            int vectorizedLength = length - (length % vectorSize);

            int i = 0;
            for (; i < vectorizedLength; i += vectorSize)
            {
                var aVec = new Vector<double>(a, i);
                var bVec = new Vector<double>(b, i);
                var diff = aVec - bVec;
                diff.CopyTo(result, i);
            }

            for (; i < length; i++)
            {
                result[i] = a[i] - b[i];
            }
        }

        /// <summary>
        /// SIMD-optimized Euclidean distance squared
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double EuclideanDistanceSquaredSIMD(double[] a, double[] b)
        {
            int length = a.Length;
            int vectorSize = Vector<double>.Count;
            int vectorizedLength = length - (length % vectorSize);

            Vector<double> sumVec = Vector<double>.Zero;

            int i = 0;
            for (; i < vectorizedLength; i += vectorSize)
            {
                var aVec = new Vector<double>(a, i);
                var bVec = new Vector<double>(b, i);
                var diff = aVec - bVec;
                sumVec += diff * diff;
            }

            double sum = Vector.Dot(sumVec, Vector<double>.One);

            // Handle remaining elements
            for (; i < length; i++)
            {
                double diff = a[i] - b[i];
                sum += diff * diff;
            }

            return sum;
        }

        /// <summary>
        /// SIMD-optimized dot product
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double DotProductSIMD(double[] a, double[] b)
        {
            int length = a.Length;
            int vectorSize = Vector<double>.Count;
            int vectorizedLength = length - (length % vectorSize);

            Vector<double> sumVec = Vector<double>.Zero;

            int i = 0;
            for (; i < vectorizedLength; i += vectorSize)
            {
                var aVec = new Vector<double>(a, i);
                var bVec = new Vector<double>(b, i);
                sumVec += aVec * bVec;
            }

            double sum = Vector.Dot(sumVec, Vector<double>.One);

            for (; i < length; i++)
            {
                sum += a[i] * b[i];
            }

            return sum;
        }

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

        private double LogGaussianProbability(double[] x, double[] mean, double[][] covariance)
        {
            int n_features = x.Length;

            double[] diff = new double[n_features];
            VectorSubtractSIMD(x, mean, diff);

            double[][] inv_cov = InvertMatrix(covariance);
            double det_cov = DeterminantMatrix(covariance);

            if (det_cov <= 0)
            {
                det_cov = 1e-10;
            }

            // Compute mahalanobis distance using SIMD-optimized matrix-vector multiplication
            double mahalanobis = 0;
            for (int i = 0; i < n_features; i++)
            {
                double temp = DotProductSIMD(inv_cov[i], diff);
                mahalanobis += diff[i] * temp;
            }

            double log_prob = -0.5 * (n_features * Math.Log(2 * Math.PI) + Math.Log(det_cov) + mahalanobis);

            return log_prob;
        }

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

        public double[][] PredictProba(double[][] X)
        {
            return EStep(X);
        }

        public double Score(double[][] X)
        {
            int n_samples = X.Length;
            double total_log_likelihood = 0;

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

                total_log_likelihood += max_log_prob + Math.Log(sum_exp);
            }

            return total_log_likelihood / n_samples;
        }

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

        public double[][] Sample(int n_samples)
        {
            double[][] samples = new double[n_samples][];
            int n_features = means_[0].Length;

            for (int i = 0; i < n_samples; i++)
            {
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

                samples[i] = SampleFromGaussian(means_[component], covariances_[component]);
            }

            return samples;
        }

        private double[] SampleFromGaussian(double[] mean, double[][] covariance)
        {
            int n_features = mean.Length;

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

            double[][] L = CholeskyDecomposition(covariance);

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

        public double[] Weights => weights_;
        public double[][] Means => means_;
        public double[][][] Covariances => covariances_;
        public bool Converged => converged_;
        public int NumIterations => n_iter_;
        public double LowerBound => lower_bound_;

        // Matrix operations (kept from original, could also be optimized with SIMD)

        private double[][] InvertMatrix(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] result = new double[n][];
            double[][] temp = new double[n][];

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

            for (int i = 0; i < n; i++)
            {
                double pivot = temp[i][i];
                if (Math.Abs(pivot) < 1e-10)
                {
                    pivot = 1e-10;
                }

                for (int j = 0; j < n; j++)
                {
                    temp[i][j] /= pivot;
                    result[i][j] /= pivot;
                }

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

        private double DeterminantMatrix(double[][] matrix)
        {
            int n = matrix.Length;
            double[][] temp = new double[n][];

            for (int i = 0; i < n; i++)
            {
                temp[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    temp[i][j] = matrix[i][j];
                }
            }

            double det = 1.0;

            for (int i = 0; i < n; i++)
            {
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
                    return 1e-10;
                }

                det *= pivot;

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
