using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private GameLogic gameLogic;

    public GameObject Selected;
    public GameObject Choices;

    public GameObject player1Panel;
    public GameObject player2Panel;

    [SerializeField]
    private Sprite[] choicesSprites;

    [SerializeField]
    private Sprite[] _playerLiveSprites;

    [SerializeField]
    private Sprite[] _opponentsLiveSprites;

    [SerializeField]
    private Image _playerLivesImage;

    [SerializeField]
    private Image _opponentsLivesImage;

    [SerializeField]
    private Sprite blankImage;

    [SerializeField]
    private GameObject[] statePanels;

    [SerializeField]
    private GameObject inPlayQuitBtn;

    [SerializeField]
    public GameObject areYouSureModal;

    public void Start()
    {
        HideSelectedChoices();
    }

    public void ShowSelectedChoices()
    {
        Selected.SetActive(true);
        Choices.SetActive(false);
    }

    public void ShowChoices()
    {
        Selected.SetActive(false);
        Choices.SetActive(true);
    }

    public void HideQuitBtn()
    {
        inPlayQuitBtn.SetActive(false);
    }

    public void ShowQuitBtn()
    {
        inPlayQuitBtn.SetActive(true);
    }

    public void HideSelectedChoices()
    {
        Selected.SetActive(false);
    }

    public void SetPlayerPanel(string player, string selected)
    {
        var panel =
            (player == "Player1")
                ? player1Panel.GetComponent<Image>()
                : (player == "Player2")
                    ? player2Panel.GetComponent<Image>()
                    : (Image)null;

        if (panel == null)
        {
            Debug.Log("Error");
            return;
        }

        for (int i = 0; i < choicesSprites.Length; ++i)
        {
            if (choicesSprites[i] != null && string.Equals(choicesSprites[i].name, selected))
            {
                panel.sprite = choicesSprites[i];
            }
        }
    }

    public void EmptyChoicePanels()
    {
        var P1Panel = player1Panel.GetComponent<Image>();
        var P2Panel = player2Panel.GetComponent<Image>();
        P1Panel.sprite = P2Panel.sprite = blankImage;
    }

    public void updateLives(int playerLives, int opponentsLives)
    {
        _playerLivesImage.sprite = _playerLiveSprites[playerLives];
        _opponentsLivesImage.sprite = _opponentsLiveSprites[opponentsLives];
    }

    public void SetUIState()
    {
        var gameState = "";

        // Reset all panels to false
        foreach (GameObject i in statePanels)
        {
            i.SetActive(false);
        }

        // Check what the current Game State is and set the gameState variable to it
        switch (GameLogic.state)
        {
            case GameLogic.GameState.MainMenu:
                gameState = "MainMenu";
                break;
            case GameLogic.GameState.InPlay:
                gameState = "InPlay";
                break;
            case GameLogic.GameState.YouWin:
                gameState = "YouWin";
                break;
            case GameLogic.GameState.YouLose:
                gameState = "YouLose";
                break;
            case GameLogic.GameState.Searching:
                gameState = "Searching";
                break;
            case GameLogic.GameState.Disconnected:
                gameState = "Disconnected";
                break;
            case GameLogic.GameState.ConnectionUnsuccessful:
                gameState = "ConnectionUnsuccessful";
                break;
            case GameLogic.GameState.AnErrorOccurred:
                gameState = "AnErrorOccurred";
                break;
        }

        // Check through each of the statePanels, and if the current state matches the name set it to active
        foreach (GameObject i in statePanels)
        {
            if (i.name == gameState)
            {
                i.SetActive(true);
            }
        }
    }
}
