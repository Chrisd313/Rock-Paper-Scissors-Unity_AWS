const AWS = require("aws-sdk");
const ddb = new AWS.DynamoDB.DocumentClient();
const environmentName = process.env.Environment;
const gameName = process.env.GameName;
const tableName = `${environmentName}-${gameName}-GameSessionsTable`;
let disconnectWs = undefined;

function init(event) {
  console.log(event);
  const apigwManagementApi = new AWS.ApiGatewayManagementApi({
    apiVersion: "2018-11-29",
    endpoint:
      event.requestContext.domainName + "/" + event.requestContext.stage,
  });
  disconnectWs = async (connectionId) => {
    await apigwManagementApi
      .deleteConnection({ ConnectionId: connectionId })
      .promise();
  };
}

function getGameSession(playerId) {
  return ddb
    .scan({
      TableName: tableName,
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

async function closeGame(uuid) {
  ddb
    .delete({
      TableName: tableName,
      Key: {
        uuid: uuid,
      },
    })
    .promise();
  console.log(
    "Successfully deleted game session " + uuid + "from " + tableName
  );
}

exports.handler = (event, context, callback) => {
  console.log("Disconnect event received: %j", event);
  init(event);

  const connectionIdForCurrentRequest = event.requestContext.connectionId;
  console.log("Request from player: " + connectionIdForCurrentRequest);

  getGameSession(connectionIdForCurrentRequest).then((data) => {
    console.log("getGameSession: " + data.Items[0].uuid);

    if (data.Items[0].player1 == connectionIdForCurrentRequest) {
      // player1 disconnected, now disconnect player 2
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
        console.log("Player2 was never filled");
      }
    } else {
      // player2 disconnected, now disconnect player 1
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
    closeGame(data.Items[0].uuid);
  });

  return callback(null, { statusCode: 200 });
};
