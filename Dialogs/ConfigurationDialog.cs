using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using Ada.State;
using Microsoft.Extensions.Configuration;
using Ada.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder;
using Ada.Models;
using Ada.Helpers;

namespace Ada.Dialogs
{
    public class ConfigurationDialog : DialogBase<ConfigurationDialog>
    {
        protected override bool EnabledByDefault => true;

        public ConfigurationDialog(Accessors accessors, IConfiguration configuration, ILogger<ConfigurationDialog> logger, LanguageService languageService) 
            : base(nameof(ConfigurationDialog), accessors, configuration, languageService, logger)
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] 
            {
                ConfirmConfigurationUpdate
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ConfirmConfigurationUpdate(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var configuration = await Accessors.BotScopedStateAccessor.GetAsync(stepContext.Context, () => new BotScopedState(), cancellationToken);

            var result = await LanguageService.RecognizeIntentAsync(stepContext.Context, cancellationToken);

            var entities = result.Entities["entities"].ToObject<List<Entity>>();

            var actionKey = entities.First(x => x.Category == "Action").ExtraInformation.First(x => x.ExtraInformationKind == "ListKey").Key;
            var configKey = entities.First(x => x.Category == "Configuration").ExtraInformation.First(x => x.ExtraInformationKind == "ListKey").Key;

            if (configuration.Configuration.TryGetValue(configKey, out bool value))
            {
                bool newValue = actionKey switch
                {
                    "Enable" => true,
                    "Disable" => false,
                    _ => throw new NotImplementedException($"Action `{actionKey}` not implemented.")
                };

                if (value == newValue)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{EmojiFactory.Smile} looks like `{configKey}` is already {actionKey.ToLower()}d."), cancellationToken: cancellationToken);
                }
                else
                {
                    configuration.Configuration[configKey] = newValue;
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"{SynonymFactory.Okay}, I have {actionKey.ToLower()}d `{configKey}`."), cancellationToken: cancellationToken);
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($":confounded: `{configKey}` is not implemented."), cancellationToken: cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}