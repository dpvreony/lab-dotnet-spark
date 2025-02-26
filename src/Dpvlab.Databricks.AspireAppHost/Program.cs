// Copyright (c) 2022 DHGMS Solutions and Contributors. All rights reserved.
// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace Dpvlab.Databricks.AspireAppHost
{
    /// <summary>
    /// Program entry point for the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var app = GetApplication(args);
            await app.RunAsync();
        }

        /// <summary>
        /// Gets the builder for the distributed application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Instance of application builder.</returns>
        public static IDistributedApplicationBuilder GetBuilder(string[] args)
        {
            var builder = DistributedApplication.CreateBuilder(args);

            //builder.AddContainer("spark", "apache/spark").WithEntrypoint("/opt/spark/bin/spark-shell");

            //builder.AddContainer("spark", "jupyter/pyspark-notebook").WithHttpEndpoint(8888, 8888);

            var sqlServer = builder.AddSqlServer("sql").WithDataVolume();

            const string masterHostName = "spark-master";
            const string imageName = "bitnami/spark";
            const int masterPort = 7077;

            var sparkContainer = builder.AddContainer(
                    masterHostName,
                    imageName)
                .WithEnvironment("SPARK_MODE", "master")
                .WithEnvironment("SPARK_MASTER_HOST", "0.0.0.0")
                .WithEnvironment("SPARK_MASTER_PORT", "7077")
                .WithEndpoint(8080, 8080, "http", name: "web-ui")
                .WithEndpoint(7077, masterPort, name: "spark-master")
                .WithEndpoint(4040, 4040, name: "spark-app-ui");

            var sparkMasterUri = new UriBuilder
            {
                Scheme = "spark",
                Host = masterHostName,
                Port = masterPort
            };
            // $"spark://spark-master:7077"

            var sparkWorker = builder.AddContainer(
                    "spark-worker",
                    imageName)
                .WithEnvironment(
                    "SPARK_MODE",
                    "worker")
                .WithEnvironment(
                    "SPARK_MASTER_URL",
                    sparkMasterUri.Uri.GetLeftPart(UriPartial.Authority))
                .WithEndpoint(
                    8081,
                    8081,
                    "http")
                .WithParentRelationship(sparkContainer);

            AddJupyter(builder, sparkContainer, sparkWorker);

            // TODO: generate a password into jupyter store https://jupyter-server.readthedocs.io/en/latest/operators/public-server.html
            // TODO: add local python notebook sample to drop in mounted volume
            // TODO: add https://github.com/dotnet/spark sample app
            // TODO: add grafana sample for spark metrics

            return builder;
        }

        private static void AddJupyter(IDistributedApplicationBuilder builder, IResourceBuilder<ContainerResource> sparkContainer,
            IResourceBuilder<ContainerResource> sparkWorker)
        {
            var jupyter = builder.AddContainer(
                    "jupyter",
                    "jupyter/pyspark-notebook")
                .WithEnvironment(
                    "JUPYTER_TOKEN",
                    "mynotebook")
                .WithEndpoint(
                    8888,
                    8888,
                    "http")
                .WithBindMount(
                    "notebooks",
                    "/home/jovyan")
                .WithVolume(
                    "jupyter-data",
                    "/data")
                .WithVolume(
                    "jupyter-user",
                    "/home/root/.jupyter")
                .WaitFor(sparkContainer)
                .WaitFor(sparkWorker);
        }

        /// <summary>
        /// Gets the distributed application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Instance of the application.</returns>
        public static DistributedApplication GetApplication(string[] args)
        {
            var builder = GetBuilder(args);
            var app = builder.Build();
            return app;
        }
    }
}
