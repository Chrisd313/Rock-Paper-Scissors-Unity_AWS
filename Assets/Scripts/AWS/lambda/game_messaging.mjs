import { DynamoDBClient } from "@aws-sdk/client-dynamodb";
import {
  ApiGatewayManagementApiClient,
  PostToConnectionCommand,
} from "@aws-sdk/client-apigatewaymanagementapi";
import { ScanCommand } from "@aws-sdk/lib-dynamodb";

const dynamoClient = new DynamoDBClient({ region: "eu-west-2" });
let send = undefined;
const environmentName = process.env.Environment;
const gameName = process.env.GameName;
const tableName = `${environmentName}-${gameName}-GameSessionsTable`;

// Op codes list
const REQUEST_START_OP = "1";
const REQUEST_ACCEPTED_OP = "2";
const MESSAGING_OP = "3";

function init(event) {
  const apiGatewayClient = new ApiGatewayManagementApiClient({
    endpoint:
      "https://" +
      event.requestContext.domainName +
      "/" +
      event.requestContext.stage,
  });

  send = async (connectionId, data) => {
    const input = {
      Data: data,
      ConnectionId: connectionId,
    };

    try {
      await apiGatewayClient.send(new PostToConnectionCommand(input));
    } catch (error) {
      console.log(error);
    }
  };
}

function getGameSession(playerId) {
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

  return dynamoClient.send(scanSessionTable);
}

export const handler = (event, context, callback) => {
  console.log("Event received: %j", event);
  init(event);

  let message = JSON.parse(event.body);
  console.log("Message received: %j", message);

  let connectionIdForCurrentRequest = event.requestContext.connectionId;

  if (message && message.opcode) {
    switch (message.opcode) {
      case REQUEST_START_OP:
        console.log(
          "REQUEST_START_OP received, checking if another player has joined the session."
        );

        getGameSession(connectionIdForCurrentRequest).then((data) => {
          console.log(
            "Successfully retrieved game session data - UUID: %j ",
            data.Items[0].uuid
          );

          if (
            data.Items[0].gameStatus != "closed" &&
            data.Items[0].player2 != "empty"
          ) {
            send(
              connectionIdForCurrentRequest,
              '{ "uuid": ' +
                data.Items[0].uuid +
                ', "opcode": ' +
                REQUEST_ACCEPTED_OP +
                " }"
            );

            console.log(
              "REQUEST_ACCEPTED_OP has been sent to player ID %j to initiate gameplay.",
              data.Items[0].uuid
            );
          }
        });

        break;

      case MESSAGING_OP:
        console.log("MESSAGING_OP has been received.");

        getGameSession(connectionIdForCurrentRequest).then((data) => {
          console.log(
            "Successfully retrieved game session data - %j ",
            data.Items[0].uuid
          );

          var sendToConnectionId = connectionIdForCurrentRequest;
          if (data.Items[0].player1 == connectionIdForCurrentRequest) {
            sendToConnectionId = data.Items[0].player2;
          } else {
            sendToConnectionId = data.Items[0].player1;
          }

          send(
            sendToConnectionId,
            '{ "uuid": ' +
              data.Items[0].uuid +
              ', "opcode": ' +
              MESSAGING_OP +
              ', "message": "' +
              message.message +
              '"}'
          );

          console.log(
            "Message has been sent to player ID %j",
            sendToConnectionId
          );
        });

        break;

      default:
      // no default case
    }
  }

  return callback(null, {
    statusCode: 200,
  });
};
