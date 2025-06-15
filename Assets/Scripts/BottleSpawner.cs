using TMPro;
using UnityEngine;
public class BottleSpawner : MonoBehaviour
{

    public TextMeshProUGUI currentLevel;

    [SerializeField] private GameObject bottlePrefab;
    [SerializeField] private LevelData[] passedlevelData;
    [SerializeField] private LevelData levelData;
    [Header("Grid Settings")]
    public int maxRows = 3;
    public int maxColumns = 3;
    public float spacing = 2.0f;
    [Header("Scaling")]
    public float scaleFactor = 0.9f;
    [Header("Spacing Control")]
    [Range(0.1f, 1.0f)]
    public float horizontalSpacingMultiplier = 0.6f; // Control horizontal spacing between bottles
    [Range(0.1f, 1.0f)]
    public float verticalSpacingMultiplier = 1.0f; // Control vertical spacing between bottles

    [Header("Debug")]
    public int debugLevelIndex = 0; // Fallback level for testing


    private void Start()
    {
        // Add safety checks and error handling
        int currentLevelIndex = GetCurrentLevelIndex();

        // Validate level index
        if (passedlevelData == null || passedlevelData.Length == 0)
        {
            Debug.LogError("passedlevelData is null or empty! Please assign level data in the inspector.");
            return;
        }

        if (currentLevelIndex < 0 || currentLevelIndex >= passedlevelData.Length)
        {
            Debug.LogError($"Level index {currentLevelIndex} is out of range! Available levels: 0-{passedlevelData.Length - 1}");
            currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, passedlevelData.Length - 1);
        }

        levelData = passedlevelData[currentLevelIndex];

        if (levelData == null)
        {
            Debug.LogError($"Level data at index {currentLevelIndex} is null!");
            return;
        }

        Debug.Log($"Loading level {currentLevelIndex}: {levelData.name}");
        SpawnBottles();
    }

    private int GetCurrentLevelIndex()
    {
        // Try to get level from LevelSelection singleton
        if (LevelSelection.Instance != null)
        {
            currentLevel.text = $"Level: {LevelSelection.Instance.currentLevel + 1}";
            return LevelSelection.Instance.currentLevel;
        }

        // If LevelSelection doesn't exist, check for alternative sources
        Debug.LogWarning("LevelSelection.Instance is null! Using fallback methods...");

        // Option 1: Check if there's a LevelSelection component in the scene
        LevelSelection levelSelection = FindObjectOfType<LevelSelection>();
        if (levelSelection != null)
        {
            Debug.Log("Found LevelSelection component in scene");

            return levelSelection.currentLevel;
        }


        // Option 3: Use debug level
        Debug.LogWarning($"No level data found, using debug level: {debugLevelIndex}");
        return debugLevelIndex;
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

        // Calculate separate horizontal and vertical spacing
        float horizontalSpacing = cellWidth * horizontalSpacingMultiplier;
        float verticalSpacing = cellHeight * verticalSpacingMultiplier;
        Vector2 startPos = new Vector2(
            -((cols - 1) * horizontalSpacing) / 2f,
             ((rows - 1) * verticalSpacing) / 2f
        );
        for (int i = 0; i < totalBottles; i++)
        {
            int row = i / cols;
            int col = i % cols;
            Vector2 spawnPos = startPos + new Vector2(col * horizontalSpacing, -row * verticalSpacing);
            GameObject bottle = Instantiate(bottlePrefab, spawnPos, Quaternion.identity, transform);
            ColorChanger cc = bottle.GetComponent<ColorChanger>();
            var data = levelData.bottles[i];
            cc.numberOfColorsInBottle = data.numberOfColorsInBottle;
            for (int j = 0; j < data.colors.Count && j < 4; j++)
                cc.bottleColors[j] = data.colors[j];
            // Use original cell size for bottle scale, not the reduced spacing
            float bottleScale = Mathf.Min(cellWidth, cellHeight) / 2f;
            bottle.transform.localScale = Vector3.one * bottleScale;
            cc.bottleScale = bottleScale;
            cc.bottleMaskSR.material.SetFloat("_BottleScale", bottleScale);
            cc.UpdateColorsOnShader();
            cc.UpdateTopColorValues();
        }
    }
}