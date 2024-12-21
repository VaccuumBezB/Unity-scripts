using UnityEngine;
using System.Collections.Generic;
//using JBooth.MicroSplat;

public class WorldGenerator : MonoBehaviour
{
    [Header("Настройки генерации")]
    [SerializeField] private Transform player;
    [SerializeField] private DiamondSquare terrainGenerator;
    [SerializeField] private int chunkSize = 256;
    [SerializeField] private int viewDistance = 3; 
    [SerializeField] private float heightScale = 20f;
    [SerializeField] private Material mat;
    [SerializeField] private Vector3 worldScale = new Vector3(10f, 10f, 10f);
    [SerializeField] private TerrainLayer mainLayer, secondLayer; 
    
    [Header("Настройки оптимизации")]
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private bool useThreading = true;
    
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();
    private Queue<GameObject> chunkPool = new Queue<GameObject>();
    private Vector2Int currentChunk;
    private float updateTimer;

    private void Start()
    {
        if (!player || !terrainGenerator)
        {
            Debug.LogError("WorldGenerator: Отсутствуют необходимые компоненты!");
            enabled = false;
            return;
        }

        InitializeChunkPool();
        UpdateCurrentChunk();
        GenerateInitialChunks();
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            updateTimer = 0;
            CheckChunks();
        }
    }

    private void InitializeChunkPool()
    {
        int poolSize = (viewDistance * 2 + 1) * (viewDistance * 2 + 1);
        for (int i = 0; i < poolSize; i++)
        {
            GameObject chunk = CreateChunk();
            chunk.SetActive(false);
            chunkPool.Enqueue(chunk);
        }
    }

    private float[,] GenerateHeightMap(Vector2Int chunkPos)
    {
        float[,] heights = new float[chunkSize, chunkSize];
        
        // Используем seed на основе позиции чанка для консистентной генерации
        int chunkSeed = chunkPos.x * 10000 + chunkPos.y + terrainGenerator.seed;
        Random.InitState(chunkSeed);

        // Генерируем высоты используя алгоритм Diamond-Square
        terrainGenerator.GenerateHeightMap(heights, chunkSize, chunkPos);

        // Сглаживаем ландшафт
        float[,] smoothedHeights = new float[chunkSize, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                float sum = 0;
                int count = 0;
                
                // Берем среднее значение по соседним точкам
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        
                        if (nx >= 0 && nx < chunkSize && ny >= 0 && ny < chunkSize)
                        {
                            // Используем весовой коэффициент - чем дальше точка, тем меньше её влияние
                            float weight = 1.0f / (1.0f + Mathf.Sqrt(dx * dx + dy * dy));
                            sum += heights[nx, ny] * weight;
                            count += 1;
                        }
                    }
                }
                
                smoothedHeights[x, y] = sum / count;
            }
        }

        // Применяем сглаженные высоты и масштабирование
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                heights[x, y] = smoothedHeights[x, y] / heightScale;
            }
        }

        return heights;
    }

    private GameObject CreateChunk()
    {
        GameObject chunk = new GameObject("Chunk");
        chunk.transform.parent = transform;
        
        Terrain terrain = chunk.AddComponent<Terrain>();
        /*MicroSplatTerrain mst = chunk.AddComponent<MicroSplatTerrain>();
        mst.Convert();*/
        TerrainCollider terrainCollider = chunk.AddComponent<TerrainCollider>();
        
        TerrainData terrainData = new TerrainData();
        //terrainData.terrainLayers[0] = mainLayer;
        
        // Устанавливаем разрешение и размер
        terrainData.heightmapResolution = chunkSize;
        terrainData.size = new Vector3(
            chunkSize * worldScale.x, 
            heightScale * 2f * worldScale.y, 
            chunkSize * worldScale.z
        );
        //terrainData.terrainLayers[1] = secondLayer;
        //terrainData.alphaMap;

        // Настраиваем детализацию
        terrainData.SetDetailResolution(1024, 16);
        
        terrain.terrainData = terrainData;
        terrain.materialTemplate = mat;
        terrainCollider.terrainData = terrainData;
        
        return chunk;
    }

    private void UpdateCurrentChunk()
    {
        Vector3 playerPos = player.position;
        currentChunk = new Vector2Int(
            Mathf.FloorToInt(playerPos.x / (chunkSize * worldScale.x)),
            Mathf.FloorToInt(playerPos.z / (chunkSize * worldScale.z))
        );
    }

    private void CheckChunks()
    {
        Vector2Int newChunk = new Vector2Int(
            Mathf.FloorToInt(player.position.x / (chunkSize * worldScale.x)),
            Mathf.FloorToInt(player.position.z / (chunkSize * worldScale.z))
        );

        if (newChunk != currentChunk)
        {
            currentChunk = newChunk;
            UpdateChunks();
        }
    }

    private void GenerateInitialChunks()
    {
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int chunkPos = currentChunk + new Vector2Int(x, z);
                GenerateChunk(chunkPos);
            }
        }
    }

    private void UpdateChunks()
    {
        // Собираем чанки, которые нужно выгрузить
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in activeChunks)
        {
            if (IsChunkOutOfRange(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        // Выгружаем ненужные чанки
        foreach (var chunkPos in chunksToRemove)
        {
            UnloadChunk(chunkPos);
        }

        // Загружаем новые чанки
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int chunkPos = currentChunk + new Vector2Int(x, z);
                if (!activeChunks.ContainsKey(chunkPos))
                {
                    GenerateChunk(chunkPos);
                }
            }
        }
    }

    private bool IsChunkOutOfRange(Vector2Int chunkPos)
    {
        return Mathf.Abs(chunkPos.x - currentChunk.x) > viewDistance ||
               Mathf.Abs(chunkPos.y - currentChunk.y) > viewDistance;
    }

    private void GenerateChunk(Vector2Int chunkPos)
    {
        if (activeChunks.ContainsKey(chunkPos)) return;

        GameObject chunk = GetChunkFromPool();
        chunk.transform.position = new Vector3(
            chunkPos.x * chunkSize * worldScale.x,
            0,
            chunkPos.y * chunkSize * worldScale.z
        );

        TerrainData terrainData = chunk.GetComponent<Terrain>().terrainData;
        float[,] heights = GenerateHeightMap(chunkPos);
        terrainData.SetHeights(0, 0, heights);

        activeChunks.Add(chunkPos, chunk);
        chunk.SetActive(true);
    }

    private GameObject GetChunkFromPool()
    {
        if (chunkPool.Count > 0)
            return chunkPool.Dequeue();
        return CreateChunk();
    }

    private void UnloadChunk(Vector2Int chunkPos)
    {
        if (!activeChunks.TryGetValue(chunkPos, out GameObject chunk)) return;

        chunk.SetActive(false);
        chunkPool.Enqueue(chunk);
        activeChunks.Remove(chunkPos);
    }
} 