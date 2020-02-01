using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace StripeSample.Infrastructure
{
    public static class IServiceProviderExtensions
    {

        public static IServiceProvider MigrateDbContext<TContext>(this IServiceProvider services, Action<TContext, IServiceProvider> seeder) where TContext : DbContext
        {
            var logger = services.GetRequiredService<ILogger<TContext>>();
            var context = services.GetService<TContext>();

            try
            {
                logger.LogInformation("Migrating database associated with context {DbContextName}", typeof(TContext).Name);

                // TODO: Wrap in retry policy handler
                InvokeSeeder(seeder, context, services);

                logger.LogInformation("Migrated database associated with context {DbContextName}", typeof(TContext).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating the database used on context {DbContextName}", typeof(TContext).Name);
            }

            return services;
        }

        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context, IServiceProvider services)
            where TContext : DbContext
        {
            context.Database.Migrate();
            seeder(context, services);
        }
    }
}
