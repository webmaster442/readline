using ReadLine.Abstractions;
using System;
using System.Collections.Generic;

namespace ReadLine
{
    public class LineReader
    {
        private readonly IConsole _consoleDriver;
        private readonly KeyHandler _keyHandler;

        public List<string> History { get; }

        public bool HistoryEnabled { get; set; }

        public LineReader(IConsole consoleDriver, IAutoCompleteHandler autoCompleteHandler)
        {
            _consoleDriver = consoleDriver;
            History = new List<string>();
            _keyHandler = new KeyHandler(consoleDriver, History, autoCompleteHandler);
        }

        public LineReader(IAutoCompleteHandler autoCompleteHandler) : this(new SystemConsole(), autoCompleteHandler)
        {
        }

        public string Read(string prompt = "", string @default = "")
        {
            Console.Write(prompt);

            string text = GetText(_keyHandler);

            if (string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(@default))
            {
                text = @default;
            }
            else if (HistoryEnabled)
            {
                History.Add(text);
            }

            return text;
        }

        public string ReadPassword(string prompt = "")
        {
            _consoleDriver.Write(prompt);
            _consoleDriver.PasswordMode = true;
            var result = GetText(_keyHandler);
            _consoleDriver.PasswordMode = false;
            return result;
        }

        private string GetText(KeyHandler keyHandler)
        {
            ConsoleKeyInfo keyInfo = _consoleDriver.ReadKey(true);
            while (keyInfo.Key != ConsoleKey.Enter)
            {
                keyHandler.Handle(keyInfo);
                keyInfo = _consoleDriver.ReadKey(true);
            }

            _consoleDriver.WriteLine(string.Empty);
            return keyHandler.Text;
        }
    }
}
