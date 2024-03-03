using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameMessage
{
    public string opcode;
    public string message;
    public string action;

    public static void SendGameMessage(string opCode, string message)
    {
        var newMessage = new GameMessage(opCode, message);
        WebSocketService.Instance.SendWebSocketMessage(JsonUtility.ToJson(newMessage));
    }

    public GameMessage(string opcodeIn, string messageIn)
    {
        action = "OnMessage";
        opcode = opcodeIn;
        message = messageIn;
    }
}
