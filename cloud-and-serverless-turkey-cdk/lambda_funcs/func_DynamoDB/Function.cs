using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using CsvHelper;
using func_DynamoDB.model;
using Microsoft.VisualBasic.FileIO;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly : LambdaSerializer (typeof (Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace func_DynamoDB {
    public class Function {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler (S3Event evnt, ILambdaContext context) {
            RegionEndpoint bucketRegion = RegionEndpoint.EUWest1;
            var s3Client = new AmazonS3Client (bucketRegion);
            var dynamoClient = new AmazonDynamoDBClient ();
            var s3Event = evnt.Records[0].S3;

            context.Logger.LogLine ($"Event Received by Lambda Function from {s3Event.Bucket.Name}");
            context.Logger.LogLine ($"Event Received by Lambda Function from {s3Event.Object.Key}");

            GetObjectRequest request = new GetObjectRequest {
                BucketName = s3Event.Bucket.Name,
                Key = s3Event.Object.Key,

            };
            GetObjectResponse response = await s3Client.GetObjectAsync (request);

            response.WriteResponseStreamToFileAsync ("/tmp/" + s3Event.Object.Key + ".txt", true, new System.Threading.CancellationToken { }).Wait ();

            #region CsvHelper Region
            try {
                Table inventoryCatalog = Table.LoadTable (dynamoClient, "serverless-inventory-app-dynamodb-table");
                List<StockDetails> result;

                using (var fileReader = new StreamReader ("/tmp/" + s3Event.Object.Key + ".txt")) {
                    var csv = new CsvReader (fileReader, CultureInfo.InvariantCulture);
                    csv.Configuration.HasHeaderRecord = false;
                    csv.Read ();
                    result = csv.GetRecords<StockDetails> ().ToList ();

                    foreach (var stock in result) {
                        var newStock = new Document ();
                        newStock["Store"] = stock.store;
                        newStock["Item"] = stock.item;
                        newStock["Count"] = stock.count;
                        await inventoryCatalog.PutItemAsync (newStock);
                    }

                }
                File.Delete ("/tmp/" + s3Event.Object.Key + ".txt");
                return "Sucess:Parsing CSV and Insert DynamoDB Table";
            } catch (Exception ex) {
                Console.WriteLine (ex.Message);
                return "Error:Parsing CSV and Insert DynamoDB";
            }
            #endregion
        }

    }
}