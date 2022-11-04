using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using Ada.State;
using Microsoft.Extensions.Configuration;
using Ada.Services;
using Ada.Helpers;

namespace Ada.Dialogs
{
    public class FactDialog : DialogBase<FactDialog>
    {
        protected override bool EnabledByDefault => true;
        private readonly FactClient _factClient; 

        public FactDialog(FactClient factClient, Accessors accessors, IConfiguration configuration, ILogger<FactDialog> logger, LanguageService languageService)
            : base(nameof(FactDialog), accessors, configuration, languageService, logger)
        {
            _factClient = factClient;

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                RespondWithFact
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> RespondWithFact(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var fact = await _factClient.GetFactAsync(cancellationToken);

            var activity = BlockFactory
                .CreateNew
                .AddMarkdownBlock($"{SynonymFactory.Okay}. {fact} {EmojiFactory.Nerd}")
                .AsActivity();

            await stepContext.Context.SendActivityAsync(activity, cancellationToken: cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}