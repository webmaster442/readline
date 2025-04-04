using System;

namespace ReadLine.Abstractions;

public interface IConsole
{
    int CursorLeft { get; }
    int CursorTop { get; }
    int BufferWidth { get; }
    int BufferHeight { get; }
    void SetCursorPosition(int left, int top);
    void Write(string value);
    void WriteLine(string value);
    bool PasswordMode { get; set; }
    ConsoleKeyInfo ReadKey(bool intercept);
}