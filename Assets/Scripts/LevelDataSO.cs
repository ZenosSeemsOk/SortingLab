// Updated LevelData ScriptableObject
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "ColorSort/LevelData")]
public class LevelData : ScriptableObject
{
    public List<BottleData> bottles;
}

[System.Serializable]
public class BottleData
{
    public List<Color> colors; // Max 4
    [Range(0, 4)]
    public int numberOfColorsInBottle; // Explicitly passed
}