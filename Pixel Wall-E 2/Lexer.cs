namespace Pixel_Wall_E_2;
public class Lexer
{
    private readonly string _sourceCode;
    private int _currentPosition;
    private int _lineNumber = 1;

    public Lexer(string sourceCode)
    {
        _sourceCode = sourceCode;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (_currentPosition < _sourceCode.Length)
        {
            char currentChar = _sourceCode[_currentPosition];

            if (char.IsWhiteSpace(currentChar))
            {
                if (currentChar == '\n')
                {
                    tokens.Add(new Token(TokenType.NewLine, "\n", _lineNumber));
                    _lineNumber++;
                }
                _currentPosition++;
            }
            else if (char.IsDigit(currentChar) || (currentChar == '-' && char.IsDigit(Peek())))
            {
                tokens.Add(ReadNumber());
            }
            else if (char.IsLetter(currentChar) || currentChar == '-')
            {
                tokens.Add(ReadIdentifierOrKeyword());
            }
            else if (currentChar == '"')
            {
                tokens.Add(ReadString());
            }
            else if (currentChar == '(')
            {
                tokens.Add(new Token(TokenType.LeftParenthesis, "(", _lineNumber));
                _currentPosition++;
            }
            else if (currentChar == ')')
            {
                tokens.Add(new Token(TokenType.RightParenthesis, ")", _lineNumber));
                _currentPosition++;
            }
            else if (currentChar == '[')
            {
                tokens.Add(new Token(TokenType.LeftBracket, "[", _lineNumber));
                _currentPosition++;
            }
            else if (currentChar == ']')
            {
                tokens.Add(new Token(TokenType.RightBracket, "]", _lineNumber));
                _currentPosition++;
            }
            else if (currentChar == ',')
            {
                tokens.Add(new Token(TokenType.Comma, ",", _lineNumber));
                _currentPosition++;
            }
            else if (currentChar == '<' && Peek() == '-')
            {
                tokens.Add(new Token(TokenType.Assignment, "<-", _lineNumber));
                _currentPosition += 2;
            }
            else if (currentChar == '&' && Peek() == '&')
            {
                tokens.Add(new Token(TokenType.And, "&&", _lineNumber));
                _currentPosition += 2;
            }
            else if (currentChar == '|' && Peek() == '|')
            {
                tokens.Add(new Token(TokenType.Or, "||", _lineNumber));
                _currentPosition += 2;
            }
            else if (currentChar == '=' && Peek() == '=')
            {
                tokens.Add(new Token(TokenType.Equal, "==", _lineNumber));
                _currentPosition += 2;
            }
            else if (currentChar == '>' && Peek() == '=')
            {
                tokens.Add(new Token(TokenType.GreaterOrEqual, ">=", _lineNumber));
                _currentPosition += 2;
            }
            else if (currentChar == '<' && Peek() == '=')
            {
                tokens.Add(new Token(TokenType.LessOrEqual, "<=", _lineNumber));
                _currentPosition += 2;
            }
            else if (currentChar == '>')
            {
                tokens.Add(new Token(TokenType.Greater, ">", _lineNumber));
                _currentPosition++;
            }
            else if (currentChar == '<')
            {
                tokens.Add(new Token(TokenType.Less, "<", _lineNumber));
                _currentPosition++;
            }
            else if (currentChar == '+')
            {
                tokens.Add(new Token(TokenType.Plus, "+", _lineNumber));
                _currentPosition++;
            }
            else if (currentChar == '-' && !char.IsDigit(Peek()))
            {
                tokens.Add(new Token(TokenType.Minus, "-", _lineNumber));
                _currentPosition++;
            }
            else if (currentChar == '*')
            {
                if (Peek() == '*')
                {
                    tokens.Add(new Token(TokenType.Power, "**", _lineNumber));
                    _currentPosition += 2;
                }
                else
                {
                    tokens.Add(new Token(TokenType.Multiply, "*", _lineNumber));
                    _currentPosition++;
                }
            }
            else if (currentChar == '/')
            {
                tokens.Add(new Token(TokenType.Divide, "/", _lineNumber));
                _currentPosition++;
            }
            else if (currentChar == '%')
            {
                tokens.Add(new Token(TokenType.Modulo, "%", _lineNumber));
                _currentPosition++;
            }
            else
            {
                // caracteres desconocidos
                _currentPosition++;
            }
        }

        tokens.Add(new Token(TokenType.EOF, "", _lineNumber));
        return tokens;
    }

    private Token ReadNumber()
    {
        int start = _currentPosition;
        
        // Manejar Numeros Negativos
        if (_sourceCode[_currentPosition] == '-')
        {
            _currentPosition++;
        }

        while (_currentPosition < _sourceCode.Length && char.IsDigit(_sourceCode[_currentPosition]))
        {
            _currentPosition++;
        }

        string numberText = _sourceCode.Substring(start, _currentPosition - start);
        return new Token(TokenType.Number, numberText, _lineNumber);
    }

   private Token ReadString()
{
    _currentPosition++; // Saltar la comilla inicial
    int start = _currentPosition;
    
    while (_currentPosition < _sourceCode.Length && _sourceCode[_currentPosition] != '"')
    {
        _currentPosition++;
    }
    
    if (_currentPosition >= _sourceCode.Length)
    {
        throw new Exception($"String sin terminar en línea {_lineNumber}");
    }
    
    string value = _sourceCode.Substring(start, _currentPosition - start);
    _currentPosition++; // Saltar la comilla final
    
    return new Token(TokenType.String, value, _lineNumber);
}

private Token ReadIdentifierOrKeyword()
{
    int start = _currentPosition;

    while (_currentPosition < _sourceCode.Length &&
          (char.IsLetterOrDigit(_sourceCode[_currentPosition]) || _sourceCode[_currentPosition] == '-'))
    {
        _currentPosition++;
    }

    string value = _sourceCode.Substring(start, _currentPosition - start);

    // Verificar si es una palabra reservada (incluyendo funciones)
    TokenType? keywordType = GetKeywordType(value);
    if (keywordType.HasValue)
    {
        return new Token(keywordType.Value, value, _lineNumber);
    }

    // Si no es keyword, es identificador
    return new Token(TokenType.Identifier, value, _lineNumber);
}
private TokenType? GetKeywordType(string value)
{
    switch (value)
    {
        case "Spawn": return TokenType.Spawn;
        case "Color": return TokenType.Color;
        case "Size": return TokenType.Size;
        case "DrawLine": return TokenType.DrawLine;
        case "DrawCircle": return TokenType.DrawCircle;
        case "DrawRectangle": return TokenType.DrawRectangle;
        case "Fill": return TokenType.Fill;
        case "GetActualX": return TokenType.GetActualX;
        case "GetActualY": return TokenType.GetActualY;
        case "GetCanvasSize": return TokenType.GetCanvasSize;
        case "GetColorCount": return TokenType.GetColorCount;
        case "IsBrushColor": return TokenType.IsBrushColor;
        case "IsBrushSize": return TokenType.IsBrushSize;
        case "IsCanvasColor": return TokenType.IsCanvasColor;
        case "GoTo": return TokenType.GoTo;
        default: return null;
    }
}

    private char Peek()
    {
        if (_currentPosition + 1 >= _sourceCode.Length)
            return '\0';
        return _sourceCode[_currentPosition + 1];
    }
}