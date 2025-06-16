namespace Pixel_Wall_E_2;

// Nodos AST
public abstract class AstNode
{
    public int LineNumber { get; protected set; }
}

// Nodos de Instruccion
public class SpawnNode : AstNode
{
    public ExpressionNode X { get; }
    public ExpressionNode Y { get; }
    
    public SpawnNode(ExpressionNode x, ExpressionNode y, int lineNumber)
    {
        X = x;
        Y = y;
        LineNumber = lineNumber;
    }
}

public class ColorNode : AstNode
{
    public string Color { get; }
    
    public ColorNode(string color, int lineNumber)
    {
        Color = color;
        LineNumber = lineNumber;
    }
}

public class SizeNode : AstNode
{
    public ExpressionNode Size { get; }
    
    public SizeNode(ExpressionNode size, int lineNumber)
    {
        Size = size;
        LineNumber = lineNumber;
    }
}

public class DrawLineNode : AstNode
{
    public ExpressionNode DirX { get; }
    public ExpressionNode DirY { get; }
    public ExpressionNode Distance { get; }
    
    public DrawLineNode(ExpressionNode dirX, ExpressionNode dirY, ExpressionNode distance, int lineNumber)
    {
        DirX = dirX;
        DirY = dirY;
        Distance = distance;
        LineNumber = lineNumber;
    }
}

public class DrawCircleNode : AstNode
{
    public ExpressionNode DirX { get; }
    public ExpressionNode DirY { get; }
    public ExpressionNode Radius { get; }
    
    public DrawCircleNode(ExpressionNode dirX, ExpressionNode dirY, ExpressionNode radius, int lineNumber)
    {
        DirX = dirX;
        DirY = dirY;
        Radius = radius;
        LineNumber = lineNumber;
    }
}

public class DrawRectangleNode : AstNode
{
    public ExpressionNode DirX { get; }
    public ExpressionNode DirY { get; }
    public ExpressionNode Distance { get; }
    public ExpressionNode Width { get; }
    public ExpressionNode Height { get; }
    
    public DrawRectangleNode(ExpressionNode dirX, ExpressionNode dirY, ExpressionNode distance, 
                           ExpressionNode width, ExpressionNode height, int lineNumber)
    {
        DirX = dirX;
        DirY = dirY;
        Distance = distance;
        Width = width;
        Height = height;
        LineNumber = lineNumber;
    }
}

public class FillNode : AstNode
{
    public FillNode(int lineNumber)
    {
        LineNumber = lineNumber;
    }
}

// Nodos de Funcion
public abstract class FunctionNode : AstNode { }

public class GetActualXNode : FunctionNode
{
    public GetActualXNode(int lineNumber)
    {
        LineNumber = lineNumber;
    }
}

public class GetActualYNode : FunctionNode
{
    public GetActualYNode(int lineNumber)
    {
        LineNumber = lineNumber;
    }
}

public class GetCanvasSizeNode : FunctionNode
{
    public GetCanvasSizeNode(int lineNumber)
    {
        LineNumber = lineNumber;
    }
}

public class GetColorCountNode : FunctionNode
{
    public string Color { get; }
    public ExpressionNode X1 { get; }
    public ExpressionNode Y1 { get; }
    public ExpressionNode X2 { get; }
    public ExpressionNode Y2 { get; }
    
    public GetColorCountNode(string color, ExpressionNode x1, ExpressionNode y1, 
                           ExpressionNode x2, ExpressionNode y2, int lineNumber)
    {
        Color = color;
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
        LineNumber = lineNumber;
    }
}

public class IsBrushColorNode : FunctionNode
{
    public string Color { get; }
    
    public IsBrushColorNode(string color, int lineNumber)
    {
        Color = color;
        LineNumber = lineNumber;
    }
}

public class IsBrushSizeNode : FunctionNode
{
    public ExpressionNode Size { get; }
    
    public IsBrushSizeNode(ExpressionNode size, int lineNumber)
    {
        Size = size;
        LineNumber = lineNumber;
    }
}

public class IsCanvasColorNode : FunctionNode
{
    public string Color { get; }
    public ExpressionNode Vertical { get; }
    public ExpressionNode Horizontal { get; }
    
    public IsCanvasColorNode(string color, ExpressionNode vertical, ExpressionNode horizontal, int lineNumber)
    {
        Color = color;
        Vertical = vertical;
        Horizontal = horizontal;
        LineNumber = lineNumber;
    }
}

// Nodos de Control de Flujo
public class LabelNode : AstNode
{
    public string Name { get; }
    
    public LabelNode(string name, int lineNumber)
    {
        Name = name;
        LineNumber = lineNumber;
    }
}

public class GoToNode : AstNode
{
    public string Label { get; }
    public ExpressionNode Condition { get; }
    
    public GoToNode(string label, ExpressionNode condition, int lineNumber)
    {
        Label = label;
        Condition = condition;
        LineNumber = lineNumber;
    }
}

// Nodo de asignacion de variables
public class VariableAssignmentNode : AstNode
{
    public string VariableName { get; }
    public ExpressionNode Expression { get; }
    
    public VariableAssignmentNode(string variableName, ExpressionNode expression, int lineNumber)
    {
        VariableName = variableName;
        Expression = expression;
        LineNumber = lineNumber;
    }
}

// Nodos de Expresiones
public abstract class ExpressionNode : AstNode { }

public class NumberLiteralNode : ExpressionNode
{
    public int Value { get; }
    
    public NumberLiteralNode(int value, int lineNumber)
    {
        Value = value;
        LineNumber = lineNumber;
    }
}

public class StringLiteralNode : ExpressionNode
{
    public string Value { get; }
    
    public StringLiteralNode(string value, int lineNumber)
    {
        Value = value;
        LineNumber = lineNumber;
    }
}

public class BooleanLiteralNode : ExpressionNode
{
    public bool Value { get; }
    
    public BooleanLiteralNode(bool value, int lineNumber)
    {
        Value = value;
        LineNumber = lineNumber;
    }
}

public class VariableReferenceNode : ExpressionNode
{
    public string VariableName { get; }
    
    public VariableReferenceNode(string variableName, int lineNumber)
    {
        VariableName = variableName;
        LineNumber = lineNumber;
    }
}

public class BinaryOperationNode : ExpressionNode
{
    public ExpressionNode Left { get; }
    public TokenType Operator { get; }
    public ExpressionNode Right { get; }
    
    public BinaryOperationNode(ExpressionNode left, TokenType op, ExpressionNode right, int lineNumber)
    {
        Left = left;
        Operator = op;
        Right = right;
        LineNumber = lineNumber;
    }
}

public class FunctionCallNode : ExpressionNode
{
    public FunctionNode Function { get; }
    
    public FunctionCallNode(FunctionNode function, int lineNumber)
    {
        Function = function;
        LineNumber = lineNumber;
    }
}

// Nodo para declaraciones de expresión
public class ExpressionStatementNode : AstNode
{
    public ExpressionNode Expression { get; }
    
    public ExpressionStatementNode(ExpressionNode expression, int lineNumber)
    {
        Expression = expression;
        LineNumber = lineNumber;
    }
}