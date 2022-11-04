using Microsoft.Bot.Builder;

namespace Ada.State
{
    public class ConfigurationState : BotState
    {
        public ConfigurationState(IStorage storage, string contextKey) : base(storage, contextKey)
        {
        }
        
        protected override string GetStorageKey(ITurnContext turnContext)
        {
            return "__CONF:GLOBAL";
        }
    }
}