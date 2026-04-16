# Gaussian Mixture Model (GMM) Implementation in C#

This is a pure C# implementation of Gaussian Mixture Model, similar to `sklearn.mixture.GaussianMixture`, without using any third-party libraries.

## Overview

A Gaussian Mixture Model is a probabilistic model that assumes all data points are generated from a mixture of a finite number of Gaussian distributions with unknown parameters. It's commonly used for:
- Clustering
- Density estimation
- Anomaly detection
- Data generation

## Features

This implementation includes:

- **Expectation-Maximization (EM) Algorithm**: Core algorithm for fitting the model
- **K-means++ Initialization**: Smart initialization for better convergence
- **Multiple Initialization Attempts**: Run multiple times and keep the best result
- **Prediction Methods**:
  - `Predict()`: Assign cluster labels to samples
  - `PredictProba()`: Compute posterior probabilities for each component
  - `Score()`: Compute average log-likelihood
  - `ScoreSamples()`: Compute log-likelihood for each sample
- **Sample Generation**: Generate new samples from the fitted distribution
- **Numerical Stability**: Uses log-sum-exp trick to prevent numerical underflow
- **Convergence Detection**: Monitors log-likelihood changes

## Usage

### Basic Example

```csharp
using MachineLearning.Mixture;

// Prepare your data (n_samples x n_features)
double[][] X = new double[][]
{
    new double[] { 1.0, 2.0 },
    new double[] { 1.5, 1.8 },
    new double[] { 5.0, 6.0 },
    new double[] { 5.5, 5.8 },
};

// Create and fit the model
GaussianMixture gmm = new GaussianMixture(
    n_components: 2,      // Number of mixture components
    max_iter: 100,        // Maximum EM iterations
    random_state: 42,     // For reproducibility
    verbose: true         // Print progress
);

gmm.Fit(X);

// Predict cluster labels
int[] labels = gmm.Predict(X);

// Get probabilities
double[][] probabilities = gmm.PredictProba(X);

// Generate new samples
double[][] new_samples = gmm.Sample(10);
```

### Parameters

- **n_components** (int, default=1): Number of mixture components
- **covariance_type** (string, default="full"): Type of covariance (currently supports "full")
- **tol** (double, default=1e-3): Convergence threshold for log-likelihood change
- **max_iter** (int, default=100): Maximum number of EM iterations
- **n_init** (int, default=1): Number of initializations to perform
- **init_params** (string, default="kmeans"): Initialization method ("kmeans" or "random")
- **random_state** (int, default=-1): Random seed for reproducibility (-1 for random)
- **verbose** (bool, default=false): Enable verbose output

### Fitted Attributes

After calling `Fit()`, you can access:

- **Weights**: Mixture component weights (array of length n_components)
- **Means**: Mean vectors for each component (n_components x n_features)
- **Covariances**: Covariance matrices for each component
- **Converged**: Whether the EM algorithm converged
- **NumIterations**: Number of iterations performed
- **LowerBound**: Log-likelihood lower bound

## Algorithm Details

### Expectation-Maximization (EM) Algorithm

1. **E-step**: Compute responsibilities (posterior probabilities) for each sample and component
2. **M-step**: Update parameters (weights, means, covariances) based on responsibilities
3. **Convergence Check**: Monitor log-likelihood changes

### Mathematical Foundation

For a Gaussian Mixture with K components:

**Probability Density:**
```
p(x) = Σ(k=1 to K) π_k * N(x | μ_k, Σ_k)
```

Where:
- π_k: Weight of component k (Σπ_k = 1)
- μ_k: Mean of component k
- Σ_k: Covariance matrix of component k
- N(x | μ, Σ): Gaussian probability density function

**Responsibility (E-step):**
```
γ(z_k) = π_k * N(x | μ_k, Σ_k) / Σ(j=1 to K) π_j * N(x | μ_j, Σ_j)
```

**Parameter Updates (M-step):**
```
N_k = Σ(i=1 to N) γ(z_ik)
π_k = N_k / N
μ_k = (1/N_k) * Σ(i=1 to N) γ(z_ik) * x_i
Σ_k = (1/N_k) * Σ(i=1 to N) γ(z_ik) * (x_i - μ_k)(x_i - μ_k)^T
```

## Implementation Notes

### Matrix Operations

The implementation includes custom matrix operations:
- **Matrix Inversion**: Gauss-Jordan elimination
- **Determinant Calculation**: LU decomposition
- **Cholesky Decomposition**: For sampling from multivariate Gaussian

### Numerical Stability

- **Log-sum-exp trick**: Prevents numerical underflow in probability calculations
- **Regularization**: Adds small values to covariance diagonal to prevent singularity
- **Pivot Selection**: Used in matrix operations for better numerical stability

### Initialization

**K-means++**:
1. Choose first center randomly
2. For each subsequent center, choose with probability proportional to squared distance from nearest existing center
3. Provides better initial means than random selection

## Example Output

```
Iteration 1: log-likelihood = -15.2341, change = 8.234567
Iteration 2: log-likelihood = -12.4532, change = 2.780900
...
Iteration 8: log-likelihood = -10.1234, change = 0.000892

Converged: True
Number of iterations: 8

Means:
Component 0: [1.150, 1.980]
Component 1: [5.150, 5.980]

Weights:
Component 0: 0.5000
Component 1: 0.5000
```

## Comparison with sklearn

This implementation provides similar functionality to `sklearn.mixture.GaussianMixture`:

| Feature | This Implementation | sklearn |
|---------|-------------------|---------|
| EM Algorithm | ✓ | ✓ |
| Multiple Initializations | ✓ | ✓ |
| K-means++ Init | ✓ | ✓ |
| Full Covariance | ✓ | ✓ |
| Tied/Diag/Spherical Cov | ✗ | ✓ |
| Regularization | ✓ | ✓ |
| Predict | ✓ | ✓ |
| Predict Proba | ✓ | ✓ |
| Score | ✓ | ✓ |
| Sample | ✓ | ✓ |
| AIC/BIC | ✗ | ✓ |

## Compilation

To compile and run:

```bash
# Compile
csc /out:GaussianMixtureExample.exe GaussianMixture.cs GaussianMixtureExample.cs

# Run
./GaussianMixtureExample.exe
```

Or create a C# project:

```bash
dotnet new console -n GaussianMixtureProject
# Copy the files to the project
dotnet build
dotnet run
```

## Limitations

- Currently only supports full covariance matrices
- Does not implement tied, diagonal, or spherical covariance types
- No AIC/BIC model selection criteria
- No warm start capability

## Extensions

Possible enhancements:
1. Add support for different covariance types (tied, diag, spherical)
2. Implement Bayesian GMM (with Dirichlet priors)
3. Add model selection criteria (AIC, BIC)
4. Optimize performance with parallel computation
5. Add visualization helpers

## References

1. Bishop, C. M. (2006). Pattern Recognition and Machine Learning. Chapter 9: Mixture Models and EM
2. Murphy, K. P. (2012). Machine Learning: A Probabilistic Perspective. Chapter 11
3. Scikit-learn documentation: https://scikit-learn.org/stable/modules/mixture.html

## License

This is a reference implementation for educational purposes.
