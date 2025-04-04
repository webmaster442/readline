using ReadLine.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadLine;

internal sealed class KeyHandler
{
    private readonly StringBuilder _text;
    private readonly List<string> _history;
    private readonly Dictionary<string, Action> _keyActions;
    private readonly IConsole _consoleDriver;

    private int _cursorPos;
    private int _cursorLimit;
    private int _historyIndex;
    private ConsoleKeyInfo _keyInfo;
    private IReadOnlyList<string> _completions;
    private int _completionStart;
    private int _completionsIndex;

    private bool IsStartOfLine()
        => _cursorPos == 0;

    private bool IsEndOfLine()
        => _cursorPos == _cursorLimit;

    private bool IsEndOfBuffer()
        => _consoleDriver.CursorLeft == _consoleDriver.BufferWidth - 1;

    private bool IsInAutoCompleteMode()
        => _completions != null;

    private void MoveCursorLeft()
        => MoveCursorLeft(1);

    private void MoveCursorLeft(int count)
    {
        if (count > _cursorPos)
            count = _cursorPos;

        if (count > _consoleDriver.CursorLeft)
            _consoleDriver.SetCursorPosition(_consoleDriver.BufferWidth - 1, _consoleDriver.CursorTop - 1);
        else
            _consoleDriver.SetCursorPosition(_consoleDriver.CursorLeft - count, _consoleDriver.CursorTop);

        _cursorPos -= count;
    }

    private void MoveCursorHome()
        => MoveCursorLeft(_cursorPos);

    private string BuildKeyInput()
    {
        return _keyInfo.Modifiers != ConsoleModifiers.Control && _keyInfo.Modifiers != ConsoleModifiers.Shift
            ? _keyInfo.Key.ToString()
            : $"{_keyInfo.Modifiers}{_keyInfo.Key}";
    }

    private void MoveCursorRight()
    {
        if (IsEndOfLine())
            return;

        if (IsEndOfBuffer())
            _consoleDriver.SetCursorPosition(0, _consoleDriver.CursorTop + 1);
        else
            _consoleDriver.SetCursorPosition(_consoleDriver.CursorLeft + 1, _consoleDriver.CursorTop);

        _cursorPos++;
    }

    private void MoveCursorEnd()
    {
        while (!IsEndOfLine())
            MoveCursorRight();
    }

    private void ClearLine()
    {
        MoveCursorEnd();
        Backspace(_cursorPos);
    }

    private void WriteNewString(string str)
    {
        ClearLine();
        foreach (char character in str)
            WriteChar(character);
    }

    private void WriteString(string str)
    {
        foreach (char character in str)
            WriteChar(character);
    }

    private void WriteChar()
    {
        if (!char.IsControl(_keyInfo.KeyChar))
            WriteChar(_keyInfo.KeyChar);
    }

    private void WriteChar(char c)
    {
        if (IsEndOfLine())
        {
            _text.Append(c);
            _consoleDriver.Write(c.ToString());
            _cursorPos++;
        }
        else
        {
            int left = _consoleDriver.CursorLeft;
            int top = _consoleDriver.CursorTop;
            string str = _text.ToString()[_cursorPos..];
            _text.Insert(_cursorPos, c);
            _consoleDriver.Write(c.ToString() + str);
            _consoleDriver.SetCursorPosition(left, top);
            MoveCursorRight();
        }

        _cursorLimit++;
    }

    private void Backspace()
    {
        Backspace(1);
    }

    private void Backspace(int count)
    {
        if (count > _cursorPos)
            count = _cursorPos;

        MoveCursorLeft(count);
        int index = _cursorPos;
        _text.Remove(index, count);
        string replacement = _text.ToString()[index..];
        int left = _consoleDriver.CursorLeft;
        int top = _consoleDriver.CursorTop;
        string spaces = new(' ', count);
        _consoleDriver.Write(string.Format("{0}{1}", replacement, spaces));
        _consoleDriver.SetCursorPosition(left, top);
        _cursorLimit -= count;
    }

    private void Delete()
    {
        if (IsEndOfLine())
            return;

        int index = _cursorPos;
        _text.Remove(index, 1);
        string replacement = _text.ToString()[index..];
        int left = _consoleDriver.CursorLeft;
        int top = _consoleDriver.CursorTop;
        _consoleDriver.Write(string.Format("{0} ", replacement));
        _consoleDriver.SetCursorPosition(left, top);
        _cursorLimit--;
    }

