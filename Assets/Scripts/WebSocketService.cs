using UnityEngine;
using NativeWebSocket;
using System;

public class WebSocketService : Singleton<WebSocketService>
{
    [SerializeField]
    public GameLogic gameLogic;
    public const string webSocketDns = "";
    public const string REQUEST_START_OP = "1";
    public const string REQUEST_ACCEPTED_OP = "2";
    public const string MESSAGING_OP = "3";
    private bool intentionalClose = false;
    private WebSocket websocket;

    void Start()
    {
        websocket = new WebSocket(webSocketDns);
        SetupWebsocketCallbacks();
        intentionalClose = false;
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    // Establishes the connection's lifecycle callbacks.
    private void SetupWebsocketCallbacks()
    {
        websocket.OnOpen += () =>
        {
            intentionalClose = false;
            GameMessage.SendGameMessage(REQUEST_START_OP, "");
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
            try
            {
                string message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("Message received: " + message);
                ProcessReceivedMessage(message);
            }
            catch (Exception ex)
            {
                Debug.Log("Error handling message: " + ex.Message);
            }
        };
    }

    async public void FindMatch()
    {
        try
        {
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

        if (gameMessage.opcode == REQUEST_ACCEPTED_OP)
        {
            gameLogic.StartGame();
        }
        else if (gameMessage.opcode == MESSAGING_OP)
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
