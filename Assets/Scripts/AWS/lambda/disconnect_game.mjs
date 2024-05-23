import { DynamoDBClient, DeleteItemCommand } from "@aws-sdk/client-dynamodb";
import { ScanCommand } from "@aws-sdk/lib-dynamodb";
import {
  ApiGatewayManagementApiClient,
  DeleteConnectionCommand,
} from "@aws-sdk/client-apigatewaymanagementapi";
const dynamoClient = new DynamoDBClient({ region: "eu-west-2" });
const environmentName = process.env.Environment;
const gameName = process.env.GameName;
const tableName = `${environmentName}-${gameName}-GameSessionsTable`;
let disconnectWs = undefined;

function init(event) {
  const apiGatewayClient = new ApiGatewayManagementApiClient({
    endpoint:
      "https://" +
      event.requestContext.domainName +
      "/" +
      event.requestContext.stage,
  });

  disconnectWs = async (connectionId) => {
    const input = {
      ConnectionId: connectionId,
    };
    const deleteCommand = new DeleteConnectionCommand(input);

    try {
      await apiGatewayClient.send(deleteCommand);
      console.log("Successfully disconnected connection ID %j", connectionId);
    } catch (error) {
      console.log(error);
    }
  };
}

function getGameSession(playerId) {
  console.log(tableName);
  const scanParams = {
    TableName: tableName,
    FilterExpression: "#p1 = :playerId or #p2 = :playerId",
    ExpressionAttributeNames: {
      "#p1": "player1",
      "#p2": "player2",
    },
    ExpressionAttributeValues: {
      ":playerId": playerId,
    },
  };

  const scanSessionTable = new ScanCommand(scanParams);
  try {
    return dynamoClient.send(scanSessionTable);
  } catch (error) {
    console.log(error);
  }
}

async function deleteSession(uuid) {
  const input = {
    Key: {
      uuid: {
        S: uuid,
      },
    },
    TableName: tableName,
  };
  const command = new DeleteItemCommand(input);
  try {
    await dynamoClient.send(command);
    console.log(
      "Successfully deleted game session " + uuid + "from " + tableName
    );
  } catch (error) {
    console.log(error);
  }
}

export const handler = (event, context, callback) => {
  console.log("Disconnect event received: %j", event);
  init(event);

  const connectionIdForCurrentRequest = event.requestContext.connectionId;
  console.log("Request from player ID %j", connectionIdForCurrentRequest);

  getGameSession(connectionIdForCurrentRequest).then((data) => {
    console.log(data);
    console.log(
      "Successfully retrieved game session data - UUID: %j ",
      data.Items[0].uuid
    );

    // Check which player has disconnected from the session and disconnect the other.
    if (data.Items[0].player1 == connectionIdForCurrentRequest) {
      if (data.Items[0].player2 !== "empty") {
        console.log("Disconnecting player 2: " + data.Items[0].player2);

        disconnectWs(data.Items[0].player2).then(
          () => {},
          (err) => {
            console.log(
              "Error closing connection, player 2 probably already closed."
            );
            console.log(err);
          }
        );
      } else {
        console.log("Player 2 was never filled");
      }
    } else {
      console.log("Disconnecting player 1: " + data.Items[0].player1);

      disconnectWs(data.Items[0].player1).then(
        () => {},
        (err) => {
          console.log(
            "Error closing connection, player 1 probably already closed."
          );
          console.log(err);
        }
      );
    }
    deleteSession(data.Items[0].uuid);
  });

  return callback(null, { statusCode: 200 });
};
