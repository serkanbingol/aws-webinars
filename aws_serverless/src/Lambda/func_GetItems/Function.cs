using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly : LambdaSerializer (typeof (Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace func_GetItems {
    public class Function {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> FunctionHandler (APIGatewayProxyRequest apigProxyEvent, ILambdaContext context) {
            var dynamoClient = new AmazonDynamoDBClient ();

            try {
                var request = new ScanRequest {

                    TableName = "serverless-inventory-app-dynamodb-table"

                };

                var response = dynamoClient.ScanAsync(request);
               
                return new APIGatewayProxyResponse {
                    Body = JsonSerializer.Serialize(response.Result.Items),
                        StatusCode = 200
                };
            } catch (Exception ex) {
                Console.WriteLine (ex.Message);
                return new APIGatewayProxyResponse {
                    Body = apigProxyEvent.Body,
                        StatusCode = 409
                };
            }

        }
    }
}