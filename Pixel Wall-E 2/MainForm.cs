using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Pixel_Wall_E_2;

public partial class MainForm : Form
{
    private Bitmap _canvasBitmap;
    private Color[,] _canvas;
    private int _canvasSize = 30;
    private Bitmap _walleBitmap;
    private PictureBox _wallePictureBox;
    private int _walleX;
    private int _walleY;

    public MainForm()
    {
        InitializeComponent();
        InitializeCanvas();
        InitializeWallE();
        textBoxCanvasSize.Text = _canvasSize.ToString();
    }

    private void InitializeCanvas()
    {
        _canvas = new Color[_canvasSize, _canvasSize];
        _canvasBitmap = new Bitmap(pictureBoxCanvas.Width, pictureBoxCanvas.Height);
        ClearCanvas();
    }

    private void InitializeWallE()
    {
        try
        {
            _walleBitmap = new Bitmap("wall-e.png");
            
            _wallePictureBox = new PictureBox
            {
                Image = _walleBitmap,
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                Visible = false
            };

            UpdateWallESize();
            pictureBoxCanvas.Controls.Add(_wallePictureBox);
            _wallePictureBox.BringToFront();
        }
        catch (Exception ex)
        {
            _wallePictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                Visible = false
            };
            
            UpdateWallESize();
            pictureBoxCanvas.Controls.Add(_wallePictureBox);
            _wallePictureBox.BringToFront();
            
            MessageBox.Show($"Error al cargar la imagen de Wall-E: {ex.Message}", 
                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void UpdateWallESize()
    {
        if (_wallePictureBox == null) return;
        
        float pixelSize = (float)pictureBoxCanvas.Width / _canvasSize;
        int size = (int)Math.Ceiling(pixelSize);
        
        _wallePictureBox.Size = new Size(size, size);
    }

    private void UpdateWallEPosition(int x, int y)
    {
        if (_wallePictureBox == null || _walleBitmap == null) return;

        float pixelSize = (float)pictureBoxCanvas.Width / _canvasSize;
        int posX = (int)(x * pixelSize);
        int posY = (int)(y * pixelSize);
        
        _wallePictureBox.Location = new Point(posX, posY);
        _wallePictureBox.Visible = true;
    }

    private void ClearCanvas()
    {
        for (int y = 0; y < _canvasSize; y++)
            for (int x = 0; x < _canvasSize; x++)
                _canvas[x, y] = Color.White;

        if (_wallePictureBox != null)
            _wallePictureBox.Visible = false;
        
        UpdateCanvasDisplay();
    }

    private void UpdateCanvasDisplay()
    {
        using (Graphics g = Graphics.FromImage(_canvasBitmap))
        {
            g.Clear(Color.White);
            float pixelSize = (float)pictureBoxCanvas.Width / _canvasSize;

            // Dibujar píxeles
            for (int y = 0; y < _canvasSize; y++)
            {
                for (int x = 0; x < _canvasSize; x++)
                {
                    using (var brush = new SolidBrush(_canvas[x, y]))
                    {
                        g.FillRectangle(brush, x * pixelSize, y * pixelSize, pixelSize, pixelSize);
                    }
                }
            }

            // Dibujar cuadrícula
            using (var gridPen = new Pen(Color.LightGray, 0.5f))
            {
                for (int i = 0; i <= _canvasSize; i++)
                {
                    g.DrawLine(gridPen, i * pixelSize, 0, i * pixelSize, _canvasSize * pixelSize);
                    g.DrawLine(gridPen, 0, i * pixelSize, _canvasSize * pixelSize, i * pixelSize);
                }
            }
        }

        pictureBoxCanvas.Image = _canvasBitmap;
        UpdateWallESize();
        UpdateWallEPosition(_walleX, _walleY);
    }

    private void btnRun_Click(object sender, EventArgs e)
    {
        string code = textBoxEditor.Text;

        // Lexer
        var lexer = new Lexer(code);
        List<Token> tokens;
        try
        {
            tokens = lexer.Tokenize();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error léxico: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Parser
        var parser = new Parser(tokens);
        List<AstNode> ast;
        try
        {
            ast = parser.Parse();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error de sintaxis: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Interpreter
        var interpreter = new Interpreter(ast, _canvasSize);
        try
        {
            interpreter.Execute();
            
            _canvas = interpreter.Canvas;
            _walleX = interpreter._walleX;
            _walleY = interpreter._walleY;
            
            UpdateCanvasDisplay();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error de ejecución: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        if (interpreter.Errors.Count > 0)
        {
            MessageBox.Show($"Errores de ejecución:\n{string.Join("\n", interpreter.Errors)}",
                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnLoad_Click(object sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = "Archivos Pixel Wall-E (*.pw)|*.pw|Todos los archivos (*.*)|*.*",
            Title = "Abrir archivo Pixel Wall-E"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                textBoxEditor.Text = File.ReadAllText(openFileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el archivo: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "Archivos Pixel Wall-E (*.pw)|*.pw|Todos los archivos (*.*)|*.*",
            Title = "Guardar archivo Pixel Wall-E"
        };

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                File.WriteAllText(saveFileDialog.FileName, textBoxEditor.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el archivo: {ex.Message}",
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void btnResize_Click(object sender, EventArgs e)
    {
        if (int.TryParse(textBoxCanvasSize.Text, out int newSize) && newSize > 0)
        {
            _canvasSize = newSize;
            InitializeCanvas();
            UpdateWallESize();
        }
        else
        {
            MessageBox.Show("Por favor ingrese un tamaño válido para el canvas (número entero positivo).",
                          "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        ClearCanvas();
    }
}