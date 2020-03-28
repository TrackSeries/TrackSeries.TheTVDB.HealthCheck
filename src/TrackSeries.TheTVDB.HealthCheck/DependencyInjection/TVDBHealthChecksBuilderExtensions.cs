using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TrackSeries.TheTVDB.Client;
using TrackSeries.TheTVDB.HealthCheck;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TVDBHealthChecksBuilderExtensions
    {
        internal const string TVDB_NAME = "TVDB API V3";

        public static IHealthChecksBuilder AddTVDB(this IHealthChecksBuilder builder, Action<TVDBHealthCheckOptions> setup = null,
            string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default)
        {
            var options = new TVDBHealthCheckOptions();
            setup?.Invoke(options);

            if(!builder.Services.Any(s => s.ServiceType == typeof(ITVDBClient)))
            {
                var clientSetup = options.ConfigueClientSetup ?? throw new InvalidOperationException("TVDBClient must be configured before calling AddTVDB or using TVDBHealthCheckOptions.ConfigureClient.");
                builder.Services.AddTVDBClient(clientSetup);
            }

            if(options.CheckSeries && options.SerieId < 1)
            {
                throw new InvalidOperationException("SerieId must be greater than 0 when CheckSeries is enabled.");
            }

            if(options.CheckSearch && string.IsNullOrEmpty(options.SearchTerm))
            {
                throw new InvalidOperationException("SearchTerm must not be null or empty when CheckSearch is enabled.");
            }

            return builder.Add(new HealthCheckRegistration(
                name ?? TVDB_NAME,
                sp => new TVDBHealthCheck(sp.GetRequiredService<ITVDBClient>(), options),
                failureStatus,
                tags,
                timeout));
        }
    }
}
