using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Idle,
        GameOver
    }
    public GridGenrator gridGenrator;
    public int score = 0;
    public int turns = 0;
   // public CardTile firstCard;
   // public CardTile secondCard;

    private int cardMatchScore = 5;
    public GameState gameState = GameState.Idle;
    public static GameManager instance;
    public List<CardMatchmaker> cardMatchmakers = new List<CardMatchmaker>();
    private CardMatchmaker currentCardMatchmaker;
    public int streakMultiplier = 1;


    private void Awake()
    {
        Application.targetFrameRate = 60;
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    private void OnEnable()
    {
        GameEventsManager.cardRevalEvent.AddListener(OnCardReveal);
    }
    private void OnDisable()
    {
        GameEventsManager.cardRevalEvent.RemoveListener(OnCardReveal);
    }

    private void InitCardMatchmakerPool()
    {
        for (int i = 0; i < 10; i++)
        {
            CardMatchmaker cardMatchmaker = new CardMatchmaker();
            cardMatchmakers.Add(cardMatchmaker);
        }
    }
    private CardMatchmaker GetCardMatchmaker()
    {
        foreach (CardMatchmaker cardMatchmaker in cardMatchmakers)
        {
            if (cardMatchmaker.firstCard == null && cardMatchmaker.secondCard == null)
            {
                return cardMatchmaker;
            }
        }
        return null;
    }

    private void OnCardReveal(CardTile cardTile)
    {
        if (currentCardMatchmaker == null || (currentCardMatchmaker.firstCard != null && currentCardMatchmaker.secondCard != null))
        {
            currentCardMatchmaker = GetCardMatchmaker();
        }
       
        turns++;
        PlayerPrefs.SetInt("Turns", turns);
        if (currentCardMatchmaker.firstCard == null)
        {
            currentCardMatchmaker.firstCard = cardTile;
        }
        else
        {
            currentCardMatchmaker.secondCard = cardTile;
            CheckMatch(currentCardMatchmaker);
        }
    }
    private IEnumerator CheckMatchCards(CardMatchmaker cardMatchmaker)
    {
       
        yield return new WaitForSeconds(1);
        if (cardMatchmaker.firstCard.cardType == cardMatchmaker.secondCard.cardType)
        {
            //SoundManager.instance.PlaySound(SoundManager.instance.cardMatchSound);
            cardMatchmaker.firstCard.HideCard();
            cardMatchmaker.secondCard.HideCard();
            AddScore(cardMatchScore);
        }
        else
        {
            //  SoundManager.instance.PlaySound(SoundManager.instance.cardFlipSound);
            cardMatchmaker.firstCard.ResetCard();
            cardMatchmaker.secondCard.ResetCard();
            streakMultiplier = 1;
        }
        cardMatchmaker.firstCard = null;
        cardMatchmaker.secondCard = null;
        gameState = GameState.Idle;
        CheckForGameOver();
    }
    public void CheckMatch(CardMatchmaker cardMatchmaker)
    {
        StartCoroutine(CheckMatchCards(cardMatchmaker));
    }

    public void AddScore(int matchScore)
    {

        score += matchScore * streakMultiplier;
        streakMultiplier++;
    }

   

    // Method to load grid data
   

    public void StartNewGame(int gridWidth, int gridHeight)
    {
        InitCardMatchmakerPool();
        gridGenrator.gridWidth = gridWidth;
        gridGenrator.gridHeight = gridHeight;
        gridGenrator.GenrateCardGrid();
        score = 0;
        turns = 0;
        gameState = GameState.Idle;
    }
    public void ExitToMainMenu()
    {
        gridGenrator.ClearGrid();
        gameState = GameState.Idle;
    }

    public void CheckForGameOver()
    {
        if (gridGenrator.tiles.TrueForAll(t => t.isExposed))
        {
            gameState = GameState.GameOver;
            Debug.Log("Game Over");
        }
    }


    [Serializable]
    public class CellData
    {
        public int index;
        public bool isExposed;
        public int cardType;
    }

    [Serializable]
    public class CardMatchmaker
    {
        public CardTile firstCard;
        public CardTile secondCard;
    }
}
