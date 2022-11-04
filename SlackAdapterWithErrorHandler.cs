using Bot.Builder.Community.Adapters.Slack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ada
{
    public class SlackAdapterWithErrorHandler : SlackAdapter
    {
        public SlackAdapterWithErrorHandler(IConfiguration configuration, IWebHostEnvironment env, TelemetryInitializerMiddleware telemetryInitializerMiddleware, ILogger<SlackAdapter> logger)
            : base(configuration, null, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {

                Use(telemetryInitializerMiddleware);

                // Log any leaked exception from the application.
                logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");


                if (env.IsDevelopment())
                {
                    // While in development, send exception message to the user.
                    await turnContext.SendActivityAsync(exception.Message);
                }
                else
                {
                    // Send a message to the user
                    await turnContext.SendActivityAsync("The bot encountered an error or bug.");
                    await turnContext.SendActivityAsync("To continue to run this bot, please fix the bot source code.");
                }

                // Send a trace activity, which will be displayed in the Bot Framework Emulator
                await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
            };
        }
    }
}
