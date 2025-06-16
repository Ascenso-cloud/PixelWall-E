using System;
using System.Collections.Generic;
using System.Drawing;

namespace Pixel_Wall_E_2;

public class Interpreter
{
    private readonly List<AstNode> _ast;
    private readonly int _canvasSize;
    private Color[,] _canvas;
    public int _walleX;
    public int _walleY;
    private Color _brushColor = Color.Transparent;
    private int _brushSize = 1;
    private Dictionary<string, object> _variables = new Dictionary<string, object>();
    private Dictionary<string, int> _labels = new Dictionary<string, int>();
    
    public List<string> Errors { get; } = new List<string>();
    public Color[,] Canvas => _canvas;
    
    public Interpreter(List<AstNode> ast, int canvasSize = 256)
    {
        _ast = ast;
        _canvasSize = canvasSize;
        InitializeCanvas();
    }
    
    private void InitializeCanvas()
    {
        _canvas = new Color[_canvasSize, _canvasSize];
        for (int y = 0; y < _canvasSize; y++)
            for (int x = 0; x < _canvasSize; x++)
                _canvas[x, y] = Color.White;
    }
    
    public void Execute()
    {
        try
        {
            PreprocessLabels();
            
            for (int pc = 0; pc < _ast.Count; pc++)
            {
                var currentNode = _ast[pc];
                
                switch (currentNode)
                {
                    case SpawnNode spawn:
                        ExecuteSpawn(spawn);
                        break;
                    case ColorNode color:
                        ExecuteColor(color);
                        break;
                    case SizeNode size:
                        ExecuteSize(size);
                        break;
                    case DrawLineNode drawLine:
                        ExecuteDrawLine(drawLine);
                        break;
                    case DrawCircleNode drawCircle:
                        ExecuteDrawCircle(drawCircle);
                        break;
                    case DrawRectangleNode drawRectangle:
                        ExecuteDrawRectangle(drawRectangle);
                        break;
                    case FillNode fill:
                        ExecuteFill(fill);
                        break;
                    case VariableAssignmentNode assignment:
                        ExecuteVariableAssignment(assignment);
                        break;
                    case GoToNode goTo:
                        pc = ExecuteGoTo(goTo, pc) - 1; // -1 para compensar el pc++
                        break;
                    case LabelNode _:
                        break;
                    default:
                        throw new RuntimeException($"Instrucción no reconocida en línea {currentNode.LineNumber}");
                }
            }
        }
        catch (RuntimeException ex)
        {
            Errors.Add(ex.Message);
        }
    }
    
    private void PreprocessLabels()
    {
        for (int i = 0; i < _ast.Count; i++)
        {
            if (_ast[i] is LabelNode label)
            {
                if (_labels.ContainsKey(label.Name))
                    throw new RuntimeException($"Etiqueta duplicada '{label.Name}' en línea {label.LineNumber}");
                _labels[label.Name] = i;
            }
        }
    }

    private int ExecuteGoTo(GoToNode goTo, int currentPc)
    {
        bool condition = ConvertToBool(EvaluateExpression(goTo.Condition));
        if (condition)
        {
            if (_labels.TryGetValue(goTo.Label, out int targetPc))
                return targetPc;
            throw new RuntimeException($"Etiqueta '{goTo.Label}' no encontrada en línea {goTo.LineNumber}");
        }
        return currentPc + 1;
    }
    
    private void ExecuteSpawn(SpawnNode spawn)
    {
        int x = ConvertToInt(EvaluateExpression(spawn.X));
        int y = ConvertToInt(EvaluateExpression(spawn.Y));
        
        if (x < 0 || x >= _canvasSize || y < 0 || y >= _canvasSize)
            throw new RuntimeException($"Posición inicial ({x}, {y}) fuera del canvas");
        
        _walleX = x;
        _walleY = y;
    }
    
