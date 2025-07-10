using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public Button[] levelButtons;
    public Image[] buttonImages;

    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;

    [SerializeField] private AudioClip buttonClickSound;

    private void Start()
    {

        foreach (var button in levelButtons)
        {
            button.enabled = false;
        }
        foreach (var image in buttonImages)
        {
            image.sprite = lockedSprite;
        }

        ButtonUnlock();
    }


    public void PickLevel(int id)
    {
        MusicManager.Instance.PlaySFX(buttonClickSound, 0.6f);
        LevelSelection.Instance.currentLevel = id;
        SceneManager.LoadScene("GameScene");
    }

    public void ButtonUnlock()
    {
        if (LevelSelection.Instance.totalUnlockedLevels <= levelButtons.Length)
        {
            for (int i = 0; i < LevelSelection.Instance.totalUnlockedLevels; i++)
            {
                buttonImages[i].sprite = unlockedSprite;
                levelButtons[i].enabled = true;
            }
        }
        else
        {
            foreach (var button in levelButtons)
            {
                button.enabled = true;
            }
            foreach (var image in buttonImages)
            {
                image.sprite = unlockedSprite;
            }
        }
    }


    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("UnlockedLevels");
        LevelSelection.Instance.totalUnlockedLevels = 1;
    }

    public void MainMenu()
    {
        MusicManager.Instance.PlaySFX(buttonClickSound, 0.6f);
        SceneManager.LoadScene("Menu Scene");
    }

    public void UnlockAllLevels()
    {
        LevelSelection.Instance.totalUnlockedLevels = LevelSelection.Instance.currentMaxLevels;
    }
}
