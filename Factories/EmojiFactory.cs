using System;

namespace Ada.Helpers
{
    public static class EmojiFactory
    {
        private static readonly Random Random = new();

        public static string Nerd  => NerdEmojis[Random.Next(0, NerdEmojis.Length)];
        public static string Smile  => SmileEmojis[Random.Next(0, SmileEmojis.Length)];
        public static string Wave => WaveEmojis[Random.Next(0, WaveEmojis.Length)];

        private static readonly string[] NerdEmojis = new[] { ":nerd_face:" };
        private static readonly string[] SmileEmojis = new[] { ":smile:", ":smile_cat:", ":smiley:", ":grin:", ":grinning:", ":slightly_smiling_face:" };
        private static readonly string[] WaveEmojis = new[] { ":wave:", ":ocean:" };
    }
}