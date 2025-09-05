using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BiomeType {
    flat_land,
    hills,
    mountain,
    high_mountains,
    swamp,
    lake,
    ocean
}

public static class BiomeInfo
{
    // TODO: change this, game design has evolved
    // Maybe use jsons?
    public static void getNoiseInfo(BiomeType biomeType, out float noiseScale, out float noiseRoughness)
    {
        switch (biomeType)
        {
            case BiomeType.flat_land:
                noiseScale = 0;
                noiseRoughness = 0;
                break;

            case BiomeType.hills:
                noiseScale = 4;
                noiseRoughness = 0;
                break;

            case BiomeType.mountain:
                noiseScale = 4;
                noiseRoughness = 0.44f;
                break;

            case BiomeType.high_mountains:
                noiseScale = 4;
                noiseRoughness = 0.44f;
                break;

            case BiomeType.swamp:
                noiseScale = 4;
                noiseRoughness = 0;
                break;

            case BiomeType.lake:
                noiseScale = 4;
                noiseRoughness = 0;
                break;

            case BiomeType.ocean:
                noiseScale = 4;
                noiseRoughness = 0;
                break;

            default:
                noiseScale = 0;
                noiseRoughness = 0;
                Debug.Log("Trying to get noise info from an unregistered biome");
                break;
        }
    }

    // TODO: change that too
    public static int getHeight(BiomeType biomeType)
    {
        switch (biomeType)
        {
            case BiomeType.flat_land:
                return 0;

            case BiomeType.hills:
                return 1;

            case BiomeType.mountain:
                return 2;

            case BiomeType.high_mountains:
                return 3;

            case BiomeType.swamp:
                return -1;

            case BiomeType.lake:
                return -2;

            case BiomeType.ocean:
                return -3;

            default:
                Debug.Log("Trying to get height from an unregistered biome");
                return 0;
        }
    }
}
