using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using SlackAPI;
using Dialog = Microsoft.Bot.Builder.Dialogs.Dialog;
using Ada.State;
using Ada.Helpers;

namespace Ada
{
    public class Bot<T> : ActivityHandler where T : Dialog
    {
        private readonly Dialog _dialog;
        private readonly Accessors _accessors;
        private readonly ILogger<Bot<T>> _logger;
        private readonly SlackTaskClient _slackClient;

        public Bot(Accessors accessors, T dialog, ILogger<Bot<T>> logger, SlackTaskClient slackClient)
        {
            _accessors = accessors;
            _dialog = dialog;
            _logger = logger;
            _slackClient = slackClient;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var userScopedState = await _accessors.UserScopedStateAccessor.GetAsync(turnContext, () => new UserScopedState(), cancellationToken);

            turnContext.SetDefaultResponseLocation();

            // Add users name to user state.
            var user = await _slackClient.GetUserById(turnContext.Activity.From.Id);
            userScopedState.UserName = user.real_name;

            // Greet user, if not already greeted.
            if (userScopedState.IsGreeted is false)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Hey {userScopedState.UserName} :wave:!"), cancellationToken);
                userScopedState.IsGreeted = true;
            }

            // Start the main dialog.
            var dialogState = _accessors.ConversationState.CreateProperty<DialogState>(nameof(DialogState));
            await _dialog.RunAsync(turnContext, dialogState, cancellationToken);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _accessors.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _accessors.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _accessors.ConfigurationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
