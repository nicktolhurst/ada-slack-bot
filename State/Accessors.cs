using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace Ada.State
{
    public class Accessors
    {
        public Accessors(ConversationState conversationState, UserState userState, ConfigurationState configurationState)
        {
            ConversationState = conversationState;
            UserState = userState;
            ConfigurationState = configurationState;
        }

        public ConversationState ConversationState { get; }
        public UserState UserState { get; }
        public ConfigurationState ConfigurationState { get; }

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }
        public IStatePropertyAccessor<UserScopedState> UserScopedStateAccessor { get; set; }
        public IStatePropertyAccessor<BotScopedState> BotScopedStateAccessor { get; set; }
    }
}