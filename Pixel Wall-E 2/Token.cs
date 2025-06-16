namespace Pixel_Wall_E_2;
public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public int LineNumber { get; }

    public Token(TokenType type, string value, int lineNumber)
    {
        Type = type;
        Value = value;
        LineNumber = lineNumber;
    }

    public override string ToString()
    {
        return $"Token({Type}, '{Value}', Line: {LineNumber})";
    }
}

public enum TokenType
{
    // Palabras reservadas (instrucciones)
    Spawn, Color, Size, DrawLine, DrawCircle, DrawRectangle, Fill,
    
    // Palabras reservadas (funciones)
    GetActualX, GetActualY, GetCanvasSize, GetColorCount, 
    IsBrushColor, IsBrushSize, IsCanvasColor,
    
    // Palabras reservadas (control de flujo)
    GoTo,
    
    // Tipos de datos
    Number, String, Boolean,
    
    // Identificadores
    Identifier, // Para nombres de variables y etiquetas
    Label,      // Específico para etiquetas de GoTo
    
    // Símbolos
    LeftParenthesis, RightParenthesis, // ( )
    LeftBracket, RightBracket,         // [ ]
    Comma,                             // ,
    Arrow,                             // ← (o -> dependiendo de implementación)
    Assignment,                        // =
    
    // Operadores aritméticos
    Plus,       // +
    Minus,      // -
    Multiply,   // *
    Divide,     // /
    Power,      // **
    Modulo,     // %
    
    // Operadores lógicos y de comparación
    And,                // &&
    Or,                 // ||
    Equal,              // ==
    NotEqual,           // !=
    Greater,            // >
    GreaterOrEqual,     // >=
    Less,               // <
    LessOrEqual,        // <=
    
    
    // Fin de línea
    NewLine,
    
    // Fin de archivo
    EOF,
    
    // Errores (para manejo de tokens inválidos)
    Invalid
}