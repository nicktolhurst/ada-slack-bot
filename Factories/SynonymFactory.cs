using System;

namespace Ada.Helpers
{
    public static class SynonymFactory
    {
        private static readonly Random Random = new();

        public static string Hello      => HelloSynonyms[Random.Next(0, HelloSynonyms.Length)];
        public static string Okay       => OkaySynonyms[Random.Next(0, OkaySynonyms.Length)];

        private static readonly string[] HelloSynonyms 
            = new[] { "Hello", "Hey", "Hi", "Hola", "Olá" };

        private static readonly string[] OkaySynonyms 
            = new[] { "Okay", "Sure" };
    }
}