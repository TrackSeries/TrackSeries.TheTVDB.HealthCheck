using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Moq;
using TrackSeries.TheTVDB.Client;
using TrackSeries.TheTVDB.Client.Languages;
using TrackSeries.TheTVDB.Client.Search;
using TrackSeries.TheTVDB.Client.Series;
using TrackSeries.TheTVDB.Client.Updates;
using Xunit;

namespace TrackSeries.TheTVDB.HealthCheck.Tests
{
    public class TVDBHealthCheckTest
    {
        [Theory]
        [InlineData(HealthStatus.Degraded)]
        [InlineData(HealthStatus.Unhealthy)]
        public async Task CheckSeriesEnabledFaultedService(HealthStatus healthStatus)
        {
            // Arrange
            var services = GetServices(GetFaultedClient(), options =>
            {
                options.CheckSeries = true;
            },
            healthStatus);

            var registration = Assert.Single(services.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations);
            var check = Assert.IsType<TVDBHealthCheck>(registration.Factory(services));

            // Act
            var result = await check.CheckHealthAsync(new HealthCheckContext() { Registration = registration });

            // Assert
            result.Status.Should().Be(healthStatus);
        }

        [Theory]
        [InlineData(HealthStatus.Degraded)]
        [InlineData(HealthStatus.Unhealthy)]
        public async Task CheckLanguagesEnabledFaultedService(HealthStatus healthStatus)
        {
            // Arrange
            var services = GetServices(GetFaultedClient(), options =>
            {
                options.CheckLanguages = true;
            },
            healthStatus);

            var registration = Assert.Single(services.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations);
            var check = Assert.IsType<TVDBHealthCheck>(registration.Factory(services));

            // Act
            var result = await check.CheckHealthAsync(new HealthCheckContext() { Registration = registration });

            // Assert
            result.Status.Should().Be(healthStatus);
        }

        [Theory]
        [InlineData(HealthStatus.Degraded)]
        [InlineData(HealthStatus.Unhealthy)]
        public async Task CheckSearchEnabledFaultedService(HealthStatus healthStatus)
        {
            // Arrange
            var services = GetServices(GetFaultedClient(), options =>
            {
                options.CheckSearch = true;
            },
            healthStatus);

            var registration = Assert.Single(services.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations);
            var check = Assert.IsType<TVDBHealthCheck>(registration.Factory(services));

            // Act
            var result = await check.CheckHealthAsync(new HealthCheckContext() { Registration = registration });

            // Assert
            result.Status.Should().Be(healthStatus);
        }

        [Theory]
        [InlineData(HealthStatus.Degraded)]
        [InlineData(HealthStatus.Unhealthy)]
        public async Task CheckUpdatesEnabledFaultedService(HealthStatus healthStatus)
        {
            // Arrange
            var services = GetServices(GetFaultedClient(), options =>
            {
                options.CheckUpdates = true;
            },
            healthStatus);

            var registration = Assert.Single(services.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value.Registrations);
            var check = Assert.IsType<TVDBHealthCheck>(registration.Factory(services));

            // Act
            var result = await check.CheckHealthAsync(new HealthCheckContext() { Registration = registration });

            // Assert
            result.Status.Should().Be(healthStatus);
        }

        private static IServiceProvider GetServices(ITVDBClient client, Action<TVDBHealthCheckOptions> setup = null, HealthStatus? failureStatus = default)
        {
            return new ServiceCollection()
            .AddSingleton(client)
            .AddHealthChecks()
            .AddTVDB(setup, failureStatus: failureStatus)
            .Services.BuildServiceProvider();
        }

        private static ITVDBClient GetFaultedClient()
        {
            var clientMock = new Mock<ITVDBClient>();
            var searchMock = new Mock<ISearchClient>();
            var seriesMock = new Mock<ISeriesClient>();
            var updatesMock = new Mock<IUpdatesClient>();
            var languagesMock = new Mock<ILanguagesClient>();

            searchMock.Setup(client => client.SearchSeriesByNameAsync(It.IsAny<string>(), default)).ThrowsAsync(new HttpRequestException());
            seriesMock.Setup(client => client.GetAsync(It.IsAny<int>(), default)).ThrowsAsync(new HttpRequestException());
            updatesMock.Setup(client => client.GetAsync(It.IsAny<DateTime>(), default)).ThrowsAsync(new HttpRequestException());
            languagesMock.Setup(client => client.GetAllAsync(default)).ThrowsAsync(new HttpRequestException());

            clientMock.Setup(client => client.Search).Returns(searchMock.Object);
            clientMock.Setup(client => client.Series).Returns(seriesMock.Object);
            clientMock.Setup(client => client.Updates).Returns(updatesMock.Object);
            clientMock.Setup(client => client.Languages).Returns(languagesMock.Object);

            return clientMock.Object;
        }
    }
}
