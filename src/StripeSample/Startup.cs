using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Stripe;
using StripeSample.Infrastructure;
using StripeSample.Infrastructure.Configuration;
using StripeSample.Infrastructure.Data;
using System;

namespace StripeSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<StripeSettings>(Configuration.GetSection("StripeSettings"));
            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));
            services.AddScoped<UserContext>();

            var connection = Configuration.GetConnectionString("SqlConnection");

            services.AddSingleton(Configuration)
                .AddApplicationInsightsTelemetry(Configuration)
                .AddStripe()
                .AddCustomHangfire(connection)
                .AddCustomDatabase(connection)
                .AddMediatR(typeof(Startup));

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider services)
        {
            var settings = services.GetService<IOptions<StripeSettings>>();

            StripeConfiguration.ApiKey = settings.Value.PrivateKey;

            services.MigrateDbContext<SubscriptionsContext>((context, __) =>
            {
                new SubscriptionsContextSeed()
                    .SeedAsync(context, settings.Value)
                    .Wait();
            });

            app.UseHangfireDashboard();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }

    static class CustomStartupExtensionMethods
    {
        public static IServiceCollection AddCustomHangfire(this IServiceCollection services, string connectionString)
        {
            services.AddHangfire(c => c.UseSqlServerStorage(connectionString));
            services.AddHangfireServer();

            return services;
        }

        public static IServiceCollection AddCustomDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<SubscriptionsContext>(o =>
            {
                o.UseSqlServer(connectionString, x =>
                {
                    x.MigrationsHistoryTable(SubscriptionsContext.MIGRATIONS_TABLE, SubscriptionsContext.DEFAULT_SCHEMA);
                    x.EnableRetryOnFailure();
                });
            });



            return services;
        }
    }
}