    private void ExecuteColor(ColorNode color)
    {
        _brushColor = color.Color switch
        {
            "Red" => Color.Red,
            "Blue" => Color.Blue,
            "Green" => Color.Green,
            "Yellow" => Color.Yellow,
            "Orange" => Color.Orange,
            "Purple" => Color.Purple,
            "Black" => Color.Black,
            "White" => Color.White,
            "Transparent" => Color.Transparent,
            _ => throw new RuntimeException($"Color '{color.Color}' no válido en línea {color.LineNumber}")
        };
    }
    
    private void ExecuteSize(SizeNode size)
    {
        int sizeValue = ConvertToInt(EvaluateExpression(size.Size));
        
        if (sizeValue <= 0)
            throw new RuntimeException($"Tamaño del pincel debe ser mayor que 0 en línea {size.LineNumber}");
        
        _brushSize = sizeValue % 2 == 0 ? sizeValue - 1 : sizeValue;
    }
    
    private void ExecuteDrawLine(DrawLineNode drawLine)
    {
        int dirX = ConvertToInt(EvaluateExpression(drawLine.DirX));
        int dirY = ConvertToInt(EvaluateExpression(drawLine.DirY));
        int distance = ConvertToInt(EvaluateExpression(drawLine.Distance));
        
        if (!IsValidDirection(dirX) || !IsValidDirection(dirY))
            throw new RuntimeException($"Dirección ({dirX}, {dirY}) no válida para DrawLine en línea {drawLine.LineNumber}");
        
        if (distance <= 0)
            throw new RuntimeException($"Distancia debe ser mayor que 0 en línea {drawLine.LineNumber}");
        
        int endX = _walleX + dirX * distance;
        int endY = _walleY + dirY * distance;
        
        if (!IsPositionValid(endX, endY))
            throw new RuntimeException($"Posición final ({endX}, {endY}) fuera del canvas en línea {drawLine.LineNumber}");
        
        DrawBresenhamLine(_walleX, _walleY, endX, endY);
        _walleX = endX;
        _walleY = endY;
    }
    
    private void ExecuteDrawCircle(DrawCircleNode drawCircle)
    {
        int dirX = ConvertToInt(EvaluateExpression(drawCircle.DirX));
        int dirY = ConvertToInt(EvaluateExpression(drawCircle.DirY));
        int radius = ConvertToInt(EvaluateExpression(drawCircle.Radius));
        
        if (!IsValidDirection(dirX) || !IsValidDirection(dirY))
            throw new RuntimeException($"Dirección ({dirX}, {dirY}) no válida para DrawCircle en línea {drawCircle.LineNumber}");
        
        if (radius <= 0)
            throw new RuntimeException($"Radio debe ser mayor que 0 en línea {drawCircle.LineNumber}");
        
        int centerX = _walleX + dirX * radius;
        int centerY = _walleY + dirY * radius;
        
        if (!IsPositionValid(centerX, centerY))
            throw new RuntimeException($"Centro del círculo ({centerX}, {centerY}) fuera del canvas en línea {drawCircle.LineNumber}");
        
        DrawMidpointCircle(centerX, centerY, radius);
        _walleX = centerX;
        _walleY = centerY;
    }
    
    private void ExecuteDrawRectangle(DrawRectangleNode drawRectangle)
    {
        int dirX = ConvertToInt(EvaluateExpression(drawRectangle.DirX));
        int dirY = ConvertToInt(EvaluateExpression(drawRectangle.DirY));
        int distance = ConvertToInt(EvaluateExpression(drawRectangle.Distance));
        int width = ConvertToInt(EvaluateExpression(drawRectangle.Width));
        int height = ConvertToInt(EvaluateExpression(drawRectangle.Height));
        
        if (!IsValidDirection(dirX) || !IsValidDirection(dirY))
            throw new RuntimeException($"Dirección ({dirX}, {dirY}) no válida para DrawRectangle en línea {drawRectangle.LineNumber}");
        
        if (width <= 0 || height <= 0)
            throw new RuntimeException($"Ancho y alto deben ser mayores que 0 en línea {drawRectangle.LineNumber}");
        
        if (distance <= 0)
            throw new RuntimeException($"Distancia debe ser mayor que 0 en línea {drawRectangle.LineNumber}");
        
        int centerX = _walleX + dirX * distance;
        int centerY = _walleY + dirY * distance;
        
        if (!IsPositionValid(centerX, centerY))
            throw new RuntimeException($"Centro del rectángulo ({centerX}, {centerY}) fuera del canvas en línea {drawRectangle.LineNumber}");
        
        DrawRectangle(centerX, centerY, width, height);
        _walleX = centerX;
        _walleY = centerY;
    }
    
