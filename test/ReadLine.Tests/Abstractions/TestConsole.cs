using ReadLine.Abstractions;
using System;

namespace ReadLine.Tests.Abstractions;

internal class TestConsole : IConsole
{
    public int CursorLeft => _cursorLeft;

    public int CursorTop => _cursorTop;

    public int BufferWidth => _bufferWidth;

    public int BufferHeight => _bufferHeight;

    public bool PasswordMode { get; set; }

    private int _cursorLeft;
    private int _cursorTop;
    private int _bufferWidth;
    private int _bufferHeight;

    public TestConsole()
    {
        _cursorLeft = 0;
        _cursorTop = 0;
        _bufferWidth = 100;
        _bufferHeight = 100;
    }

    public void SetBufferSize(int width, int height)
    {
        _bufferWidth = width;
        _bufferHeight = height;
    }

    public void SetCursorPosition(int left, int top)
    {
        _cursorLeft = left;
        _cursorTop = top;
    }

    public void Write(string value)
    {
        _cursorLeft += value.Length;
    }

    public void WriteLine(string value)
    {
        _cursorLeft += value.Length;
    }

    public ConsoleKeyInfo ReadKey(bool intercept)
    {
        return new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);
    }
}