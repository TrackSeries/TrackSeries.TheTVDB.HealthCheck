using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace TrackSeries.TheTVDB.HealthCheck.Tests
{
    public class TVDBHealthChecksBuilderTest
    {
        const string APIKEY = nameof(APIKEY);

        [Fact]
        public void AddCheckShouldRegisterWhenTVDBClientIsPreviouslyConfigured()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddTVDBClient(options =>
            {
                options.ApiKey = APIKEY;
            });

            services.AddHealthChecks()
                .AddTVDB();

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;

            // Assert
            var registration = Assert.Single(options.Registrations);
            Assert.IsType<TVDBHealthCheck>(registration.Factory(serviceProvider));
        }

        [Fact]
        public void AddCheckShouldRegisterWhenTVDBClientIsConfiguredOnAddTVDB()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddHealthChecks()
                .AddTVDB(options =>
                {
                    options.ConfigureClient(client =>
                    {
                        client.ApiKey = APIKEY;
                    });
                });

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;

            // Assert
            var registration = Assert.Single(options.Registrations);
            Assert.IsType<TVDBHealthCheck>(registration.Factory(serviceProvider));
        }

        [Fact]
        public void AddCheckShouldThrowWhenTVDBClientIsNotConfigured()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act && Assert
            var exception = Assert.Throws<InvalidOperationException>(() => services.AddHealthChecks().AddTVDB());
            Assert.Equal("TVDBClient must be configured before calling AddTVDB or using TVDBHealthCheckOptions.ConfigureClient.", exception.Message);
        }
    }
}
