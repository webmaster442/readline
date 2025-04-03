using System;
using System.Collections.Generic;

namespace ReadLine.Tests
{
    class AutoCompleteHandler : IAutoCompleteHandler
    {
        public char[] Separators { get; set; } = new char[] { ' ', '.', '/', '\\', ':' };
        public IReadOnlyList<string> GetSuggestions(string text, int index) => new string[] { "World", "Angel", "Love" };
    }
}