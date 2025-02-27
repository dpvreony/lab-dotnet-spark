## Proof of Concept for Local Testable Spark Environment

This repository demonstrates how to set up a local environment to test against Spark using Aspire. The benefits of having such an ability include:

- **Rapid Development**: Quickly iterate and test Spark applications locally without needing a full cluster setup.
- **Cost Efficiency**: Save costs by testing locally rather than on expensive cloud resources.
- **Debugging**: Easier to debug and troubleshoot issues in a controlled local environment.

## Testable Spark Environment using Aspire

The `Program.cs` file sets up a testable Spark environment using Aspire. Below is an overview of the process:

- **Application Setup**: The application is initiated using `DistributedApplication.CreateBuilder(args)` which sets up the environment.
- **SQL Server Integration**: A SQL Server container is added with `builder.AddSqlServer("sql").WithDataVolume()`.
- **Spark Master Node**:
  - A Spark master container is configured with various environment variables to run in master mode.
  - HTTP and Spark master endpoints are set up for communication.
- **Spark Worker Node**:
  - A Spark worker container is configured and linked to the master node.
  - Environment variables are set to connect to the Spark master.
- **Jupyter Notebook Integration**:
  - A Jupyter container is added to the environment, configured with a token for security.
  - Various volumes and endpoints are set up for the Jupyter container.
  - The Jupyter container is set to wait for the Spark master and worker containers.

This setup ensures that a testable Spark environment is created, integrating SQL Server and Jupyter Notebook for a comprehensive testing setup.
