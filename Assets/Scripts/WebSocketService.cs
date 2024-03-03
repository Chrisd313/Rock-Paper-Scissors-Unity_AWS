using UnityEngine;
using NativeWebSocket;
using System;

public class WebSocketService : Singleton<WebSocketService>
{
    [SerializeField]
    public GameLogic gameLogic;
    public const string webSocketDns = "";
    public bool connectionEstablished;
    public const string RequestStartOp = "1";
    public const string PlayingOp = "2";
    public const string MessagingOp = "3";
    private bool intentionalClose = false;
    private WebSocket websocket;

    void Start()
    {
        intentionalClose = false;
    }

    void Update()
    {
        if (connectionEstablished)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
#endif
        }
    }

    // Establishes the connection's lifecycle callbacks.
    private void SetupWebsocketCallbacks()
    {
        websocket.OnOpen += () =>
        {
            intentionalClose = false;
            GameMessage.SendGameMessage(RequestStartOp, "");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("An error has occurred:  " + e);
            gameLogic.ChangeState(GameLogic.GameState.AnErrorOccurred);
        };

        websocket.OnClose += (e) =>
        {
            if (!intentionalClose)
            {
                if (!gameLogic.gameOver)
                {
                    gameLogic.ChangeState(GameLogic.GameState.Disconnected);
                }
            }
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            ProcessReceivedMessage(message);
        };
    }

    async public void FindMatch()
    {
        try
        {
            websocket = new WebSocket(webSocketDns);
            SetupWebsocketCallbacks();
            GameLogic.Instance.ChangeState(GameLogic.GameState.Searching);
            await websocket.Connect();
        }
        catch (System.Exception ex)
        {
            Debug.Log("An error occurred while establishing the connection.");
            GameLogic.Instance.ChangeState(GameLogic.GameState.ConnectionUnsuccessful);
        }
    }

    private void ProcessReceivedMessage(string message)
    {
        GameMessage gameMessage = JsonUtility.FromJson<GameMessage>(message);
        Debug.Log("Message reecived: " + message);

        if (gameMessage.opcode == PlayingOp)
        {
            gameLogic.StartGame();
        }
        else if (gameMessage.opcode == MessagingOp)
        {
            gameLogic.opponentsChoice = gameMessage.message;
            gameLogic.CompareChoices();
        }
    }

    public async void SendWebSocketMessage(string message)
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
        }
    }

    public async void QuitGame()
    {
        intentionalClose = true;
        await websocket.Close();
    }

    public async void EndConnection()
    {
        await websocket.Close();
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
