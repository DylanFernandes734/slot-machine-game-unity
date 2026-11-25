using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image[] slots; // The 4 Image slots (Slot_0, Slot_1, Slot_2, Slot_3)

    [Header("Settings")]
    [SerializeField] private float minSpinDuration = 2.0f;
    [SerializeField] private float spinSpeed = 1200f;
    [SerializeField] private float slotHeight = 160f;

    private List<SymbolData> availableSymbols;
    private bool isSpinning = false;
    private SymbolData finalSymbol;

    public bool IsSpinning => isSpinning;
    public SymbolData FinalSymbol => finalSymbol;

    public void SetupReel(List<SymbolData> symbols)
    {
        availableSymbols = symbols;
        InitializeSymbolsRandomly();
    }

    private void InitializeSymbolsRandomly()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            SymbolData randomSym = availableSymbols[Random.Range(0, availableSymbols.Count)];
            slots[i].sprite = randomSym.symbolSprite;
        }
        finalSymbol = availableSymbols[0]; // Default initial fallback
    }

    public IEnumerator Spin(float extraDelay, SymbolData targetSymbol)
    {
        isSpinning = true;
        finalSymbol = targetSymbol;

        float elapsedTime = 0f;
        float totalDuration = minSpinDuration + extraDelay;

        while (elapsedTime < totalDuration)
        {
            // Move slots downwards
            foreach (Image slot in slots)
            {
                slot.rectTransform.anchoredPosition += Vector2.down * spinSpeed * Time.deltaTime;
            }

            // Loop slots that go below the mask boundary
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].rectTransform.anchoredPosition.y <= -slotHeight)
                {
                    // Move it back to the top of the stack
                    slots[i].rectTransform.anchoredPosition = new Vector2(
                        slots[i].rectTransform.anchoredPosition.x,
                        slots[i].rectTransform.anchoredPosition.y + (slots.Length * slotHeight)
                    );

                    // Assign a random symbol while spinning
                    SymbolData randomSym = availableSymbols[Random.Range(0, availableSymbols.Count)];
                    slots[i].sprite = randomSym.symbolSprite;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Snap smoothly to the winning target symbol at center (Slot_1 at Y = 0)
        yield return StartCoroutine(SnapToTarget(targetSymbol));

        isSpinning = false;
    }

    private IEnumerator SnapToTarget(SymbolData targetSymbol)
    {
        // Place target symbol at payline (Slot_1, Y = 0)
        slots[1].sprite = targetSymbol.symbolSprite;
        slots[1].rectTransform.anchoredPosition = new Vector2(slots[1].rectTransform.anchoredPosition.x, 0);

        // Place other slots in their rest positions around it
        slots[0].rectTransform.anchoredPosition = new Vector2(slots[0].rectTransform.anchoredPosition.x, slotHeight);
        slots[2].rectTransform.anchoredPosition = new Vector2(slots[2].rectTransform.anchoredPosition.x, -slotHeight);
        slots[3].rectTransform.anchoredPosition = new Vector2(slots[3].rectTransform.anchoredPosition.x, slotHeight * 2);

        // Randomize non-payline symbols for visual realism
        slots[0].sprite = availableSymbols[Random.Range(0, availableSymbols.Count)].symbolSprite;
        slots[2].sprite = availableSymbols[Random.Range(0, availableSymbols.Count)].symbolSprite;
        slots[3].sprite = availableSymbols[Random.Range(0, availableSymbols.Count)].symbolSprite;

        yield return null;
    }
}