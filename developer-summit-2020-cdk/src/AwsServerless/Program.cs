using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AwsServerless
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new AwsServerlessStack(app, "AwsServerlessStack");
            app.Synth();
        }
    }
}
