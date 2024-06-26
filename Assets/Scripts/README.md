# RockPaperScissors-Unity-AWS

This is a project demo of a multiplayer rock, paper, scissors game created with Unity and AWS.

## Pre-Requisites

If you wish to use this project demo, please see the below requisites:

- You should have a basic understanding of AWS Cloud services, including APIGateway, Lambda and DynamoDB
- You will need AWS account
- Be aware of the AWS pricing structure
- AWS SAM CLI installed on your device
- Unity installed on your device - This project will work on Unity versions 2020.2.5f1 - 2022.2.5f1.

## Deployment Steps

The AWS services used in this project are all defined within an AWS CloudFormation template (AWS>Deployment>template.yml). To make deployment simple, I've made use of the GruntJS task runner to create simple commands that will run the AWS CLI commands necessary for deployment. This section will cover the deployment instructions.

1. In your terminal, run the following command to download the Node.js dependencies:
   npm install

2. Setup your environment variables in the .env file.

3. In your terminal, run the following grunt command to create an S3 bucket. This grunt task will run the AWS CLI command to create an S3 bucket in AWS.
   grunt createbucket

4. In your terminal, run the following grunt command, which will zip the Lambda code, package the CloudFormation template and deploy our resources into a CloudFormation stack on AWS.
   grunt deploy

5. In the AWS console, go to your API Gateway resources and navigate to the Stages page. Copy your WebSocket URL.

6. Back in your Unity scripts, go to the WebSocketService.cs file and update the webSocketDns variable with the WebSocket URL you copied from Unity.

7. In Unity, navigate to Edit > Build Settings. The Platform should be "Windows, Mac, Linux" by default. Click Build to create an executable file for the game.

8. If the AWS services were deployed correctly, you should now be able to play a two-player game of rock, paper, scissors. You can test this by opening the game in two seperate windows and pressing Play Online.
