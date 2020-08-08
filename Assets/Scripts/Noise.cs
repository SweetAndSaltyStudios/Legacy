using UnityEngine;

public static class Noise
{
    private const int RANDOM_BOUNDARIES = 1000000;

    public static float[,] GenerateNoiseMap(
        int mapWidth, 
        int mapHeight, 
        int seed,
        float scale, 
        int octaves, 
        float persistance, 
        float lacunarity,
        Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

       
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-RANDOM_BOUNDARIES, RANDOM_BOUNDARIES) + offset.x;
            float offsetY = prng.Next(-RANDOM_BOUNDARIES, RANDOM_BOUNDARIES) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }



        if (scale <= 0)
        {
            scale = 0.001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2;
        float halfHeight = mapHeight / 2;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i < octaves; i++)
                {
                    float tempX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float tempY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    float perlinValue = Mathf.PerlinNoise(tempX, tempY) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight,
                                                    maxNoiseHeight,
                                                    noiseMap[x, y]);
            }
        }
                return noiseMap;
    }
}
