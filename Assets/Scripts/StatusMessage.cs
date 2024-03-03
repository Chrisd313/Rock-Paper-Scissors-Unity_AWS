using UnityEngine;
using UnityEngine.UI;

public class StatusMessage : MonoBehaviour
{
    public const string WaitingOnMatch = "Waiting on match...";
    public const string YouWon = "You Won!";
    public const string YouLost = "You Lost!";
    public const string Playing = "Shoot!";
    public const string GameOver = "Game Over";
    public const string Searching = "Finding an opponent...";
    public const string Null = "";
    public const string Disconnected = "Connection to the other player has been lost!";

    private Text _outcomeText;

    void Start()
    {
        _outcomeText = this.GetComponent<Text>();
    }

    public void SetText(string textInput)
    {
        _outcomeText = this.GetComponent<Text>();

        _outcomeText.text = textInput;
    }
}
