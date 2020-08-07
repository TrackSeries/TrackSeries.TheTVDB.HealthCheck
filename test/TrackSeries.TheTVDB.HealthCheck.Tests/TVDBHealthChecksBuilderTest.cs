using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
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
            var services = GetServices();

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
            var services = GetServices();

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
            var services = GetServices();

            // Act
            Action action = () => services.AddHealthChecks().AddTVDB();

            // Assert
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("TVDBClient must be configured before calling AddTVDB or using TVDBHealthCheckOptions.ConfigureClient.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(int.MinValue)]
        public void AddCheckShouldThrowWhenCheckSeriesIsEnabledWithInvalidSerieId(int serieId)
        {
            // Arrange
            var services = GetServices();

            // Act
            Action action = () => services.AddHealthChecks()
            .AddTVDB(options =>
            {
                options.CheckSeries = true;
                options.SerieId = serieId;
                options.ConfigureClient(client =>
                {
                    client.ApiKey = APIKEY;
                });
            });

            // Assert
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("SerieId must be greater than 0 when CheckSeries is enabled.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void AddCheckShouldThrowWhenCheckSearchIsEnabledWithInvalidSearchTerm(string searchTerm)
        {
            // Arrange
            var services = GetServices();

            // Act
            Action action = () => services.AddHealthChecks()
            .AddTVDB(options =>
            {
                options.CheckSearch = true;
                options.SearchTerm = searchTerm;
                options.ConfigureClient(client =>
                {
                    client.ApiKey = APIKEY;
                });
            });

            // Assert
            action.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("SearchTerm must not be null or empty when CheckSearch is enabled.");
        }

        private IServiceCollection GetServices()
        {
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder().Build();
            services.AddSingleton<IConfiguration>(configuration);
            return services;
        }
    }
}
