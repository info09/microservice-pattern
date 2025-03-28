﻿namespace Pattern.AppHost.Extensions;

public static class ResourceExtensions
{
    public static IResourceBuilder<PostgresDatabaseResource> AddDefaultDatabase<TProject>(this IResourceBuilder<PostgresServerResource> builder)
    {
        return builder.AddDatabase($"{typeof(TProject).Name.Replace('_', '-')}-Db");
    }

    public static IResourceBuilder<ProjectResource> AddProjectWithPostfix<TProject>(this IDistributedApplicationBuilder builder, string postfix = "") where TProject : IProjectMetadata, new()
    {
        if (string.IsNullOrEmpty(postfix))
        {
            return builder.AddProject<TProject>(typeof(TProject).Name.Replace('_', '-'));
        }
        else
        {
            return builder.AddProject<TProject>(typeof(TProject).Name.Replace('_', '-') + "-" + postfix);
        }
    }
}
