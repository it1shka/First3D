using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MapGenerator3D : MonoBehaviour
{
    [Space, Space]
    public float downgrading = 1f;

    [Space, Space]
    public GameObject levelPrefab;
    public Vector2 levelSize = new Vector2(10, 10);
    public int
        mapDepth = 5,
        mapWidth = 5;
    public int
        asyncOperationsTile = 100,
        asyncOperationsTree = 1000;

    [Space, Space]
    public bool spawnObjects = false;
    public int tiling = 4;
    public SpawnObject[] spawnableObjects;

    [Space, Space]
    public GameObject loadingScreen;
    public TextMeshProUGUI
        status,
        percents;
    public Slider slider;
    //[Space, Space]
    //public Transform starsTf;

    private int levelVertSize;
    private void Start()
    {
        levelSize *= downgrading;
        // tiling = Mathf.FloorToInt((float)tiling / (downgrading / 2));
        // if (tiling < 1) tiling = 1;
        mapDepth = Mathf.FloorToInt((float)mapDepth / downgrading);
        mapWidth = Mathf.FloorToInt((float)mapWidth / downgrading);

        levelVertSize = (int)Mathf.Sqrt(
            levelPrefab.GetComponent<MeshFilter>().
                sharedMesh.vertices.Length
        );
        GenerateMapAsync();
    }

    private void SetLevelSeed()
    {
        float Rand() => Random.Range(-1000000f, 1000000f);
        LevelGenerator3D.globalOffset = new Vector2(Rand(), Rand());
    } 

    public void GenerateMapAsync()
    {
        SetLevelSeed();
        StartCoroutine(SpawnMapAsync());
    }


    private IEnumerator SpawnMapAsync()
    {
        var operations = 0;
        var globalVertexPositions = new Vector3[levelVertSize * mapDepth, levelVertSize * mapWidth];
        for (int z = 0, operationsTotal = 0; z < mapDepth; z++)
        {
            for (var x = 0; x < mapWidth; x++, operationsTotal++)
            {
                var levelPosition =
                    new Vector3(levelSize.x * x, 0f, levelSize.y * z) + transform.position;
                var newLevel = Instantiate(levelPrefab, levelPosition, Quaternion.identity);
                var genComp = newLevel.GetComponent<LevelGenerator3D>();
                genComp.mapScale /= downgrading;
                genComp.transform.localScale = new Vector3(
                    genComp.transform.localScale.x * downgrading,
                    genComp.transform.localScale.y,
                    genComp.transform.localScale.z * downgrading
                );
                genComp.GenerateLevel();
                var meshFilter = newLevel.GetComponent<MeshFilter>();
                var vertexes = meshFilter.mesh.vertices;
                for(var lz =0; lz < levelVertSize; lz++)
                {
                    for(var lx =0; lx < levelVertSize; lx++)
                    {
                        var localVertexIndex = lz * levelVertSize + lx;
                        globalVertexPositions[z  * levelVertSize + lz, x  * levelVertSize + lx]
                            = meshFilter.transform.TransformPoint(vertexes[localVertexIndex]);
                    }
                }

                SetStatus("Generating tiles...",  (float)operationsTotal / mapDepth / mapWidth );

                operations++;
                if(operations >= asyncOperationsTile)
                {
                    operations = 0;
                    yield return null;
                }
            }
        }

        Debug.LogWarning("Map gened async!");
        if (spawnObjects)
            StartCoroutine(SpawnTreesAsync(globalVertexPositions));
        else
            loadingScreen.SetActive(false);
        yield break; 
    }

    private IEnumerator SpawnTreesAsync(Vector3[,] globalVertexPositions)
    {
        var operations = 0;
        for(int zInd=0, operationsTotal = 0; zInd < globalVertexPositions.GetLength(0) / tiling; zInd++)
        {
            for(var xInd = 0; xInd < globalVertexPositions.GetLength(1) / tiling; xInd++, operationsTotal++)
            {
                var heightSum = 0f;
                for(var zStart = zInd * tiling; zStart < (zInd + 1) * tiling; zStart++)
                {
                    for(var xStart = xInd * tiling; xStart < (xInd + 1) * tiling; xStart++)
                    {
                        heightSum += globalVertexPositions[zStart, xStart].y;
                    }
                }
                heightSum /= tiling * tiling;
                /*
                if(heightSum <= treeMaxBound && heightSum >= treeMinBound)
                {
                    Instantiate(treePrefab, 
                        globalVertexPositions[zInd * tiling + tiling / 2, xInd * tiling + tiling / 2], 
                        Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                }
                */
                for(var i = 0; i<spawnableObjects.Length; i++)
                {
                    var so = spawnableObjects[i];
                    if (so.maxBound > heightSum)
                    {
                        if(Random.Range(0f, 1f) <= so.probability && 
                            so.vars.Length > 0)
                        {
                            Instantiate(so.vars[Random.Range(0, so.vars.Length)],
                                globalVertexPositions[zInd * tiling + tiling / 2, xInd * tiling + tiling / 2],
                                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
                        }
                        break;
                    }
                }

                SetStatus("Planting trees...", 
                    (float)operationsTotal / globalVertexPositions.GetLength(0) * tiling / globalVertexPositions.GetLength(1) * tiling);

                operations++;
                if (operations >= asyncOperationsTree)
                {
                    operations = 0;
                    yield return null;
                }
            }
        }

        // //placing the stars
        //starsTf.position = transform.position + new Vector3(levelSize.x * mapWidth / 2, 0f, levelSize.y * mapDepth / 2);

        loadingScreen.SetActive(false);

        Debug.LogWarning("Trees gened async!");
        yield break;
    }

    private void SetStatus(string message, float value)
    {
        status.text = message;
        slider.value = value;
        percents.text = $"{(int)(value * 100)}%";
    }
}

[System.Serializable]
public class SpawnObject
{
    public string name;
    public float maxBound;
    [Range(0f, 1f)]
    public float probability;
    public GameObject[] vars;
}