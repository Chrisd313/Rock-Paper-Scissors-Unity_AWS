import {
  DynamoDBClient,
  PutItemCommand,
  UpdateItemCommand,
} from "@aws-sdk/client-dynamodb";
import { ScanCommand } from "@aws-sdk/lib-dynamodb";
import {
  ApiGatewayManagementApiClient,
  PostToConnectionCommand,
} from "@aws-sdk/client-apigatewaymanagementapi";

const dynamoClient = new DynamoDBClient({ region: "eu-west-2" });

let send = undefined;
const environmentName = process.env.Environment;
const gameName = process.env.GameName;
const TABLE_NAME = `${environmentName}-${gameName}-GameSessionsTable`;
const PLAYING_OP = "2";

export const handler = (event, context, callback) => {
  const connectionId = event.requestContext.connectionId;

  console.log("Connect event received: %j", event);
  init(event);

  addConnectionId(connectionId).then(() => {
    callback(null, {
      statusCode: 200,
    });
  });
};

function init(event) {
  var endpointUrl =
    "https://" +
    event.requestContext.domainName +
    "/" +
    event.requestContext.stage;

  var apiGatewayClient = new ApiGatewayManagementApiClient({
    endpoint: endpointUrl,
  });

  send = async (connectionId, data) => {
    console.log("sending message to " + connectionId);
    const input = {
      Data: data,
      ConnectionId: connectionId,
    };

    const command = new PostToConnectionCommand(input);

    try {
      await apiGatewayClient.send(command);
    } catch (error) {
      console.log(error);
    }
  };
}

const scanParams = {
  TableName: TABLE_NAME,
  FilterExpression: "#p2 = :empty and #status <> :closed",
  ExpressionAttributeNames: {
    "#p2": "player2",
    "#status": "gameStatus",
  },
  ExpressionAttributeValues: {
    ":empty": "empty",
    ":closed": "closed",
  },
};

const scanSessionTable = new ScanCommand(scanParams);

function getAvailableGameSession() {
  return dynamoClient.send(scanSessionTable);
}

function addConnectionId(connectionId) {
  return getAvailableGameSession().then((data) => {
    console.log("Game session data: %j", data);

    if (data && data.Count < 1) {
      // Create new game session.
      console.log("No sessions are available to join, creating a new session.");
      const createSessionParams = {
        TableName: TABLE_NAME,
        Item: {
          uuid: { S: Date.now() + "" },
          player1: { S: connectionId },
          player2: { S: "empty" },
        },
      };

      const createSession = new PutItemCommand(createSessionParams);

      return dynamoClient.send(createSession);
    } else {
      // Join existing game session.
      console.log("Available session found, adding user as player 2.");

      const joinSessionParams = {
        TableName: TABLE_NAME,
        Key: {
          uuid: { S: data.Items[0].uuid },
        },
        UpdateExpression: "set player2 = :p2",
        ExpressionAttributeValues: {
          ":p2": { S: connectionId },
        },
      };

      const joinSession = new UpdateItemCommand(joinSessionParams);

      return dynamoClient
        .send(joinSession)
        .then(() => {
          // Inform player 1 that player 2 has joined to initiate the start of game.
          console.log(
            "Notifying player 1 that player 2 has joined the session."
          );
          send(
            data.Items[0].player1,
            JSON.stringify({ uuid: data.Items[0].uuid, opcode: PLAYING_OP })
          );
        })
        .catch((error) => {
          console.error(error);
        });
    }
  });
}
