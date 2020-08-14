using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class LevelGenerator3D : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;
    public float
        mapScale = 1.1f,
        heightMultiplier = 2f;

    [Space, Space]
    public bool simple = true;
    public Wave[] waves;

    [Space, Space]
    public AnimationCurve heightCurve;

    [Space, Space]
    public Color[] terrainColors;
    public float[] terrainColorThresholds;
    public static Vector2 globalOffset;
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        #region Generating offset, heightMap, texture and mesh

        var verts = meshFilter.mesh.vertices;
        var levelSize = (int)Mathf.Sqrt(verts.Length);
        var offset = new Vector2(
            -transform.position.x / transform.localScale.x,
            -transform.position.z / transform.localScale.z
        ) + globalOffset;
        float[,] heightMap;
        if(simple)
        {
            //print("Level gened simple!");
            heightMap = NoiseMap.Get(levelSize, levelSize, mapScale, offset);
        }
        else
        {
            //print("Level gened with waves!");
            heightMap = NoiseMap.GetComplex(levelSize, levelSize, mapScale, offset, waves);
        }
        var texture = NoiseMap.AsColorTexture(heightMap, terrainColors, terrainColorThresholds);
        if(meshRenderer)
            meshRenderer.material.mainTexture = texture;

        for(int z=0, vertInd = 0; z < levelSize; z++)
        {
            for(var x=0; x<levelSize; x++, vertInd++)
            {
                var height = heightMap[z, x];
                var vertex = verts[vertInd];
                verts[vertInd] = new Vector3(vertex.x, heightCurve.Evaluate(height) * heightMultiplier, vertex.z);
            }
        }

        #endregion



        #region Applying all the stuff
        meshFilter.mesh.vertices = verts;
        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.RecalculateBounds();
        meshCollider.sharedMesh = meshFilter.mesh;
        #endregion

    }
}

public static class NoiseMap {
    #region Get Noise Map Methods
    public static float[,] Get(int mapDepth, int mapWidth, float scale, Vector2 offset)
    {
        var noiseMap = new float[mapDepth,mapWidth];
        for(var z=0; z<mapDepth; z++)
        {
            for(var x=0; x<mapWidth; x++)
            {
                float
                    sampleZ = (z + offset.y) / scale,
                    sampleX = (x + offset.x) / scale;
                var noise = Mathf.PerlinNoise(sampleX, sampleZ);

                noiseMap[z, x] = noise;
            }
        }

        return noiseMap;
    }
    public static float[,] GetComplex(int mapDepth, int mapWidth, float scale, Vector2 offset, Wave[] waves)
    {
        var noiseMap = new float[mapDepth, mapWidth];
        for (var z = 0; z < mapDepth; z++)
        {
            for (var x = 0; x < mapWidth; x++)
            {
                float
                    sampleZ = (z + offset.y) / scale,
                    sampleX = (x + offset.x) / scale,

                    noise = 0f,
                    normalization = 0f;
                foreach(var wave in waves)
                {
                    float proc(float sample) => sample * wave.frequency + wave.seed;
                    noise += wave.amplitude * Mathf.PerlinNoise(
                        proc(sampleX),
                        proc(sampleZ)
                    );
                    normalization += wave.amplitude;

                }
                noise /= normalization;

                noiseMap[z, x] = noise;
            }
        }

        return noiseMap;
    }

    #endregion

    #region Get Texture Methods
    public static Texture2D AsTexture(float[,] noiseMap)
    {
        int
            mapDepth = noiseMap.GetLength(0),
            mapWidth = noiseMap.GetLength(1);
        var colorMap = new Color[mapDepth * mapWidth];
        for(var z=0; z<mapDepth; z++)
        {
            for(var x=0; x<mapWidth; x++)
            {
                int colorIndex = z * mapWidth + x;
                var height = noiseMap[z, x];
                colorMap[colorIndex] = Color.Lerp(Color.black, Color.white, height);
            }
        }

        var texture = new Texture2D(mapDepth, mapWidth);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D AsColorTexture(float[,] noiseMap, Color[] colors, float[] colorThreshold)
    {
        if (colors.Length != colorThreshold.Length)
            throw new System.Exception("There are some extra thresholds/colors!");

        int
            mapDepth = noiseMap.GetLength(0),
            mapWidth = noiseMap.GetLength(1);
        var colorMap = new Color[mapDepth * mapWidth];
        for (var z = 0; z < mapDepth; z++)
        {
            for (var x = 0; x < mapWidth; x++)
            {
                int colorIndex = z * mapWidth + x;
                var height = noiseMap[z, x];
                var color = Color.white;
                for(var ind = 0; ind < colors.Length; ind++)
                    if(colorThreshold[ind] > height)
                    {
                        color = colors[ind];
                        break;
                    }
                colorMap[colorIndex] = color;
            }
        }

        var texture = new Texture2D(mapDepth, mapWidth);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    #endregion
}

[System.Serializable]
public class Wave {
    public float
        frequency,
        amplitude,
        seed;
}