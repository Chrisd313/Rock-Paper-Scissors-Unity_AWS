using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameLogic : Singleton<GameLogic>
{
    public StatusMessage statusMessage;
    public UIManager uiManager;
    public string playerChoice = "";
    public string opponentsChoice = "";
    public int playerLives = 3;
    public int opponentLives = 3;
    public bool isWinner;
    public bool gameOver;
    public static GameState state;
    public static bool isPlayingOnline;
    public int countdownTime;
    public Text countdownDisplay;

    public enum GameState
    {
        MainMenu,
        InPlay,
        YouWin,
        YouLose,
        Searching,
        Disconnected,
        ConnectionUnsuccessful,
        AnErrorOccurred
    }

    public void Start()
    {
        state = GameState.MainMenu;
        uiManager.HideSelectedChoices();
    }

    public void StartGame()
    {
        gameOver = false;
        ChangeState(GameState.InPlay);
        statusMessage.SetText(StatusMessage.Playing);
        SetLives();
        uiManager.ShowChoices();
    }

    public void SelectChoice(string selectedChoice)
    {
        ChangeState(GameState.InPlay);

        playerChoice = selectedChoice;

        uiManager.ShowSelectedChoices();
        uiManager.SetPlayerPanel("Player1", selectedChoice);
        uiManager.ShowQuitBtn();

        if (isPlayingOnline)
        {
            GameMessage.SendGameMessage(WebSocketService.MessagingOp, playerChoice);
        }
        else
        {
            opponentsChoice = RandomChoice();
        }

        CompareChoices();
    }

    public void CompareChoices()
    {
        // Check if both players have made a choice.
        if (playerChoice != "" && opponentsChoice != "")
        {
            uiManager.SetPlayerPanel("Player2", opponentsChoice);

            // Check the choices to determine the winner.
            switch (opponentsChoice)
            {
                case "Rock":
                    switch (playerChoice)
                    {
                        case "Rock":
                            statusMessage.SetText("It's a tie!");
                            RoundOutcome.Instance.Outcome("Draw");
                            break;

                        case "Paper":
                            statusMessage.SetText("The paper covers the rock!");
                            RoundOutcome.Instance.Outcome("Win");
                            opponentLives--;
                            break;

                        case "Scissors":
                            statusMessage.SetText("The rock destroys the scissors!");
                            RoundOutcome.Instance.Outcome("Lose");
                            playerLives--;
                            break;
                    }
                    break;

                case "Paper":
                    switch (playerChoice)
                    {
                        case "Rock":
                            statusMessage.SetText("The paper covers the rock!");
                            playerLives--;
                            RoundOutcome.Instance.Outcome("Lose");
                            break;

                        case "Paper":
                            statusMessage.SetText("It's a tie!");
                            RoundOutcome.Instance.Outcome("Draw");
                            break;

                        case "Scissors":
                            statusMessage.SetText("The scissors cuts the paper!");
                            RoundOutcome.Instance.Outcome("Win");
                            opponentLives--;
                            break;
                    }
                    break;

                case "Scissors":
                    switch (playerChoice)
                    {
                        case "Rock":
                            statusMessage.SetText("The rock destroys the scissors!");
                            RoundOutcome.Instance.Outcome("Win");
                            opponentLives--;
                            break;

                        case "Paper":
                            statusMessage.SetText("The scissors cuts the paper!");
                            RoundOutcome.Instance.Outcome("Lose");
                            playerLives--;
                            break;

                        case "Scissors":
                            statusMessage.SetText("It's a tie!");
                            RoundOutcome.Instance.Outcome("Draw");
                            break;
                    }
                    break;
            }

            uiManager.updateLives(playerLives, opponentLives);

            if (playerLives == 0 || opponentLives == 0)
            {
                if (GameLogic.isPlayingOnline)
                {
                    WebSocketService.Instance.EndConnection();
                }
                uiManager.HideQuitBtn();
                Result();
            }
            else
            {
                StartCoroutine(CountdownToRoundReset());
            }
        }
    }

    IEnumerator CountdownToRoundReset()
    {
        countdownTime = 3;
        while (countdownTime > 0 & !gameOver)
        {
            countdownDisplay.text = countdownTime.ToString();

            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        // Clear players choices.
        playerChoice = "";
        opponentsChoice = "";

        // Reset viewable UI.
        countdownDisplay.text = "";
        RoundOutcome.Instance.Outcome("");
        RoundOutcome.Instance.HideAll();

        ResetRound();
    }

    public void ResetRound()
    {
        uiManager.ShowChoices();
        if (!gameOver)
        {
            statusMessage.SetText(StatusMessage.Playing);
        }
        uiManager.EmptyChoicePanels();
    }

    private void Result()
    {
        gameOver = true;
        if (playerLives == 0)
        {
            isWinner = false;
        }
        else if (opponentLives == 0)
        {
            isWinner = true;
        }
        StartCoroutine(SetResultPanel());
    }

    IEnumerator SetResultPanel()
    {
        yield return new WaitForSeconds(2);
        RoundOutcome.Instance.HideAll();
        statusMessage.SetText("");
        if (isWinner)
        {
            ChangeState(GameState.YouWin);
        }
        else
        {
            ChangeState(GameState.YouLose);
        }
    }

    public void SetLives()
    {
        playerLives = 3;
        opponentLives = 3;
        uiManager.updateLives(playerLives, opponentLives);
    }

    public void ChangeState(GameState newState)
    {
        state = newState;
        uiManager.SetUIState();
    }

    public string RandomChoice()
    {
        int computerChoice = Random.Range(1, 4);

        switch (computerChoice)
        {
            case 1:
                return "Rock";
            case 2:
                return "Paper";
            case 3:
                return "Scissors";
            default:
                return "error";
        }
    }

    public void ResetStatusMessage()
    {
        statusMessage.SetText("");
    }
}
