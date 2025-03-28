var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CQRS_Library_BookService>("cqrs-library-bookservice");

builder.Build().Run();
