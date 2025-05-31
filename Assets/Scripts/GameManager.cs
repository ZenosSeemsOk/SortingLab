using UnityEngine;

public class GameManager : MonoBehaviour
{
    private ColorChanger[] bottles;

    void Start()
    {
        bottles = FindObjectsOfType<ColorChanger>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) // Press G to check
        {
            if (AllBottlesInValidState())
            {
                Debug.Log("Game Over! All bottles are valid (either empty or filled with one color).");
                // TODO: Add win screen or next level logic
            }
            else
            {
                Debug.Log("Some bottles are not valid yet.");
            }
        }
    }

    private bool AllBottlesInValidState()
    {
        foreach (ColorChanger bottle in bottles)
        {
            if (!bottle.IsBottleInValidState())
                return false;
        }
        return true;
    }
}
