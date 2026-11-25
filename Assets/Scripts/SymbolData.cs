using UnityEngine;

[System.Serializable]
public class SymbolData
{
    public string symbolName;
    public Sprite symbolSprite;
    public int payoutMultiplier; // e.g. 7 pays 10x, Cherries pays 5x, Bell 3x, BAR 2x
}