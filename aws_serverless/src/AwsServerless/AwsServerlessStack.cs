using System.Threading;
using System.Diagnostics.Tracing;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Lambda.EventSources;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;

namespace AwsServerless {
    public class AwsServerlessStack : Stack {
        internal AwsServerlessStack (Construct scope, string id, IStackProps props = null) : base (scope, id, props) {
            // Create A S3 Bucket
            var s3Bucket = new Bucket (this, "demoS3", new BucketProps {
                BucketName = "serverless-inventory-app-s3-bucket",
                    RemovalPolicy = RemovalPolicy.DESTROY,
                    PublicReadAccess=true,
            });
         
            // Create A SNS
            var snsTopic = new Topic (this, "demoSNS", new TopicProps {
                DisplayName = "Out of Stock Topic",

            });
            snsTopic.AddSubscription (new EmailSubscription ("serkanbingol@outlook.com", new EmailSubscriptionProps {

            }));

            // Create A DynamoDB
            var dynamoDBTable = new Table (this, "demoDynamoDB", new TableProps {
                TableName = "serverless-inventory-app-dynamodb-table",
                    PartitionKey = new Attribute { Name = "id", Type = AttributeType.STRING },
                    BillingMode = BillingMode.PAY_PER_REQUEST,
                    Stream = StreamViewType.NEW_AND_OLD_IMAGES,
                    RemovalPolicy = RemovalPolicy.DESTROY

            });

            // Create Lambda Function for DynamoDB
            var func_DynamoDB = new Function (this, "demoFuncDynamoDB", new FunctionProps {
                Runtime = Runtime.DOTNET_CORE_3_1,
                    FunctionName = "serverless-inventory-app-func-dynamodb",
                    Code = Code.FromAsset ("publish_lambda/func_DynamoDB"),
                    Handler = "func_DynamoDB::func_DynamoDB.Function::FunctionHandler",
                    Timeout = Amazon.CDK.Duration.Minutes(3)

            });
            // Create Lambda Function for SNS
            var func_SNS = new Function (this, "demoFuncSNS", new FunctionProps {
                Runtime = Runtime.DOTNET_CORE_3_1,
                    FunctionName = "serverless-inventory-app-func-sns",
                    Code = Code.FromAsset ("publish_lambda/func_SNS"),
                    Handler = "func_SNS::func_SNS.Function::FunctionHandler"

            });

            // Create Lambda Function for RestAPI
            var func_GetItems = new Function (this, "demoFuncGetItems", new FunctionProps {
                Runtime = Runtime.DOTNET_CORE_3_1,
                    FunctionName = "serverless-inventory-app-func-getitems",
                    Code = Code.FromAsset ("publish_lambda/func_GetItems"),
                    Handler = "func_GetItems::func_GetItems.Function::FunctionHandler"

            });

            // Create API Gateway for Applications
            var apiGateway = new LambdaRestApi (this, "demoAPIGateway", new LambdaRestApiProps {
                Handler = func_GetItems,
                    RestApiName = "serverless-inventory-app-api-inventory",
                    Proxy = false,

            });
            var items = apiGateway.Root.AddResource ("api");
            items.AddMethod ("GET");

            // Create Lambda Event for Upload Image S3
            func_DynamoDB.AddEventSource (new S3EventSource (s3Bucket, new S3EventSourceProps {
                Events = new [] { EventType.OBJECT_CREATED }
            }));

            // Create Lambda Event for Insert DynamoDB
            func_SNS.AddEventSource (new DynamoEventSource (dynamoDBTable, new DynamoEventSourceProps {
                StartingPosition = StartingPosition.TRIM_HORIZON
            }));

        }
    }
}