    private void ExecuteFill(FillNode fill)
    {
        if (_brushColor == Color.Transparent)
        {
            Errors.Add($"Error en línea {fill.LineNumber}: No se puede rellenar con color transparente");
            return;
        }

        if (!IsPositionValid(_walleX, _walleY))
        {
            Errors.Add($"Error en línea {fill.LineNumber}: Posición actual fuera del canvas");
            return;
        }

        Color targetColor = _canvas[_walleX, _walleY];
        if (targetColor == _brushColor) 
            return;

        FloodFill(_walleX, _walleY, targetColor);
    }
    
    private void ExecuteVariableAssignment(VariableAssignmentNode assignment)
    {
        object value = EvaluateExpression(assignment.Expression);
        value = UnwrapValue(value);

        if (value is double d)
        {
            value = (int)d; // Convertir double a int
        }

        if (value is int || value is bool || value is string)
        {
            _variables[assignment.VariableName] = value;
        }
        else
        {
            throw new RuntimeException($"Tipo no soportado para asignación en línea {assignment.LineNumber}");
        }
    }
    
    private object EvaluateExpression(ExpressionNode expr)
    {
        try
        {
            switch (expr)
            {
                case NumberLiteralNode num:
                    return num.Value;
                case BooleanLiteralNode boolNode:
                    return boolNode.Value;
                case StringLiteralNode str:
                    return str.Value;
                case VariableReferenceNode varRef:
                    if (!_variables.TryGetValue(varRef.VariableName, out object value))
                    {
                        throw new RuntimeException($"Variable '{varRef.VariableName}' no definida en línea {varRef.LineNumber}");
                    }
                    return value;
                case BinaryOperationNode binOp:
                    object left = EvaluateExpression(binOp.Left);
                    object right = EvaluateExpression(binOp.Right);

                    // Operaciones aritméticas
                    if (binOp.Operator == TokenType.Plus || binOp.Operator == TokenType.Minus ||
                        binOp.Operator == TokenType.Multiply || binOp.Operator == TokenType.Divide ||
                        binOp.Operator == TokenType.Power || binOp.Operator == TokenType.Modulo)
                    {
                        double dl = Convert.ToDouble(UnwrapValue(left));
                        double dr = Convert.ToDouble(UnwrapValue(right));

                        double result = binOp.Operator switch
                        {
                            TokenType.Plus => dl + dr,
                            TokenType.Minus => dl - dr,
                            TokenType.Multiply => dl * dr,
                            TokenType.Divide => dr == 0 ? throw new RuntimeException($"División por cero en línea {binOp.LineNumber}") : dl / dr,
                            TokenType.Power => Math.Pow(dl, dr),
                            TokenType.Modulo => dr == 0 ? throw new RuntimeException($"Módulo por cero en línea {binOp.LineNumber}") : dl % dr,
                            _ => throw new RuntimeException($"Operador no soportado: {binOp.Operator} en línea {binOp.LineNumber}")
                        };
                        return (int)result; // Forzar resultado entero
                    }
                    // Operaciones de comparación
                    else
                    {
                        return binOp.Operator switch
                        {
                            TokenType.Equal => Equals(UnwrapValue(left), UnwrapValue(right)),
                            TokenType.NotEqual => !Equals(UnwrapValue(left), UnwrapValue(right)),
                            TokenType.Greater => Compare(UnwrapValue(left), UnwrapValue(right)) > 0,
                            TokenType.GreaterOrEqual => Compare(UnwrapValue(left), UnwrapValue(right)) >= 0,
                            TokenType.Less => Compare(UnwrapValue(left), UnwrapValue(right)) < 0,
                            TokenType.LessOrEqual => Compare(UnwrapValue(left), UnwrapValue(right)) <= 0,
                            TokenType.And => ConvertToBool(left) && ConvertToBool(right),
                            TokenType.Or => ConvertToBool(left) || ConvertToBool(right),
                            _ => throw new RuntimeException($"Operador no soportado entre tipos en línea {binOp.LineNumber}")
                        };
                    }
                case FunctionCallNode funcCall:
                    return EvaluateFunctionCall(funcCall.Function);
                default:
                    throw new RuntimeException($"Expresión no soportada en línea {expr.LineNumber}");
            }
        }
        catch (RuntimeException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RuntimeException($"Error al evaluar expresión en línea {expr.LineNumber}: {ex.Message}");
        }
    }

