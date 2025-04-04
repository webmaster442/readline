using System;

namespace ReadLine.Abstractions;

internal sealed class SystemConsole : IConsole
{
    public int CursorLeft => Console.CursorLeft;

    public int CursorTop => Console.CursorTop;

    public int BufferWidth => Console.BufferWidth;

    public int BufferHeight => Console.BufferHeight;

    public bool PasswordMode { get; set; }

    public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);

    public void SetCursorPosition(int left, int top)
    {
        if (!PasswordMode)
            Console.SetCursorPosition(left, top);
    }

    public void Write(string value)
    {
        if (PasswordMode)
            value = new string(default, value.Length);

        Console.Write(value);
    }

    public void WriteLine(string value) => Console.WriteLine(value);
}