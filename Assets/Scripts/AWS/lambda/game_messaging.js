const AWS = require("aws-sdk");
const ddb = new AWS.DynamoDB.DocumentClient();
let send = undefined;
const environmentName = process.env.Environment;
const gameName = process.env.GameName;
const TABLE_NAME = `${environmentName}-${gameName}-GameSessionsTable`;

// Op codes list
const REQUEST_START_OP = "1";
const PLAYING_OP = "2";
const MESSAGING_OP = "3";

function init(event) {
  const apigwManagementApi = new AWS.ApiGatewayManagementApi({
    apiVersion: "2018-11-29",
    endpoint:
      event.requestContext.domainName + "/" + event.requestContext.stage,
  });
  send = async (connectionId, data) => {
    await apigwManagementApi
      .postToConnection({
        ConnectionId: connectionId,
        Data: `${data}`,
      })
      .promise();
  };
}

function getGameSession(playerId) {
  return ddb
    .scan({
      TableName: TABLE_NAME,
      FilterExpression: "#p1 = :playerId or #p2 = :playerId",
      ExpressionAttributeNames: {
        "#p1": "player1",
        "#p2": "player2",
      },
      ExpressionAttributeValues: {
        ":playerId": playerId,
      },
    })
    .promise();
}

exports.handler = (event, context, callback) => {
  console.log("Event received: %j", event);
  init(event);

  let message = JSON.parse(event.body);
  console.log("Message received: %j", message);

  let connectionIdForCurrentRequest = event.requestContext.connectionId;
  console.log("Current connection id: " + connectionIdForCurrentRequest);

  if (message && message.opcode) {
    switch (message.opcode) {
      case REQUEST_START_OP:
        getGameSession(connectionIdForCurrentRequest).then((data) => {
          console.log("getGameSession: " + data.Items[0].uuid);

          // we check for closed to handle an edge case where if player1 joins and immediately quits,
          // we mark closed to make sure a player2 can't join an abandoned game session
          var opcodeStart = "0";
          if (
            data.Items[0].gameStatus != "closed" &&
            data.Items[0].player2 != "empty"
          ) {
            opcodeStart = PLAYING_OP;
          }

          send(
            connectionIdForCurrentRequest,
            '{ "uuid": ' +
              data.Items[0].uuid +
              ', "opcode": ' +
              opcodeStart +
              " }"
          );
        });

        break;

      case MESSAGING_OP:
        getGameSession(connectionIdForCurrentRequest).then((data) => {
          console.log("getGameSession: %j", data.Items[0]);

          var sendToConnectionId = connectionIdForCurrentRequest;
          if (data.Items[0].player1 == connectionIdForCurrentRequest) {
            // request came from player1, just send out to player2
            sendToConnectionId = data.Items[0].player2;
          } else {
            // request came from player2, just send out to player1
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
