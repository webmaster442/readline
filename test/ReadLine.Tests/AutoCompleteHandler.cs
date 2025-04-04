using System.Collections.Generic;

namespace ReadLine.Tests;

internal class AutoCompleteHandler : IAutoCompleteHandler
{
    public char[] Separators { get; set; } = [' ', '.', '/', '\\', ':'];
    public IReadOnlyList<string> GetSuggestions(string text, int index) => ["World", "Angel", "Love"];
}