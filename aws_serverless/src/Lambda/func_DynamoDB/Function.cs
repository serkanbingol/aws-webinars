using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
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
            var client = new AmazonS3Client (bucketRegion);
            var s3Event = evnt.Records[0].S3;

            context.Logger.LogLine ($"Event Received by Lambda Function from {s3Event.Bucket.Name}");
            context.Logger.LogLine ($"Event Received by Lambda Function from {s3Event.Object.Key}");

            GetObjectRequest request = new GetObjectRequest {
                BucketName = s3Event.Bucket.Name,
                Key = s3Event.Object.Key,

            };
            GetObjectResponse response = await client.GetObjectAsync (request);

            response.WriteResponseStreamToFileAsync ("/tmp/inventory.txt", true, new System.Threading.CancellationToken { }).Wait ();
            try {
                using (TextFieldParser csvReader = new TextFieldParser ("/tmp/inventory.txt")) {
                    csvReader.SetDelimiters (new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;

                    string[] colFields;    
                    colFields = csvReader.ReadFields ();
                    foreach (string column in colFields) {
                        context.Logger.LogLine (column);
                    }
                    while (!csvReader.EndOfData) {
                        colFields = csvReader.ReadFields ();
                        foreach (string column in colFields) {
                            context.Logger.LogLine (column);
                        }

                    }

                }
            } catch (Exception ex) {
                Console.WriteLine (ex.Message);
                return "Error:Parsing CSV";
            }

            return "TEST COMPLETED";

            #region Stream s3 Object
            // string responseBody = "";
            // try {
            //     GetObjectRequest request = new GetObjectRequest {
            //         BucketName = s3Event.Bucket.Name,
            //         Key = s3Event.Object.Key,

            //     };
            //     using (GetObjectResponse response = await client.GetObjectAsync (request))
            //     using (Stream responseStream = response.ResponseStream)
            //     using (StreamReader reader = new StreamReader (responseStream)) {
            //         // string title = response.Metadata["x-amz-meta-title"]; // Assume you have "title" as medata added to the object.
            //         string contentType = response.Headers["Content-Type"];
            //         // context.Logger.LogLine ($"Object metadata, Title: {title}");
            //         context.Logger.LogLine ($"Content type: {contentType}");

            //         responseBody = reader.ReadToEnd (); // Now you process the response body.
            //         context.Logger.LogLine (responseBody);
            //         context.Logger.LogLine ("----------------------");

            //         var array = responseBody.ToString ().Split (',');
            //         foreach (string column in array) {
            //             context.Logger.LogLine (column);
            //         }
            //         context.Logger.LogLine ("end");
            //         return "OK";

            //     }

            // } catch (AmazonS3Exception e) {
            //     context.Logger.LogLine ($"Error encountered ***. Message:'{e.Message}' when writing an object");
            //     return "ERROR";
            // } catch (Exception e) {
            //     context.Logger.LogLine ($"Unknown encountered on server. Message:'{e.Message}' when writing an object");
            //     return "ERROR";
            // }
            #endregion

        }

    }
}