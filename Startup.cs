using Bot.Builder.Community.Adapters.Slack;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Azure.Storage.Queues;

using Ada.State;
using Ada.Dialogs;
using Ada.Services;
using SlackAPI;

namespace Ada
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
            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options 
                => options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth);

            services.AddSingleton(sp => new QueueClient(Configuration.GetSection("StorageAccount").GetValue<string>("ConnectionString"), "webhook-queue"));

            // Create the slack adapter.
            services.AddSingleton<SlackAdapter, SlackAdapterWithErrorHandler>();

            services.AddSingleton(sp =>
            {
                var token = Configuration.GetValue<string>("SlackBotToken");
                return new SlackTaskClient(token);
            });

            services.AddSingleton(sp =>
            {
                var key = Configuration.GetValue<string>("ApiNinjaKey");
                return new FactClient(key);
            });

            services
                .AddApplicationInsightsTelemetry()
                .AddSingleton<IBotTelemetryClient, BotTelemetryClient>()
                .AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>()
                .AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>()
                .AddSingleton<TelemetryInitializerMiddleware>()
                .AddSingleton<TelemetryLoggerMiddleware>();

            services.AddSingleton<LanguageService>();

            // // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            var storage = new CosmosDbPartitionedStorage(
                new CosmosDbPartitionedStorageOptions()
                {
                    CosmosDbEndpoint = Configuration.GetSection("CosmosDb").GetValue<string>("CosmosDbEndpoint"),
                    AuthKey = Configuration.GetSection("CosmosDb").GetValue<string>("AuthKey"), 
                    DatabaseId = Configuration.GetSection("CosmosDb").GetValue<string>("DatabaseId"), 
                    ContainerId = Configuration.GetSection("CosmosDb").GetValue<string>("ContainerId"), 
                });

            services
                .AddSingleton(new UserState(storage))
                .AddSingleton(new ConversationState(storage))
                .AddSingleton(new ConfigurationState(storage, "__CONF"));

            // Create state accessors
            services.AddSingleton(sp =>
            {
                var conversationState = sp.GetRequiredService<ConversationState>();
                var userState = sp.GetRequiredService<UserState>();
                var configurationState = sp.GetRequiredService<ConfigurationState>();

                return new Accessors(conversationState, userState, configurationState)
                {
                    DialogStateAccessor = conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                    UserScopedStateAccessor = userState.CreateProperty<UserScopedState>(nameof(UserScopedState)),
                    BotScopedStateAccessor = configurationState.CreateProperty<BotScopedState>(nameof(BotScopedState)),
                };
            });

            // Add all dialogs that live in the same namespace as MainDialog.
            services
                .AddSingleton<MainDialog>()
                .AddSingleton<FactDialog>()
                .AddSingleton<ConfigurationDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, Ada.Bot<MainDialog>>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}
