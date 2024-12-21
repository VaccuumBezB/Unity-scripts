using UnityEngine;
using UnityEngine.TerrainTools;

[RequireComponent(typeof(TerrainCollider))]
public class DiamondSquare : MonoBehaviour 
{
	[Header("Основные настройки")]
	[SerializeField] private float roughness = 0.5f;
	[SerializeField] public int seed;
	[SerializeField] private float heightScale = 40f;
	[SerializeField] private float erosionFactor = 0.15f;
	[SerializeField] private int erosionIterations = 5;
	[Header("Настройки ландшафта")]
	[SerializeField] private float baseFrequency = 0.6f;
	[SerializeField] private float persistence = 0.5f;
	[SerializeField] private int octaves = 6;
	[SerializeField] private float mountainBias = 1.2f;
	[SerializeField] private float valleyDepth = 0.3f;

	private TerrainData data;
	private int size;
	private bool randomizeCornerValues = false;
	private float[,] heights;

	public float Roughness 
	{
		get { return roughness; }
		set { roughness = Mathf.Clamp(value, 0.001f, 1.999f); }
	}

	private void Awake() 
	{
		seed = Random.Range(0, 256);
		data = transform.GetComponent<TerrainCollider>().terrainData;
		size = data.heightmapResolution;
		Reset();
	}

	public void SetSeed() 
	{
		Random.InitState(seed);
	}

	public void ToggleRandomizeCornerValues() 
	{
		randomizeCornerValues = !randomizeCornerValues;
	}

	public void Reset() 
	{
		heights = new float[size, size];

		if (randomizeCornerValues) 
		{
			heights[0, 0] = Random.value;
			heights[size - 1, 0] = Random.value;
			heights[0, size - 1] = Random.value;
				heights[size - 1, size - 1] = Random.value;
		}

		data.SetHeights(0, 0, heights);
	}

	public void GenerateHeightMap(float[,] targetHeights, int targetSize, Vector2Int chunkPos)
	{
		// Генерация базовой карты высот с помощью нескольких октав шума Перлина
		for (int x = 0; x < targetSize; x++)
		{
			for (int y = 0; y < targetSize; y++)
			{
				float amplitude = 1f;
				float frequency = baseFrequency;
				float noiseHeight = 0f;
				float amplitudeSum = 0f;

				for (int i = 0; i < octaves; i++)
				{
					float sampleX = (x + chunkPos.x * (targetSize - 1)) * frequency;
					float sampleY = (y + chunkPos.y * (targetSize - 1)) * frequency;
					
					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
					noiseHeight += perlinValue * amplitude;
					amplitudeSum += amplitude;

					amplitude *= persistence;
					frequency *= 2f;
				}

				noiseHeight /= amplitudeSum;
				
				// Применяем нелинейное преобразование для создания более выраженного рельефа
				noiseHeight = Mathf.Pow(noiseHeight, mountainBias);
				
				// Создаем более глубокие долины
				if (noiseHeight < 0.5f)
				{
					noiseHeight = Mathf.Lerp(noiseHeight, noiseHeight * valleyDepth, (0.5f - noiseHeight) * 2f);
				}

				targetHeights[x, y] = noiseHeight;
			}
		}

		// Применяем термальную эрозию и масштабирование высот
		ApplyThermalErosion(targetHeights, targetSize);
		ApplyHeightScale(targetHeights, targetSize);
	}

	private void ApplyThermalErosion(float[,] heights, int size)
	{
		float[,] tempHeights = new float[size, size];
		float talus = erosionFactor * 0.8f; // Угол осыпания

		for (int iteration = 0; iteration < erosionIterations; iteration++)
		{
			System.Array.Copy(heights, tempHeights, heights.Length);
			
			for (int x = 1; x < size - 1; x++)
			{
				for (int y = 1; y < size - 1; y++)
				{
					float maxSlope = 0f;
					int maxSlopeX = 0;
					int maxSlopeY = 0;
					float currentHeight = heights[x,y];

					// Находим самый крутой склон
					for (int dx = -1; dx <= 1; dx++)
					{
						for (int dy = -1; dy <= 1; dy++)
						{
							if (dx == 0 && dy == 0) continue;
							
							float slope = (currentHeight - heights[x+dx,y+dy]) / 
								Mathf.Sqrt(dx*dx + dy*dy);
							
							if (slope > maxSlope)
							{
								maxSlope = slope;
								maxSlopeX = dx;
								maxSlopeY = dy;
							}
						}
					}

					// Если склон круче угла осыпания, перемещаем материал
					if (maxSlope > talus)
					{
						float diff = maxSlope - talus;
						float transfer = diff * 0.5f;
						tempHeights[x,y] -= transfer;
						tempHeights[x+maxSlopeX,y+maxSlopeY] += transfer;
					}
				}
			}
			
			System.Array.Copy(tempHeights, heights, heights.Length);
		}
	}

	private void ApplyHeightScale(float[,] heights, int size)
	{
		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				heights[x,y] *= heightScale;
			}
		}
	}

	public int GetStepSize(int depth)
	{
		if (!ValidateDepth(depth))
		{
			return -1;
		}
		return (int)((size - 1) / Mathf.Pow(2, (depth - 1)));
	}

	public int MaxDepth()
	{
		return (int)((Mathf.Log(size - 1) / 0.69314718056f) + 1);
	}

	private bool ValidateDepth(int depth)
	{
		return depth > 0 && depth <= MaxDepth();
	}
}
