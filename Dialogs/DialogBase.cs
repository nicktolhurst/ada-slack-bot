using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ada.Services;
using Ada.State;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ada.Dialogs
{
    public abstract class DialogBase<T> : ComponentDialog where T : ComponentDialog
    {
        private readonly string _dialogId;
        public DialogBase(string dialogId,
            Accessors accessors,
            IConfiguration configuration,
            LanguageService languageService,
            ILogger<T> logger) : base(dialogId)
        {
            _dialogId = dialogId;

            Accessors = accessors;
            Configuration = configuration;
            LanguageService = languageService;
            Logger = logger;
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default)
        {
            var state = await Accessors.BotScopedStateAccessor.GetAsync(innerDc.Context, () => new BotScopedState(), cancellationToken);

            var disabled = state.SetOrGetConfiguration(_dialogId, EnabledByDefault) is false; 


            if (disabled)
            {
                await innerDc.Context.SendActivityAsync(MessageFactory.Text($"`{_dialogId}` is currently disabled :pouting_cat:. If you want to enable it, just ask!"), cancellationToken: cancellationToken);
                return await innerDc.CancelAllDialogsAsync(cancellationToken);
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        public Accessors Accessors { get; }
        public IConfiguration Configuration { get; }
        public LanguageService LanguageService { get; }
        public ILogger<T> Logger { get; }

        protected abstract bool EnabledByDefault { get; }
    }
}