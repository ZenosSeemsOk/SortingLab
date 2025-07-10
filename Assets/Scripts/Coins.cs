// 6/29/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class Coins : MonoBehaviour
{
    public int amount { get; private set; } // Make amount private to enforce encapsulation
    public static Coins Instance { get; private set; }

    private const string CoinsKey = "Coins"; // Key for saving coins in PlayerPrefs


    public event System.Action<int> OnCoinsChanged;

    public void AddCoins(int value)
    {
        amount += value;
        SaveCoins();
        OnCoinsChanged?.Invoke(amount);
    }

    public bool SpendCoins(int value)
    {
        if (amount >= value)
        {
            amount -= value;
            SaveCoins();
            OnCoinsChanged?.Invoke(amount);
            return true;
        }
        return false;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadCoins();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(CoinsKey, amount);
        PlayerPrefs.Save();
    }

    private void LoadCoins()
    {
        amount = PlayerPrefs.GetInt(CoinsKey, 0); // Default to 0 coins
    }
}