using UnityEngine;

public class BottleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private LevelData levelData;

    [Header("Grid Settings")]
    public int maxRows = 3;
    public int maxColumns = 3;
    public float spacing = 2.0f;

    [Header("Scaling")]
    public float scaleFactor = 0.9f;

    private void Start()
    {
        SpawnBottles();
    }

    private void SpawnBottles()
    {
        int totalBottles = levelData.bottles.Count;
        int rows = Mathf.Min(maxRows, Mathf.CeilToInt(totalBottles / (float)maxColumns));
        int cols = Mathf.Min(maxColumns, totalBottles);

        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Screen.width / Screen.height;

        float availableWidth = screenWidth * scaleFactor;
        float availableHeight = screenHeight * scaleFactor;

        float cellWidth = availableWidth / cols;
        float cellHeight = availableHeight / rows;

        float adjustedSpacing = Mathf.Min(cellWidth, cellHeight);

        Vector2 startPos = new Vector2(
            -((cols - 1) * adjustedSpacing) / 2f,
             ((rows - 1) * adjustedSpacing) / 2f
        );

        for (int i = 0; i < totalBottles; i++)
        {
            int row = i / cols;
            int col = i % cols;

            Vector2 spawnPos = startPos + new Vector2(col * adjustedSpacing, -row * adjustedSpacing);
            GameObject bottle = Instantiate(bottlePrefab, spawnPos, Quaternion.identity, transform);

            ColorChanger cc = bottle.GetComponent<ColorChanger>();
            var data = levelData.bottles[i];
            cc.numberOfColorsInBottle = data.numberOfColorsInBottle;

            for (int j = 0; j < data.colors.Count && j < 4; j++)
                cc.bottleColors[j] = data.colors[j];

            float bottleScale = adjustedSpacing / 2f;
            bottle.transform.localScale = Vector3.one * bottleScale;

            cc.bottleScale = bottleScale; // NEW
            cc.bottleMaskSR.material.SetFloat("_BottleScale", bottleScale); // NEW

            cc.UpdateColorsOnShader();
            cc.UpdateTopColorValues();
        }
    }
}
