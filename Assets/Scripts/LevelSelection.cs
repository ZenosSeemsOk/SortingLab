using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelection : MonoBehaviour
{
    public int currentMaxLevels;
    public static LevelSelection Instance { get; private set; }

    public int totalUnlockedLevels;
    public int currentLevel;

    private void Awake()
    {    
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved unlocked levels
            totalUnlockedLevels = PlayerPrefs.GetInt("UnlockedLevels", 1); // Default to 1 if not saved yet
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void PickLevel(int id)
    {
        currentLevel = id;
        SceneManager.LoadScene("GameScene");
    }

}
