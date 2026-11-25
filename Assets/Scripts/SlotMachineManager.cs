using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotMachineManager : MonoBehaviour
{
    [Header("Reels & Symbol Config")]
    [SerializeField] private Reel[] reels; // Array of 3 reels (Reel_0, Reel_1, Reel_2)
    [SerializeField] private List<SymbolData> symbolsConfig;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI betText;
    [SerializeField] private Button spinButton;
    [SerializeField] private Button betIncreaseBtn;
    [SerializeField] private Button betDecreaseBtn;

    [Header("Economy Settings")]
    [SerializeField] private int playerBalance = 1000;
    [SerializeField] private int currentBet = 10;
    [SerializeField] private int minBet = 5;
    [SerializeField] private int maxBet = 100;
    [SerializeField] private int betStep = 5;

    private bool isGameSpinning = false;

    private void Start()
    {
        UpdateUI();

        // Initialize reels with symbols
        foreach (Reel reel in reels)
        {
            reel.SetupReel(symbolsConfig);
        }

        // Hook up button listeners
        spinButton.onClick.AddListener(OnSpinClicked);
        betIncreaseBtn.onClick.AddListener(IncreaseBet);
        betDecreaseBtn.onClick.AddListener(DecreaseBet);
    }

    private void OnSpinClicked()
    {
        if (isGameSpinning) return;
        if (playerBalance < currentBet)
        {
            winText.text = "INSUFFICIENT FUNDS!";
            return;
        }

        StartCoroutine(SpinAllReels());
    }

    private IEnumerator SpinAllReels()
    {
        isGameSpinning = true;
        spinButton.interactable = false;
        betIncreaseBtn.interactable = false;
        betDecreaseBtn.interactable = false;

        playerBalance -= currentBet;
        winText.text = "SPINNING...";
        UpdateUI();

        // Determine outcome before animation starts
        SymbolData[] outcomes = new SymbolData[3];
        for (int i = 0; i < 3; i++)
        {
            outcomes[i] = symbolsConfig[Random.Range(0, symbolsConfig.Count)];
        }

        // Start reel spin animations with staggered stop delays
        Coroutine r0 = StartCoroutine(reels[0].Spin(0.0f, outcomes[0]));
        Coroutine r1 = StartCoroutine(reels[1].Spin(0.5f, outcomes[1]));
        Coroutine r2 = StartCoroutine(reels[2].Spin(1.0f, outcomes[2]));

        yield return r0;
        yield return r1;
        yield return r2;

        EvaluateResult(outcomes);

        isGameSpinning = false;
        spinButton.interactable = true;
        UpdateBetButtonsState();
    }

    private void EvaluateResult(SymbolData[] finalSymbols)
    {
        string sym0 = finalSymbols[0].symbolName;
        string sym1 = finalSymbols[1].symbolName;
        string sym2 = finalSymbols[2].symbolName;

        int winAmount = 0;

        if (sym0 == sym1 && sym1 == sym2)
        {
            // 3 of a kind (Jackpot / Full payout)
            winAmount = currentBet * finalSymbols[0].payoutMultiplier;
            winText.text = $"JACKPOT! +${winAmount}";
        }
        else if (sym0 == sym1 || sym1 == sym2 || sym0 == sym2)
        {
            // 2 of a kind (Partial payout)
            SymbolData matchingSymbol = (sym0 == sym1) ? finalSymbols[0] : finalSymbols[1];
            winAmount = Mathf.Max(currentBet, (currentBet * matchingSymbol.payoutMultiplier) / 2);
            winText.text = $"2 OF A KIND! +${winAmount}";
        }
        else
        {
            winText.text = "TRY AGAIN!";
        }

        playerBalance += winAmount;
        UpdateUI();
    }

    private void IncreaseBet()
    {
        if (isGameSpinning) return;
        currentBet = Mathf.Min(currentBet + betStep, maxBet);
        UpdateUI();
    }

    private void DecreaseBet()
    {
        if (isGameSpinning) return;
        currentBet = Mathf.Max(currentBet - betStep, minBet);
        UpdateUI();
    }

    private void UpdateUI()
    {
        balanceText.text = $"BALANCE: ${playerBalance}";
        betText.text = $"BET: ${currentBet}";
        UpdateBetButtonsState();
    }

    private void UpdateBetButtonsState()
    {
        betIncreaseBtn.interactable = !isGameSpinning && (currentBet < maxBet);
        betDecreaseBtn.interactable = !isGameSpinning && (currentBet > minBet);
    }
}