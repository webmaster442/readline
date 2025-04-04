using System;
using System.Collections.Generic;

namespace ReadLine.Demo;

public static class Program
{
    public static void Main(string[] args)
    {
        var lineReader = new LineReader(new AutoCompletionHandler());

        Console.WriteLine("ReadLine Library Demo");
        Console.WriteLine("---------------------");
        Console.WriteLine();

        string[] history = new string[] { "ls -a", "dotnet run", "git init" };
        lineReader.History.AddRange(history);

        string input = lineReader.Read("(prompt)> ");
        Console.WriteLine(input);

        input = lineReader.ReadPassword("Enter Password> ");
        Console.WriteLine(input);
    }
}

class AutoCompletionHandler : IAutoCompleteHandler
{
    public char[] Separators { get; set; } = new char[] { ' ', '.', '/', '\\', ':' };
    public IReadOnlyList<string> GetSuggestions(string text, int index)
    {
        if (text.StartsWith("git "))
            return new string[] { "init", "clone", "pull", "push" };
        else
            return null;
    }
}
