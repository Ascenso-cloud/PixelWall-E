namespace Pixel_Wall_E_2;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _currentTokenIndex;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public List<AstNode> Parse()
    {
        var ast = new List<AstNode>();

        while (!IsAtEnd())
        {
            var node = ParseStatement();
            if (node != null)
                ast.Add(node);
        }

        return ast;
    }
private AstNode ParseStatement()
{
    // Saltar líneas vacías
    while (Match(TokenType.NewLine)) { }

    if (IsAtEnd()) return null;

        // Intenta primero identificar etiquetas
        if (Check(TokenType.Identifier))
        {
            if (_currentTokenIndex + 1 < _tokens.Count &&
           _tokens[_currentTokenIndex + 1].Type == TokenType.Assignment)
            {
                var identifier = Advance();
                return ParseVariableAssignment();
            }
            // Caso especial: posible etiqueta
            var currentToken = Peek();
            var nextToken = _currentTokenIndex + 1 < _tokens.Count ? _tokens[_currentTokenIndex + 1] : null;

            if (!IsReservedKeyword(currentToken.Value) &&
                (nextToken == null || nextToken.Type == TokenType.NewLine || nextToken.Type == TokenType.EOF))
            {
                var labelToken = Advance();
                if (Check(TokenType.NewLine)) Advance();
                return new LabelNode(labelToken.Value, labelToken.LineNumber);
            }
        
    }

    // Parsear instrucciones específicas
    if (Match(TokenType.Spawn)) return ParseSpawn();
    if (Match(TokenType.Color)) return ParseColor();
    if (Match(TokenType.Size)) return ParseSize();
    if (Match(TokenType.DrawLine)) return ParseDrawLine();
    if (Match(TokenType.DrawCircle)) return ParseDrawCircle();
    if (Match(TokenType.DrawRectangle)) return ParseDrawRectangle();
    if (Match(TokenType.Fill)) return ParseFill();
    if (Match(TokenType.GoTo)) return ParseGoTo();
    


    try
    {
        var expr = ParseExpression();
        return new ExpressionStatementNode(expr, expr.LineNumber);
    }
    catch
    {
        
        if (Check(TokenType.Identifier))
        {
            var token = Peek();
            throw new Exception($"Posible etiqueta mal formada en línea {token.LineNumber}. " +
                              $"Las etiquetas deben estar solas en su línea.");
        }
        throw new Exception($"Declaración no válida en la línea {Peek().LineNumber}");
    }
}

private bool IsReservedKeyword(string identifier)
{
    var reservedKeywords = new HashSet<string>
    {
        "Spawn", "Color", "Size", "DrawLine", "DrawCircle", "DrawRectangle", "Fill",
        "GetActualX", "GetActualY", "GetCanvasSize", "GetColorCount",
        "IsBrushColor", "IsBrushSize", "IsCanvasColor", "GoTo"
    };
    return reservedKeywords.Contains(identifier);
}

