using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using Ada.State;
using Microsoft.Extensions.Configuration;
using Ada.Services;
using Microsoft.Bot.Builder;
using Ada.Helpers;

namespace Ada.Dialogs
{
    public class MainDialog : DialogBase<MainDialog>
    {
        public MainDialog(Accessors accessors, IConfiguration configuration, ILogger<MainDialog> logger, LanguageService languageService, IBotTelemetryClient telemetryClient, ConfigurationDialog configurationDialog, FactDialog factDialog) 
            : base(nameof(MainDialog), accessors, configuration, languageService, logger)
        {
            this.TelemetryClient = telemetryClient;
            
            AddDialog(configurationDialog);
            AddDialog(factDialog);

            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] { DecideUserIntent }));

            InitialDialogId = nameof(WaterfallDialog);
        }
        protected override bool EnabledByDefault => true;
        private async Task<DialogTurnResult> DecideUserIntent(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = await LanguageService.RecognizeIntentAsync(stepContext.Context, cancellationToken);
            var intent = (string) result.Properties["topIntent"];
            var score = (decimal)result.Intents[intent].Score;
            var threshold = Configuration.GetSection(nameof(MainDialog)).GetValue<decimal>("AcceptableIntentThreshold");

            if(score < threshold)
            {
                var activity = BlockFactory.CreateNew
                    .AddMarkdownBlock($"I don't fully understand. Can you try asking something else?")
                    .AddContextBlock($":question: The top intent *'{intent}'* scored *{Math.Round(score * 100)}%*. Minimum score of *{Math.Round(threshold * 100)}%* required.")
                    .AsActivity();

                await stepContext.Context.SendActivityAsync(activity, cancellationToken);
                return await stepContext.CancelAllDialogsAsync(cancellationToken);
            }

            return intent switch
            {
                Intents.Configuration => await stepContext.BeginDialogAsync(nameof(ConfigurationDialog), cancellationToken: cancellationToken),
                Intents.Fact => await stepContext.BeginDialogAsync(nameof(FactDialog), cancellationToken: cancellationToken),

                // We could gracefully respond to the user that their intent is not learned.
                _ => throw new System.NotImplementedException($"The users intent to '{intent}' was not handled."),
            };
        }

        private static class Intents
        {
            public const string Configuration = @"Configure";
            public const string Fact = @"Fact";
        }
    }
}