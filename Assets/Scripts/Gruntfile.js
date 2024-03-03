require(`dotenv`).config();
const environmentName = process.env.ENVIRONMENT;
const gameName = process.env.GAME_NAME;
const bucketName = process.env.S3_BUCKET_NAME;
const region = process.env.AWS_REGION;

module.exports = function (grunt) {
  grunt.loadNpmTasks("grunt-exec");

  // Task configuration
  grunt.initConfig({
    pkg: grunt.file.readJSON("package.json"),
    exec: {
      create_s3_bucket: {
        cmd: `aws s3 mb s3://${bucketName} --region ${region}`,
      },
      package_cloudformation: {
        command: [
          "node createZip.js",
          `aws cloudformation package --template-file ./AWS/deployment/template.yml --s3-bucket ${bucketName} --s3-prefix rps-deploy --output-template-file ./AWS/deployment/packaged-template.yml`,
          `aws cloudformation deploy --template-file ./AWS/deployment/packaged-template.yml --stack-name ${bucketName} --region ${region} --capabilities CAPABILITY_IAM --parameter-overrides Environment=${environmentName} GameName=${gameName}`,
        ].join("&&"),
      },
    },
  });

  // GRUNT TASK COMMANDS //

  // Command: grunt createbucket
  // Desc: Creates an S3 bucket.
  grunt.registerTask("createbucket", ["exec:create_s3_bucket"]);

  // Command: grunt deploy
  // Desc: Packages the CloudFormation template and deploys the Cloudformation stack resources to AWS.
  grunt.registerTask("deploy", ["exec:package_cloudformation"]);
};
