const AdmZip = require("adm-zip");
const fs = require("fs");

const zipDirectoryPath = "./AWS/deployment/zip_files";

// This function zips the Lambda code so it can be uploaded to an S3 bucket for deployment.
function createZipFile(inputFilePath, outputZipFilePath) {
  if (!fs.existsSync(zipDirectoryPath)) {
    fs.mkdirSync(zipDirectoryPath);

    console.log(`Directory '${zipDirectoryPath}' created.`);
  }

  const zip = new AdmZip();
  zip.addLocalFile(inputFilePath);
  zip.writeZip(outputZipFilePath);
  console.log(`Zip file "${outputZipFilePath}" created successfully.`);
}

createZipFile(
  "./AWS/lambda/join_game.mjs",
  "./AWS/deployment/zip_files/join_game.zip"
);
createZipFile(
  "./AWS/lambda/disconnect_game.mjs",
  "./AWS/deployment/zip_files/disconnect_game.zip"
);
createZipFile(
  "./AWS/lambda/game_messaging.mjs",
  "./AWS/deployment/zip_files/game_messaging.zip"
);