    private object EvaluateFunctionCall(FunctionNode function)
    {
        try
        {
            switch (function)
            {
                case GetActualXNode _:
                    return _walleX;
                case GetActualYNode _:
                    return _walleY;
                case GetCanvasSizeNode _:
                    return _canvasSize;
                case GetColorCountNode colorCount:
                    string color = colorCount.Color;
                    int x1 = ConvertToInt(EvaluateExpression(colorCount.X1));
                    int y1 = ConvertToInt(EvaluateExpression(colorCount.Y1));
                    int x2 = ConvertToInt(EvaluateExpression(colorCount.X2));
                    int y2 = ConvertToInt(EvaluateExpression(colorCount.Y2));
                    return CountColorInArea(color, x1, y1, x2, y2);
                case IsBrushColorNode brushColor:
                    Color targetColor = GetColorFromString(brushColor.Color);
                    return _brushColor == targetColor ? 1 : 0;
                case IsBrushSizeNode brushSize:
                    int size = ConvertToInt(EvaluateExpression(brushSize.Size));
                    return _brushSize == size ? 1 : 0;
                case IsCanvasColorNode canvasColor:
                    string colorName = canvasColor.Color;
                    int vertical = ConvertToInt(EvaluateExpression(canvasColor.Vertical));
                    int horizontal = ConvertToInt(EvaluateExpression(canvasColor.Horizontal));
                    return CheckCanvasColor(colorName, vertical, horizontal) ? 1 : 0;
                default:
                    throw new RuntimeException($"Función no soportada en línea {function.LineNumber}");
            }
        }
        catch (RuntimeException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RuntimeException($"Error al ejecutar función en línea {function.LineNumber}: {ex.Message}");
        }
    }

    private object UnwrapValue(object value)
    {
        return value switch
        {
            int or bool or string => value,
            double d => (int)d,
            _ => throw new RuntimeException($"Tipo no convertible: {value?.GetType().Name}")
        };
    }

    private int CountColorInArea(string color, int x1, int y1, int x2, int y2)
    {
        Color targetColor = GetColorFromString(color);
        int startX = Math.Clamp(Math.Min(x1, x2), 0, _canvasSize - 1);
        int endX = Math.Clamp(Math.Max(x1, x2), 0, _canvasSize - 1);
        int startY = Math.Clamp(Math.Min(y1, y2), 0, _canvasSize - 1);
        int endY = Math.Clamp(Math.Max(y1, y2), 0, _canvasSize - 1);
        
        int count = 0;
        for (int y = startY; y <= endY; y++)
            for (int x = startX; x <= endX; x++)
                if (_canvas[x, y] == targetColor)
                    count++;
        
        return count;
    }
    
    private bool CheckCanvasColor(string color, int vertical, int horizontal)
    {
        int x = _walleX + horizontal;
        int y = _walleY + vertical;
        
        if (x < 0 || x >= _canvasSize || y < 0 || y >= _canvasSize)
            return false;
            
        return _canvas[x, y] == GetColorFromString(color);
    }
    
    private Color GetColorFromString(string color)
    {
        return color switch
        {
            "Red" => Color.Red,
            "Blue" => Color.Blue,
            "Green" => Color.Green,
            "Yellow" => Color.Yellow,
            "Orange" => Color.Orange,
            "Purple" => Color.Purple,
            "Black" => Color.Black,
            "White" => Color.White,
            "Transparent" => Color.Transparent,
            _ => throw new RuntimeException($"Color '{color}' no válido")
        };
    }

