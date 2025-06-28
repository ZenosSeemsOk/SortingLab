using UnityEngine;

public class Coins : MonoBehaviour
{
    public int amount;
    public static Coins Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }

}
