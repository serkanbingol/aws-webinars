using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;

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
            var s3Event = evnt.Records[0].S3;
            context.Logger.LogLine ($"Event Received by Lambda Function from {s3Event.Bucket.Name}");
            // var reader = new StreamReader (File.OpenRead (s3Event.Object.Key));

            // while (!reader.EndOfStream) {
            //     var line = reader.ReadLine ();
            //     context.Logger.LogLine($"{line}");
            // }

            return System.Net.HttpStatusCode.OK.ToString ();
        }
    }
}