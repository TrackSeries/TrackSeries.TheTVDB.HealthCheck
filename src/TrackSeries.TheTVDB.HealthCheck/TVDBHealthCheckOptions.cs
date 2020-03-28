using System;
using TrackSeries.TheTVDB.Client;

namespace TrackSeries.TheTVDB.HealthCheck
{
    public class TVDBHealthCheckOptions
    {
        const int GameOfThronesId = 121361;

        public bool CheckSearch { get; set; } = false;
        public string SearchTerm { get; set; } = "game of thrones";
        public bool CheckSeries { get; set; } = true;
        public int SerieId { get; set; } = GameOfThronesId;
        public bool CheckUpdates { get; set; } = false;
        public bool CheckLanguages { get; set; } = false;
        internal Action<TVDBClientOptions> ConfigueClientSetup { get; private set; } = null;
        public void ConfigureClient(Action<TVDBClientOptions> setup)
        {
            ConfigueClientSetup = setup;
        }
    }
}