private bool CheckNext(TokenType type)
{
    if (_currentTokenIndex + 1 >= _tokens.Count) return false;
    return _tokens[_currentTokenIndex + 1].Type == type;
}
    private VariableAssignmentNode ParseVariableAssignment()
    {
        var varNameToken = Previous();
        var varName = varNameToken.Value;
        var lineNumber = varNameToken.LineNumber;

        Expect(TokenType.Assignment, "Se esperaba '<-' después del nombre de la variable");
        
        var expression = ParseExpression();
        return new VariableAssignmentNode(varName, expression, lineNumber);
    }

    private ExpressionNode ParseExpression()
    {
        return ParseLogicalOr();
    }

    private ExpressionNode ParseLogicalOr()
    {
        var left = ParseLogicalAnd();

        while (Match(TokenType.Or))
        {
            var op = Previous().Type;
            var right = ParseLogicalAnd();
            left = new BinaryOperationNode(left, op, right, left.LineNumber);
        }

        return left;
    }

    private ExpressionNode ParseLogicalAnd()
    {
        var left = ParseComparison();

        while (Match(TokenType.And))
        {
            var op = Previous().Type;
            var right = ParseComparison();
            left = new BinaryOperationNode(left, op, right, left.LineNumber);
        }

        return left;
    }

    private ExpressionNode ParseComparison()
    {
        var left = ParseTerm();

        if (Match(TokenType.Equal, TokenType.NotEqual, TokenType.Greater,
                TokenType.GreaterOrEqual, TokenType.Less, TokenType.LessOrEqual))
        {
            var op = Previous().Type;
            var right = ParseTerm();
            return new BinaryOperationNode(left, op, right, left.LineNumber);
        }

        return left;
    }

    private ExpressionNode ParseTerm()
    {
        var left = ParseFactor();

        while (Match(TokenType.Plus, TokenType.Minus))
        {
            var op = Previous().Type;
            var right = ParseFactor();
            left = new BinaryOperationNode(left, op, right, left.LineNumber);
        }

        return left;
    }

    private ExpressionNode ParseFactor()
    {
        var left = ParsePower();

        while (Match(TokenType.Multiply, TokenType.Divide, TokenType.Modulo))
        {
            var op = Previous().Type;
            var right = ParsePower();
            left = new BinaryOperationNode(left, op, right, left.LineNumber);
        }

        return left;
    }

    private ExpressionNode ParsePower()
    {
        var left = ParseUnary();

        if (Match(TokenType.Power))
        {
            var op = Previous().Type;
            var right = ParsePower();
            return new BinaryOperationNode(left, op, right, left.LineNumber);
        }

        return left;
    }

    private ExpressionNode ParseUnary()
    {
        if (Match(TokenType.Minus))
        {
            var op = Previous().Type;
            var right = ParseUnary();
            return new BinaryOperationNode(
                new NumberLiteralNode(0, Previous().LineNumber),
                op,
                right,
                Previous().LineNumber);
        }
        return ParsePrimary();
    }

    private ExpressionNode ParsePrimary()
    {
        if (Match(TokenType.Number))
        {
            if (!int.TryParse(Previous().Value, out int value))
            {
                throw new Exception($"Número no válido en la línea {Previous().LineNumber}");
            }
            return new NumberLiteralNode(value, Previous().LineNumber);
        }

        if (Match(TokenType.String))
        {
            return new StringLiteralNode(Previous().Value, Previous().LineNumber);
        }

        // Manejar funciones como valores primarios
        if (Match(TokenType.GetActualX, TokenType.GetActualY, TokenType.GetCanvasSize, 
                TokenType.GetColorCount, TokenType.IsBrushColor, TokenType.IsBrushSize, 
                TokenType.IsCanvasColor))
        {
            var funcToken = Previous();
            var functionNode = ParseFunctionNode(funcToken);
            return new FunctionCallNode(functionNode, funcToken.LineNumber);
        }

        if (Match(TokenType.Identifier))
        {
            if (Peek().Type == TokenType.LeftParenthesis)
                return ParseFunctionCall();

            return new VariableReferenceNode(Previous().Value, Previous().LineNumber);
        }

        if (Match(TokenType.LeftParenthesis))
        {
            var expr = ParseExpression();
            Expect(TokenType.RightParenthesis, "Se esperaba ')' después de la expresión");
            return expr;
        }

        throw new Exception($"Expresión no válida en la línea {Peek().LineNumber}");
    }

    private ExpressionNode ParseFunctionCall()
    {
        var funcName = Previous().Value;
        var lineNumber = Previous().LineNumber;

        Expect(TokenType.LeftParenthesis, "Se esperaba '(' después del nombre de la función");

        switch (funcName)
        {
            case "GetActualX":
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");
                return new FunctionCallNode(new GetActualXNode(lineNumber), lineNumber);

            case "GetActualY":
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");
                return new FunctionCallNode(new GetActualYNode(lineNumber), lineNumber);

            case "GetCanvasSize":
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");
                return new FunctionCallNode(new GetCanvasSizeNode(lineNumber), lineNumber);

            case "GetColorCount":
                Expect(TokenType.String, "Se esperaba un string de color");
                var color = Previous().Value;
                Expect(TokenType.Comma, "Se esperaba ',' después del color");

                var x1 = ParseExpression();
                Expect(TokenType.Comma, "Se esperaba ',' después de x1");

                var y1 = ParseExpression();
                Expect(TokenType.Comma, "Se esperaba ',' después de y1");

                var x2 = ParseExpression();
                Expect(TokenType.Comma, "Se esperaba ',' después de x2");

                var y2 = ParseExpression();
                Expect(TokenType.RightParenthesis, "Se esperaba ')' después de y2");

                return new FunctionCallNode(
                    new GetColorCountNode(color, x1, y1, x2, y2, lineNumber),
                    lineNumber);

            case "IsBrushColor":
                Expect(TokenType.String, "Se esperaba un string de color");
                var brushColor = Previous().Value;
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");

                return new FunctionCallNode(
                    new IsBrushColorNode(brushColor, lineNumber),
                    lineNumber);

            case "IsBrushSize":
                var brushSize = ParseExpression();
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");

                return new FunctionCallNode(
                    new IsBrushSizeNode(brushSize, lineNumber),
                    lineNumber);

            case "IsCanvasColor":
                Expect(TokenType.String, "Se esperaba un string de color");
                var canvasColor = Previous().Value;
                Expect(TokenType.Comma, "Se esperaba ',' después del color");

                var vertical = ParseExpression();
                Expect(TokenType.Comma, "Se esperaba ',' después de vertical");

                var horizontal = ParseExpression();
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");

                return new FunctionCallNode(
                    new IsCanvasColorNode(canvasColor, vertical, horizontal, lineNumber),
                    lineNumber);

            default:
                throw new Exception($"Función desconocida '{funcName}' en la línea {lineNumber}");
        }
    }

    private FunctionNode ParseFunctionNode(Token funcToken)
    {
        var lineNumber = funcToken.LineNumber;
        
        switch (funcToken.Type)
        {
            case TokenType.GetActualX:
                Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de GetActualX");
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");
                return new GetActualXNode(lineNumber);
                
            case TokenType.GetActualY:
                Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de GetActualY");
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");
                return new GetActualYNode(lineNumber);
                
            case TokenType.GetCanvasSize:
                Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de GetCanvasSize");
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");
                return new GetCanvasSizeNode(lineNumber);
                
            case TokenType.GetColorCount:
                Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de GetColorCount");
                Expect(TokenType.String, "Se esperaba un string de color");
                var color = Previous().Value;
                Expect(TokenType.Comma, "Se esperaba ',' después del color");
                
                var x1 = ParseExpression();
                Expect(TokenType.Comma, "Se esperaba ',' después de x1");
                
                var y1 = ParseExpression();
                Expect(TokenType.Comma, "Se esperaba ',' después de y1");
                
                var x2 = ParseExpression();
                Expect(TokenType.Comma, "Se esperaba ',' después de x2");
                
                var y2 = ParseExpression();
                Expect(TokenType.RightParenthesis, "Se esperaba ')' después de y2");
                
                return new GetColorCountNode(color, x1, y1, x2, y2, lineNumber);
                
            case TokenType.IsBrushColor:
                Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de IsBrushColor");
                Expect(TokenType.String, "Se esperaba un string de color");
                var brushColor = Previous().Value;
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");
                
                return new IsBrushColorNode(brushColor, lineNumber);
                
            case TokenType.IsBrushSize:
                Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de IsBrushSize");
                var size = ParseExpression();
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");
                
                return new IsBrushSizeNode(size, lineNumber);
                
            case TokenType.IsCanvasColor:
                Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de IsCanvasColor");
                Expect(TokenType.String, "Se esperaba un string de color");
                var canvasColor = Previous().Value;
                Expect(TokenType.Comma, "Se esperaba ',' después del color");
                
                var vertical = ParseExpression();
                Expect(TokenType.Comma, "Se esperaba ',' después de vertical");
                
                var horizontal = ParseExpression();
                Expect(TokenType.RightParenthesis, "Se esperaba ')'");
                
                return new IsCanvasColorNode(canvasColor, vertical, horizontal, lineNumber);
                
            default:
                throw new Exception($"Función no soportada en línea {lineNumber}");
        }
    }

    
     private FillNode ParseFill()
    {
        var fillToken = Previous();
        Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de Fill");
        Expect(TokenType.RightParenthesis, "Se esperaba ')' después de Fill");
        return new FillNode(fillToken.LineNumber);
    }
    private SpawnNode ParseSpawn()
    {
        var spawnToken = Previous();
        Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de Spawn");

        var x = ParseExpression();
        Expect(TokenType.Comma, "Se esperaba ',' después de X");

        var y = ParseExpression();
        Expect(TokenType.RightParenthesis, "Se esperaba ')' después de Y");

        return new SpawnNode(x, y, spawnToken.LineNumber);
    }

    private ColorNode ParseColor()
    {
        var colorToken = Previous();
        Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de Color");

        Expect(TokenType.String, "Se esperaba un string de color");
        var color = Previous().Value;

        Expect(TokenType.RightParenthesis, "Se esperaba ')' después del color");

        return new ColorNode(color, colorToken.LineNumber);
    }

    private SizeNode ParseSize()
    {
        var sizeToken = Previous();
        Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de Size");

        var size = ParseExpression();
        Expect(TokenType.RightParenthesis, "Se esperaba ')' después del tamaño");

        return new SizeNode(size, sizeToken.LineNumber);
    }

    private DrawLineNode ParseDrawLine()
    {
        var drawLineToken = Previous();
        Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de DrawLine");

        var dirX = ParseExpression();
        Expect(TokenType.Comma, "Se esperaba ',' después de dirX");

        var dirY = ParseExpression();
        Expect(TokenType.Comma, "Se esperaba ',' después de dirY");

        var distance = ParseExpression();
        Expect(TokenType.RightParenthesis, "Se esperaba ')' después de distance");

        return new DrawLineNode(dirX, dirY, distance, drawLineToken.LineNumber);
    }

    private DrawCircleNode ParseDrawCircle()
    {
        var drawCircleToken = Previous();
        Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de DrawCircle");

        var dirX = ParseExpression();
        Expect(TokenType.Comma, "Se esperaba ',' después de dirX");

        var dirY = ParseExpression();
        Expect(TokenType.Comma, "Se esperaba ',' después de dirY");

        var radius = ParseExpression();
        Expect(TokenType.RightParenthesis, "Se esperaba ')' después de radius");

        return new DrawCircleNode(dirX, dirY, radius, drawCircleToken.LineNumber);
    }

    private DrawRectangleNode ParseDrawRectangle()
    {
        var drawRectToken = Previous();
        Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de DrawRectangle");

        var dirX = ParseExpression();
        Expect(TokenType.Comma, "Se esperaba ',' después de dirX");

        var dirY = ParseExpression();
        Expect(TokenType.Comma, "Se esperaba ',' después de dirY");

        var distance = ParseExpression();
        Expect(TokenType.Comma, "Se esperaba ',' después de distance");

        var width = ParseExpression();
        Expect(TokenType.Comma, "Se esperaba ',' después de width");

        var height = ParseExpression();
        Expect(TokenType.RightParenthesis, "Se esperaba ')' después de height");

        return new DrawRectangleNode(dirX, dirY, distance, width, height, drawRectToken.LineNumber);
    }

    private GoToNode ParseGoTo()
    {
        var goToToken = Previous();
        Expect(TokenType.LeftBracket, "Se esperaba '[' después de GoTo");

        Expect(TokenType.Identifier, "Se esperaba nombre de etiqueta");
        var label = Previous().Value;

        Expect(TokenType.RightBracket, "Se esperaba ']' después de la etiqueta");
        Expect(TokenType.LeftParenthesis, "Se esperaba '(' después de la condición");

        var condition = ParseExpression();
        Expect(TokenType.RightParenthesis, "Se esperaba ')' después de la condición");

        return new GoToNode(label, condition, goToToken.LineNumber);
    }

    private LabelNode ParseLabel()
    {
        var labelName = Previous().Value;
        var lineNumber = Previous().LineNumber;

        return new LabelNode(labelName, lineNumber);
    }

    // Métodos auxiliares
    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _currentTokenIndex++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private Token Peek()
    {
        return _tokens[_currentTokenIndex];
    }

    private Token Previous()
    {
        return _tokens[_currentTokenIndex - 1];
    }

    private void Expect(TokenType type, string errorMessage)
    {
        if (!Check(type))
            throw new Exception($"{errorMessage} en la línea {Peek().LineNumber}");

        Advance();
    }
}