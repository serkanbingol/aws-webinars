using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly : LambdaSerializer (typeof (Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace func_SNS {
    public class Function {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler (DynamoDBEvent dynamoEvent, ILambdaContext context) {

            var client = new AmazonSimpleNotificationServiceClient ();

            context.Logger.LogLine ($"Event Received by Lambda Function from {dynamoEvent}");
            context.Logger.LogLine ($"Beginning to process {dynamoEvent.Records.Count} records...");

            foreach (var record in dynamoEvent.Records) {

                var dbRecord = record.Dynamodb.NewImage;
                if (dbRecord != null) {
                    var itemStore = dbRecord["Store"].S;
                    var itemName = dbRecord["Item"].S;
                    var itemStock = Convert.ToInt32 (dbRecord["Count"].N);
                    var message = itemStore + " is out of stock of " + itemName;

                    context.Logger.LogLine ($"Records {itemStore} - {itemName} - {itemStock.ToString()}");

                    if (itemStock == 0) {
                        var allTopics = await client.ListTopicsAsync ();
                        var NoStockTopic = allTopics.Topics.FirstOrDefault (x => x.TopicArn.Contains ("NoStock")).TopicArn;
                        var request = new PublishRequest {
                            TopicArn = NoStockTopic,
                            Message = message,
                            Subject = "Inventory Alert!"
                        };

                        client.PublishAsync (request).Wait ();

                    }
                }

            }
            return "OK";
        }

    }
}