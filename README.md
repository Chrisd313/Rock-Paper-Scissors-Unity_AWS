# Rock, Paper, Scissors - Multiplayer in Unity with AWS

## Introduction

This Unity project utilises Amazon Web Services (AWS) to implement simple two player multiplayer gameplay. While this is not recommended as a final solution to add multiplayer functionality to your Unity game, it is a relatively cheap way to build an early multiplayer prototype.

Firstly, I must give credit to Youtuber [BatteryAcidDev](https://www.youtube.com/@BatteryAcidDev). In 2020, he created a two part video series ([part 1](https://www.youtube.com/watch?v=X45VYma6738), [part 2](https://www.youtube.com/watch?v=X45VYma6738)) outlining how to create a multiplayer instance using AWS services and Websockets. His videos formed the basis of this project, and are certainly worth the watch.

My project demo is a 2 person multiplayer game of Rock, Paper, Scissors. Throughout this article we will cover:

- How multiplayer was integrated into the game with the use of [Websockets](https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API) and AWS services (APIGateway, Lambda and DynamoDB)
- Where to download the project and deploy it via [AWS CloudFormation](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/Welcome.html) and the [Grunt.js](https://gruntjs.com/) task runner.

### Pre-Requisites
- An understanding of AWS Cloud services, including APIGateway, Lambda and DynamoDB
- An AWS account
- Be aware of the AWS pricing structure - [AWS Pricing](https://aws.amazon.com/pricing/?nc2=h_ql_pr_ln&aws-products-pricing.sort-by=item.additionalFields.productNameLowercase&aws-products-pricing.sort-order=asc&awsf.Free%20Tier%20Type=*all&awsf.tech-category=*all)
- AWS SAM CLI installed on your device - [Installing the AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html)
- Unity installed on your device - This project was tested on Unity versions 2020.2.5f1 to 2022.2.5f1.

## The Design

This Unity demo uses the following services to support multiplayer gameplay:

**WebSocket API** - WebSockets is an API that allows a user/client to send messages to a server and receive event-driven responses.

**AWS ApiGateway** - Amazon's APIGateway service allows developers to create their own APIs. APIGateway can be configured to create an API that works together with the WebSocket protocol. The APIGateway in this project has three integration routes which trigger different Lambda functions to achieve a task.

**AWS DynamoDB** - DynamoDB is Amazon's cloud-hosted database service. In this project, it is used to store the online game session and player IDs.

**AWS Lambda** - Lambda is a compute service that allows function code to be uploaded to AWS and invoked without the developer needing to worry about computational resources and server systems.
There are three Lambdas in the project that support the multiplayer infrastructure in this project:
- JoinGame - This Lambda is responsible for establishing a connection to our APIGateway and connecting the two players in a session.
- GameMessaging - This Lambda is responsible for the passing of messages from the client to the server.
- DisconnectGame - This Lambda is responsible for closing the game session.

Below I will explain how the services work together to facilitate multiplayer gameplay.

### Creating the multiplayer session
The diagram below describes how the initial connection between Unity and AWS is established, and how the game session is created.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/mcuamrv12p6bgcbf47x2.png)

1. When a Player presses 'Play Online' in the Unity client, a Websocket instance will be created that connects to the APIGateway in AWS. When the connection is successful, APIGateway will invoke the JoinGame Lambda function.
2. The JoinGame Lambda will read the GameSession DynamoDB table. As the table is either empty or has no available sessions to join at this point, the Lambda will create a new session in the table and adds Player 1 to it with a connection ID. When Player 2 selects 'Play Online', the second invocation of the Lambda will find the game session created by Player 1, and will add Player 2 to it.
3. Now that both players have joined the session, a message is sent back from AWS to each player in Unity. This message triggers the StartGame() function in Unity, and the game begins.

### Client to client communication
The next diagram explains how messages are sent between each client/player via the APIGateway.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/zbiy6kawzeqy1ow6vb0j.png)

1. During gameplay, the player will select their choice. Selecting a choice will trigger the SendGameMessage() function, which will create a JSON message. This message contains the player's choice of rock, paper or scissors.
2. The JSON message is sent via Websockets to the APIGateway. The 'action' field in the JSON object is set to 'OnMessage' so that the APIGateways OnMessage integration is used to trigger the GameMessaging Lambda. The 'opcode' is used as a way to map different types of messages to different logic (There is a switch statement in the Lambda to check the opcodes. Whilst there are only two opcodes in the project demo this could expanded for more complex games).
3. Player 2 receives the message, triggering the ProcessReceivedMessage() function which will process the message and conduct the series of events required to continue the game. In this demo, when a message is received the opposing players choice is stored as a variable and only revealed when both players have made a choice. 


### Disconnecting the game
The final diagram displays how disconnect events are handled.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/1gvvyj4g4gmyksdtv90l.png)