    private void TransposeChars()
    {
        // local helper functions
        bool almostEndOfLine() => _cursorLimit - _cursorPos == 1;
        int incrementIf(Func<bool> expression, int index) => expression() ? index + 1 : index;
        int decrementIf(Func<bool> expression, int index) => expression() ? index - 1 : index;

        if (IsStartOfLine()) { return; }

        var firstIdx = decrementIf(IsEndOfLine, _cursorPos - 1);
        var secondIdx = decrementIf(IsEndOfLine, _cursorPos);

        char secondChar = _text[secondIdx];
        _text[secondIdx] = _text[firstIdx];
        _text[firstIdx] = secondChar;

        var left = incrementIf(almostEndOfLine, _consoleDriver.CursorLeft);
        var cursorPosition = incrementIf(almostEndOfLine, _cursorPos);

        WriteNewString(_text.ToString());

        _consoleDriver.SetCursorPosition(left, _consoleDriver.CursorTop);
        _cursorPos = cursorPosition;

        MoveCursorRight();
    }

    private void StartAutoComplete()
    {
        Backspace(_cursorPos - _completionStart);

        _completionsIndex = 0;

        WriteString(_completions[_completionsIndex]);
    }

    private void NextAutoComplete()
    {
        Backspace(_cursorPos - _completionStart);

        _completionsIndex++;

        if (_completionsIndex == _completions.Count)
            _completionsIndex = 0;

        WriteString(_completions[_completionsIndex]);
    }

    private void PreviousAutoComplete()
    {
        Backspace(_cursorPos - _completionStart);

        _completionsIndex--;

        if (_completionsIndex == -1)
            _completionsIndex = _completions.Count - 1;

        WriteString(_completions[_completionsIndex]);
    }

    private void PrevHistory()
    {
        if (_historyIndex > 0)
        {
            _historyIndex--;
            WriteNewString(_history[_historyIndex]);
        }
    }

    private void NextHistory()
    {
        if (_historyIndex < _history.Count)
        {
            _historyIndex++;
            if (_historyIndex == _history.Count)
                ClearLine();
            else
                WriteNewString(_history[_historyIndex]);
        }
    }

    private void ResetAutoComplete()
    {
        _completions = null;
        _completionsIndex = 0;
    }

    public string Text
    {
        get
        {
            return _text.ToString();
        }
    }

    public KeyHandler(IConsole console, List<string> history, IAutoCompleteHandler autoCompleteHandler)
    {
        _consoleDriver = console;

        _history = history ?? new List<string>();
        _historyIndex = _history.Count;
        _text = new StringBuilder();
        _keyActions = new Dictionary<string, Action>();

        _keyActions["LeftArrow"] = MoveCursorLeft;
        _keyActions["Home"] = MoveCursorHome;
        _keyActions["End"] = MoveCursorEnd;
        _keyActions["ControlA"] = MoveCursorHome;
        _keyActions["ControlB"] = MoveCursorLeft;
        _keyActions["RightArrow"] = MoveCursorRight;
        _keyActions["ControlF"] = MoveCursorRight;
        _keyActions["ControlE"] = MoveCursorEnd;
        _keyActions["Backspace"] = Backspace;
        _keyActions["Delete"] = Delete;
        _keyActions["ControlD"] = Delete;
        _keyActions["ControlH"] = Backspace;
        _keyActions["ControlL"] = ClearLine;
        _keyActions["Escape"] = ClearLine;
        _keyActions["UpArrow"] = PrevHistory;
        _keyActions["ControlP"] = PrevHistory;
        _keyActions["DownArrow"] = NextHistory;
        _keyActions["ControlN"] = NextHistory;
        _keyActions["ControlU"] = () => Backspace(_cursorPos);
        _keyActions["ControlK"] = () =>
        {
            int pos = _cursorPos;
            MoveCursorEnd();
            Backspace(_cursorPos - pos);
        };
        _keyActions["ControlW"] = () =>
        {
            while (!IsStartOfLine() && _text[_cursorPos - 1] != ' ')
                Backspace();
        };
        _keyActions["ControlT"] = TransposeChars;

        _keyActions["Tab"] = () =>
        {
            if (IsInAutoCompleteMode())
            {
                NextAutoComplete();
            }
            else
            {
                if (autoCompleteHandler == null || !IsEndOfLine())
                    return;

                string text = _text.ToString();

                _completionStart = text.LastIndexOfAny(autoCompleteHandler.Separators);
                _completionStart = _completionStart == -1 ? 0 : _completionStart + 1;

                _completions = autoCompleteHandler.GetSuggestions(text, _completionStart);
                _completions = _completions?.Count == 0 ? null : _completions;

                if (_completions == null)
                    return;

                StartAutoComplete();
            }
        };

        _keyActions["ShiftTab"] = () =>
        {
            if (IsInAutoCompleteMode())
            {
                PreviousAutoComplete();
            }
        };
    }

    public void Handle(ConsoleKeyInfo keyInfo)
    {
        _keyInfo = keyInfo;

        // If in auto complete mode and Tab wasn't pressed
        if (IsInAutoCompleteMode() && _keyInfo.Key != ConsoleKey.Tab)
            ResetAutoComplete();

        _keyActions.TryGetValue(BuildKeyInput(), out Action action);
        action ??= WriteChar;
        action.Invoke();
    }
}