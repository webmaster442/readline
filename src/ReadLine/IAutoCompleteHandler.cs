using System.Collections.Generic;

namespace ReadLine;

public interface IAutoCompleteHandler
{
    char[] Separators { get; set; }
    IReadOnlyList<string> GetSuggestions(string text, int index);
}