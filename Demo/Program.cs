using System;
using System.Threading.Tasks;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Demo
{
    class Program
    {
        const string AzureTableStorageConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1";

        static async Task Main(string[] args)
        {
            var host =  new SiloHostBuilder()
                .UseLocalhostClustering()
                .AddAzureTableGrainStorageAsDefault(configure => {
                    configure.Configure(options => options.ConnectionString = AzureTableStorageConnectionString);
                })
                .Configure<ClusterOptions>(options => {
                    options.ClusterId = "dev";
                    options.ServiceId = "Orleans";
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .Build();
            await host.StartAsync();

            var grainFactory = host.Services.GetService<IGrainFactory>();
            var grain = grainFactory.GetGrain<IMyGrainWithState>("helloworld");
            await grain.SaveState("Hello");

            await host.StopAsync();

        }
    }

    public interface IMyGrainWithState : IGrainWithStringKey {
        Task SaveState(string name);
    }

    public class MyGrainWithState : Grain, IMyGrainWithState {

        readonly IPersistentState<State> state;
        readonly ILogger<MyGrainWithState> logger;

        public MyGrainWithState([PersistentState("state")] IPersistentState<State> state, ILogger<MyGrainWithState> logger) {
            this.state = state;
            this.logger = logger;
        }

        public async Task SaveState(string name)
        {
            state.State.Name = name;
            await state.WriteStateAsync();
            logger.LogWarning("State written to AzureTableStorage");
        }

        public class State {
            public string Name { get; set; }
        }

    }
}