    private int ConvertToInt(object value)
    {
        return value switch
        {
            int i => i,
            bool b => b ? 1 : 0,
            double d => (int)d,
            _ => throw new RuntimeException($"No se puede convertir {value?.GetType().Name} a entero")
        };
    }

    private bool ConvertToBool(object value)
    {
        return value switch
        {
            bool b => b,
            int i => i != 0,
            double d => d != 0,
            _ => throw new RuntimeException($"No se puede convertir {value?.GetType().Name} a booleano")
        };
    }

    private int Compare(object a, object b)
    {
        try
        {
            double da = Convert.ToDouble(UnwrapValue(a));
            double db = Convert.ToDouble(UnwrapValue(b));
            return da.CompareTo(db);
        }
        catch
        {
            throw new RuntimeException($"No se pueden comparar los tipos {a?.GetType().Name} y {b?.GetType().Name}");
        }
    }

    private void DrawBresenhamLine(int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawPixelWithBrushSize(x0, y0);

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private void DrawMidpointCircle(int centerX, int centerY, int radius)
    {
        int x = radius;
        int y = 0;
        int err = 0;

        while (x >= y)
        {
            DrawPixelWithBrushSize(centerX + x, centerY + y);
            DrawPixelWithBrushSize(centerX + y, centerY + x);
            DrawPixelWithBrushSize(centerX - y, centerY + x);
            DrawPixelWithBrushSize(centerX - x, centerY + y);
            DrawPixelWithBrushSize(centerX - x, centerY - y);
            DrawPixelWithBrushSize(centerX - y, centerY - x);
            DrawPixelWithBrushSize(centerX + y, centerY - x);
            DrawPixelWithBrushSize(centerX + x, centerY - y);

            if (err <= 0)
            {
                y += 1;
                err += 2 * y + 1;
            }
            if (err > 0)
            {
                x -= 1;
                err -= 2 * x + 1;
            }
        }
    }

    private void DrawRectangle(int centerX, int centerY, int width, int height)
    {
        int halfWidth = width / 2;
        int halfHeight = height / 2;
        
        for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
        {
            DrawPixelWithBrushSize(x, centerY - halfHeight);
            DrawPixelWithBrushSize(x, centerY + halfHeight);
        }
        
        for (int y = centerY - halfHeight + 1; y < centerY + halfHeight; y++)
        {
            DrawPixelWithBrushSize(centerX - halfWidth, y);
            DrawPixelWithBrushSize(centerX + halfWidth, y);
        }
    }

    private void FloodFill(int x, int y, Color targetColor)
    {
        if (!IsPositionValid(x, y) || _canvas[x, y] != targetColor || _canvas[x, y] == _brushColor)
            return;

        Stack<Point> pixels = new Stack<Point>();
        pixels.Push(new Point(x, y));

        while (pixels.Count > 0)
        {
            Point current = pixels.Pop();
            if (!IsPositionValid(current.X, current.Y) || _canvas[current.X, current.Y] != targetColor)
                continue;

            _canvas[current.X, current.Y] = _brushColor;

            pixels.Push(new Point(current.X + 1, current.Y));
            pixels.Push(new Point(current.X - 1, current.Y));
            pixels.Push(new Point(current.X, current.Y + 1));
            pixels.Push(new Point(current.X, current.Y - 1));
        }
    }

    private void DrawPixelWithBrushSize(int x, int y)
    {
        if (_brushColor == Color.Transparent)
            return;

        int halfSize = _brushSize / 2;
        for (int dy = -halfSize; dy <= halfSize; dy++)
        {
            for (int dx = -halfSize; dx <= halfSize; dx++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (IsPositionValid(nx, ny))
                {
                    _canvas[nx, ny] = _brushColor;
                }
            }
        }
    }

    private bool IsValidDirection(int value) => value == -1 || value == 0 || value == 1;
    private bool IsPositionValid(int x, int y) => x >= 0 && x < _canvasSize && y >= 0 && y < _canvasSize;
}

public class RuntimeException : Exception
{
    public RuntimeException(string message) : base(message) { }
}