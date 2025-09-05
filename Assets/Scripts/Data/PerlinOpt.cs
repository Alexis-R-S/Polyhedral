using System;

[Serializable]
public class PerlinOpt
{
    // Perlin noise parameters
    public int detail; // Number of octaves
    public float[] frequencies;    // Frequency of each octave
    public float[] amplitudes;     // Amplitude of each octave

    // Attenuation parameters
    public float height;
    public float max;
    public float power;
    public float width;
}
