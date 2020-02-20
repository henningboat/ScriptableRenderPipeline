// ----------------------------------------------------------------------------
// SSS/Transmittance helper
// ----------------------------------------------------------------------------

// Computes the fraction of light passing through the object.
// Evaluate Int{0, inf}{2 * Pi * r * R(sqrt(r^2 + d^2))}, where R is the diffusion profile.
// Note: 'volumeAlbedo' should be premultiplied by 0.25.
// Ref: Approximate Reflectance Profiles for Efficient Subsurface Scattering by Pixar (BSSRDF only).
float3 ComputeTransmittanceDisney(float3 S, float3 volumeAlbedo, float thickness)
{
    // Thickness and SSS mask are decoupled for artists.
    // In theory, we should modify the thickness by the inverse of the mask scale of the profile.
    // thickness /= subsurfaceMask;

#if 0
    float3 exp_13 = exp(((-1.0 / 3.0) * thickness) * S);
#else
    // Help the compiler. S is premultiplied by ((-1.0 / 3.0) * LOG2_E) on the CPU.
    float3 p = thickness * S;
    float3 exp_13 = exp2(p);
#endif

    // Premultiply & optimize: T = (1/4 * A) * (e^(-t * S) + 3 * e^(-1/3 * t * S))
    return volumeAlbedo * (exp_13 * (3 + exp_13 * exp_13));
}

// Performs sampling of the Normalized Burley diffusion profile in polar coordinates.
// The result must be multiplied by the albedo.
float3 EvalBurleyDiffusionProfile(float r, float3 S)
{
    float3 exp_13 = exp2(((LOG2_E * (-1.0/3.0)) * r) * S); // Exp[-S * r / 3]
    float3 expSum = exp_13 * (1 + exp_13 + exp_13);        // Exp[-S * r / 3] + Exp[-S * r]

#if SSS_ELIMINATE_CONSTANTS
    return expSum;
#else
    return (S * rcp(8 * PI)) * expSum; // S / (8 * Pi) * (Exp[-S * r / 3] + Exp[-S * r])
#endif
}

// https://zero-radiance.github.io/post/sampling-diffusion/
// Performs sampling of a Normalized Burley diffusion profile in polar coordinates.
// 'u' is the random number: [0, 1).
// rcp(S) = 1 / ShapeParam = ScatteringDistance.
// 'r' is the sampled radial distance.
// rcp(Pdf) is the reciprocal of the corresponding PDF value.
void SampleBurleyDiffusionProfile(float u, float rcpS, out float r, out float rcpPdf)
{
    u = 1 - u; // Convert cCDF to CDF s.t. (r(0) = 0) and (r(1) = Inf)

    // assert(0 <= u && u < 1);

    float g = 1 + (4 * u) * (2 * u + sqrt(1 + (4 * u) * u));
    float n = exp2(log2(g) * (-1.0/3.0));                    // g^(-1/3)
    float p = (g * n) * n;                                   // g^(+1/3)
    float c = 1 + p + n;                                     // 1 + g^(+1/3) + g^(-1/3)
    float d = (3 / LOG2_E * 2) + (3 / LOG2_E) * log2(u);     // 3 * Log[4 * u]
    float x = (3 / LOG2_E) * log2(c) - d;                    // 3 * Log[c / (4 * u)]

    // x      = S * r
    // exp_13 = Exp[-x/3] = Exp[-1/3 * 3 * Log[c / (4 * u)]]
    // exp_13 = Exp[-Log[c / (4 * u)]] = (4 * u) / c
    // exp_1  = Exp[-x] = exp_13 * exp_13 * exp_13
    // expSum = exp_1 + exp_13 = exp_13 * (1 + exp_13 + exp_13)
    // rcpExp = rcp(expSum) = c^3 / ((4 * u) * (c^2 + 16 * u^2))
    float rcpExp = ((c * c) * c) * rcp((4 * u) * ((c * c) + (4 * u) * (4 * u)));

    r      = x * rcpS;
#if SSS_ELIMINATE_CONSTANTS
    rcpPdf = rcpExp;
#else
    rcpPdf = (8 * PI * rcpS) * rcpExp; // (8 * Pi) / S / (Exp[-S * r / 3] + Exp[-S * r])
#endif
}
