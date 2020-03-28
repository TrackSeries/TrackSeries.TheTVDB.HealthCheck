using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TrackSeries.TheTVDB.Client;

namespace TrackSeries.TheTVDB.HealthCheck
{
    public class TVDBHealthCheck : IHealthCheck
    {
        private readonly ITVDBClient _client;
        private readonly TVDBHealthCheckOptions _options;

        public TVDBHealthCheck(ITVDBClient client, TVDBHealthCheckOptions options)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_options.CheckSeries)
                {
                    _ = await _client.Series.GetAsync(_options.SerieId, cancellationToken);
                }

                if (_options.CheckSearch)
                {
                    _ = await _client.Search.SearchSeriesByNameAsync(_options.SearchTerm);
                }

                if(_options.CheckUpdates)
                {
                    _ = await _client.Updates.GetAsync(DateTime.Now.AddDays(-1), cancellationToken);
                }

                if (_options.CheckLanguages)
                {
                    _ = await _client.Languages.GetAllAsync(cancellationToken);
                }
            }
            catch(Exception exception)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: exception);
            }

            return HealthCheckResult.Healthy();
        }


    }
}
