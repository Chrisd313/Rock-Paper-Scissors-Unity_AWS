using UnityEngine;
using UnityEngine.UI;

public class ButtonFunctions : MonoBehaviour
{
    [SerializeField]
    private StatusMessage statusMessage = null;

    public void PlayOnline()
    {
        GameLogic.isPlayingOnline = true;
        WebSocketService.Instance.FindMatch();
    }

    public void PlayCPU()
    {
        GameLogic.isPlayingOnline = false;
        GameLogic.Instance.ChangeState(GameLogic.GameState.InPlay);
        GameLogic.Instance.StartGame();
    }

    public void SoftQuit()
    {
        UIManager.Instance.areYouSureModal.SetActive(true);
    }

    public void HardQuit()
    {
        GameLogic.Instance.ResetStatusMessage();
        GameLogic.Instance.ResetRound();
        GameLogic.Instance.gameOver = true;
        if (GameLogic.isPlayingOnline)
        {
            WebSocketService.Instance.QuitGame();
        }
        GameLogic.Instance.ChangeState(GameLogic.GameState.MainMenu);
        UIManager.Instance.areYouSureModal.SetActive(false);
    }

    public void CloseAreYouSureModal()
    {
        UIManager.Instance.areYouSureModal.SetActive(false);
    }

    public void Disconnected()
    {
        GameLogic.Instance.gameOver = true;
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