1. When the game is over or if a player disconnects by quitting or closing the game application, the Websocket connection for that player will be terminated. This will trigger the disconnect route in our APIGateway in AWS which will invoke the DisconnectGame Lambda.
2. The DisconnectGame Lambda will check if the other player is still connected, and if so it will terminate their connection as well. If this happens during a game, a 'connection lost' message will be shown.
3. The DisconnectGame Lambda will delete the session from the GameSessionTable.

## AWS Deployment Steps
The AWS services used in this project are all defined within an AWS CloudFormation template. To make deployment simple, I've made use of the GruntJS task runner to create simple commands that will run the AWS CLI commands necessary for deployment. This section will cover the deployment instructions.

1 - Clone the Git repository for the demo:  https://github.com/Chrisd313/Rock-Paper_Scissors-Unity_AWS

2 - In the command terminal, navigate to the Rock-Paper_Scissors-Unity_AWS\Assets\Scripts directory and run the following command to download the Node.js dependencies:
   **_npm install_**

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/gbnxmryoe3gu9ldvcd6b.png)



3 - Find the .env file (Assets > Scripts > .env) and setup your environment variables in the .env file.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/otfeeei3ed4ouhtspdh2.png)


4 - In your terminal, from the Rock-Paper_Scissors-Unity_AWS\Assets\Scripts run the following grunt command to create an S3 bucket. This grunt task will run the AWS CLI command to create an S3 bucket in AWS.
   **_grunt createbucket_**

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/aafj9cxb2mfvjastxt2i.png)

5 - In your terminal, from the Rock-Paper_Scissors-Unity_AWS\Assets\Scripts run the following grunt command, which will zip the Lambda code, package the CloudFormation template and deploy our resources into a CloudFormation stack on AWS.
   **_grunt deploy_**

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/kg2z5f7iaxu82b8pimdu.png)

If the deployment was successful, you should be able to view your newly created CloudFormation stack and its resources within the AWS console.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/2fhktvg8qnzuadi4z9jf.png)


6 - From the CloudFormation page, you can open your API Gateway by clicking on it's physical ID, or by simply opening the API Gateway service page. In the API Gateway page, navigate to the Stages panel. Copy your WebSocket URL - we will be adding this into our Unity scripts to integrate with the API Gateway.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/x89df5o2vme1dc38cl2u.png)


## Unity Deployment Steps

1 - In Unity, you will be able to open the Rock-Paper-Scissors-Unity_AWS project that we cloned from Git earlier (as it is a new project, the setup may take a few minutes!).

The project may open with a blank scene - if so, simply navigate to the 'Scenes' folder in the Assets directory and select the 'Sample Scene' to open the correct scene for the game.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/t5lp5bnnyfdw9tbm3xdn.png)

The game can be played within the Unity editor. We can play against the CPU no problem, but if we click 'Play Online' we will get an error message - let's fix that and connect with our API Gateway.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/mr079wowjst7qv3k54sw.png)

2 - In the Assets panel, find and open the WebSocketService.cs script.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/6he4esao3bdcrlsesj3g.png)

Look for the webSocketDns variable on line 9 - this is currently empty. Paste in the WebSocket URL that we retrieved from the AWS API Gateway console earlier.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/wq7hr9w7bzju3ik6zt9e.png)

Returning to the Unity editor, if we press the Play button to start up the game and select 'Play Online' we now get an awaiting player message.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/n94hjwxjii9vh3e46tif.png)


3 - Next, we'll create a build for the game. In Unity, navigate to File > Build Settings. The Platform should be "Windows, Mac, Linux" by default. Click Build to create an executable file for the game.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/zbfc3d1bc0qyolm34jav.png)


4 - If the AWS services were deployed correctly, you should now be able to play a two-player game of rock, paper, scissors. You can test this by opening the game in two seperate windows and pressing Play Online.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/hdawk2fpfaicaaza85op.png)

## Conclusion
Although not production-ready solution for multiplayer, it is a quick, easy and low-cost way to implement multiplayer functionality for prototyping purposes. For more long-term solutions, AWS does offer its own game server hosting service - [AWS Gamelift](https://aws.amazon.com/gamelift/).

Thank you for taking a look at this project, and I hope you have found it useful!
