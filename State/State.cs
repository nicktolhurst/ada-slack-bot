using System.Collections.Generic;

namespace Ada.State
{

    public class UserScopedState
    {
        public string UserName { get; set; }
        public bool IsGreeted { get; set; }
        public UserScopedConfiguration Configuration { get; set; }
    }

    public class UserScopedConfiguration
    {
        public bool GreetingsEnabled { get; set; }
    }

    public class BotScopedState
    {
        public string Name { get; set; } = "Ada";
        public Dictionary<string, bool> Configuration { get; set; } = new Dictionary<string, bool>();
        public bool SetOrGetConfiguration(string key, bool enabled = false)
        {
            if (!Configuration.ContainsKey(key))
            {
                Configuration.Add(key, enabled);
            }

            return Configuration[key];
        }
    }
}