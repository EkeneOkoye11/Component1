using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using static Component1.Form1;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing.Text;
using System.CodeDom;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;


namespace Component1
{
    /// <summary>
    /// Represents the main form of the application for drawing shapes
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Confirm if the circle has been drawn
        /// </summary>
        private bool circleDrawn = false;
        /// <summary>
        /// Represents the diameter of the circle drawn
        /// </summary>
        private int circleDiameter = 0; // Assuming an initial value, adjust as needed
        /// <summary>
        /// Stores the content of the last displayed messagebox
        /// </summary>
        private string lastMessageBoxContent;
        /// <summary>
        /// The action which happens when a button is clicked
        /// </summary>



        public Action buttonClickAction;
        /// <summary>
        /// Event handler for the paint event of the picture box
        /// </summary>
        private PaintEventHandler pictureBox1_Paint;
        /// <summary>
        /// Handles syntax checking for commands
        /// </summary>
        private SyntaxChecker syntaxChecker; // Add a syntax checker
        /// <summary>
        /// Represents the menu strip for the form
        /// </summary>
        private MenuStrip menuStrip1;
        /// <summary>
        /// Represents the pen used for drawing shapes
        /// </summary>
        private Pen pen; //declare the pen
        /// <summary>
        /// Represents a black pen used for drawing
        /// </summary>
        private Pen blackPen; // set pen color to black
        /// <summary>
        /// Represents bitmap used to store the drawing
        /// </summary>
        private Bitmap drawingArea; //bitmap to store the drawing
        /// <summary>
        /// Represents the bitmap 
        /// </summary>
        private Point penPosition; //Variable to store the current position
        /// <summary>
        /// Represnts the ToolStripMenuItem for loading drawings
        /// </summary>
        private ToolStripMenuItem LoadToolStripMenuItem;
        /// <summary>
        /// Represents the ToolStripMenuItem for aving drwings
        /// </summary>
        private ToolStripMenuItem SaveToolStripMenuItem;
        /// <summary>
        /// Indicates if shapes should be filled men drawn
        /// </summary>

        private ToolStripMenuItem loadToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private bool fillShapes;
        /// <summary>
        /// Loads drawing related settings from file
        /// </summary>
        /// <param name="fileName"></param>
        //private Graphics graphics;
        //private bool errorDisplayed;

        private void LoadFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] parts = line.Split(' ');
                        if (parts[0] == "penPosition" && parts.Length == 3 &&
                            int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                        {
                            penPosition = new Point(x, y);
                        }
                        else if (parts[0] == "penColor" && parts.Length == 2)
                        {
                            pen.Color = Color.FromName(parts[1]);
                        }
                        else if (parts[0] == "FillShapes" && parts.Length == 2 && bool.TryParse(parts[1], out bool fill))
                        {
                            fillShapes = fill;
                        }
                    }
                }

            }


        }
        /// <summary>
        /// Saves drawing related settings a file
        /// </summary>
        /// <param name="fileName"></param>
        private void SaveToFile(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                //save current state of program
                sw.WriteLine($"PenPosition {penPosition.X} {penPosition.Y}");
                sw.WriteLine($"PenColor {pen.Color.Name}");
                sw.WriteLine($"FillShapes {fillShapes}");
            }
        }
        /// <summary>
        /// Handles click for load, which allows the loading for drawing settings from a file
        /// </summary>
        /// <param name="sender">The object that triggered the event</param>
        /// <param name="e">an EventArgs object which contains event data</param>
        private void LoadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadFromFile(openFileDialog.FileName);
                // Refresh the PictureBox
                pictureBox1.Refresh();
            }
        }
        /// <summary>
        /// Handles click for save, which allows the saving of drawing settings to a file
        /// </summary>
        /// <param name="sender">The object that triggered the event</param>
        /// <param name="e">an EventArgs object which contains event data</param>
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveToFile(saveFileDialog.FileName);
            }
        }
        /// <summary>
        /// Executes a drawing command based on the string command typed in
        /// </summary>
        /// <param name="command">The input command to be executed</param>
        /// <remarks>
        /// The method passes the input command, checks its syntax and executes the corresponding drawing opertion
        /// Supported commands:
        /// "moveTo x y": Moves the pen to the specified coordinates (x, y)
        /// "drawTo x, y": Draws a line frm the current pen position to the specified coordinates(x, y)
        /// "clear": Clears the graphic area
        /// "reset": Resets the pen position
        /// "Circle radius": Draws a circle with the specified radius.
        /// Rectangle width height": Draws a rectangle with the specified width and height
        /// "Triangle side1 side2": Draws a triangle with the specified side lengths
        /// "blue": Sets the pen colour to blue
        /// "green": Sets the pen color to green
        /// "red" Sets the pen color to red.
        /// </remarks>

        //public class CommandExecutor
        //{
        // private SyntaxChecker syntaxChecker;

        //public CommandExecutor()
        // {
        //     syntaxChecker = new SyntaxChecker();
        //}

        // public object buttonClickAction { get; private set; }

        public void ExecuteCommand(string command)
        {
            try
            {
                syntaxChecker.CheckSyntax(command); //check the syntax of the command
                string[] input = command.Split(' ');
                buttonClickAction?.Invoke();
                // If the command is executed successfully, reset the error message
                ShowMessageBox(""); // Empty string or null to reset the error message

                switch (input[0])
                {
                    case "moveTo":
                        if (input[0].StartsWith("moveTo") && input.Length == 3 && int.TryParse(input[1], out int x) && int.TryParse(input[2], out int y))
                        {
                            penPosition = new Point(x, y);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid syntax for moveTo command.");
                        }
                        break;
                    //default:
                    //throw new ArgumentException("Invalid syntax for moveTo command.");



                    case "drawTo":
                        if (input.Length == 3 && int.TryParse(input[1], out int x2) && int.TryParse(input[2], out int y2))
                        {
                            DrawToPosition(x2, y2);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid syntax for drawTo command.");
                        }
                        break;

                    case "clear":
                        ClearGraphicsArea();
                        break;
                    case "reset":
                        ResetPenPosition();
                        break;

                    case "Circle":
                        if (input[0] == "Circle" && input.Length == 2 && int.TryParse(input[1], out int radius))
                        {
                            DrawCircle(radius);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid syntax for Circle command.");
                        }
                        break;

                    case "Rectangle":
                        if (input[0] == "Rectangle" && input.Length == 3 && int.TryParse(input[1], out int width) && int.TryParse(input[2], out int height))
                        {
                            DrawRectangle(width, height);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid syntax for Rectangleo command.");
                        }
                        break;

                    case "Triangle":
                        if (input[0] == "Triangle" && input.Length == 3 && int.TryParse(input[1], out int side1) && int.TryParse(input[2], out int side2))
                        {
                            DrawTriangle(side1, side2);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid syntax for drawTo command.");
                        }
                        break;

                    case "blue":
                        SetPenColor(Color.Blue);
                        break;

                    case "green":
                        SetPenColor(Color.Green);
                        break;

                    case "red":
                        SetPenColor(Color.Red);
                        break;
                }

            }

            catch (ArgumentException ex)
            {
                // If there's an exception, set the error message
                ShowMessageBox(ex.Message);
                //MessageBox.Show(ex.Message, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //throw ex;
            }
        }
        /// <summary>
        /// Handles the keypress event for textbook1, focusing on the enter key
        /// </summary>
        /// <param name="sender">The object that triggerd the event</param>
        /// <param name="e">A keyPressEventArgs object containing event data</param>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string command = textBox1.Text;
                ExecuteCommand(command);
                button1.PerformClick();
                e.Handled = true;
            }
        }
        /// <summary>
        /// Initialising a new instance of the <see cref="Form1"/>
        /// </summary>
        /// </remarks 
        /// This constructor sets up the initial configuration for the Form1 instance, including initializing components, attaching event handlers,
        /// setting up the drawing area, configuring the menu strip, and intialising the syntax checker.
        /// </remarks>

        // private void ExecuteCommand(string command)
        // {
        //    throw new NotImplementedException();
        // }

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            blackPen = new Pen(Color.Black);
            pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBox1_Paint);
            button1.Click += new System.EventHandler(this.button1_Click);
            penPosition = new Point(10, 10); //the initial position of the pen
            pen = blackPen;// the initial color of the pen
            textBox1.KeyPress += new KeyPressEventHandler(textBox1_KeyPress);
            loadToolStripMenuItem = new ToolStripMenuItem("Load");
            saveToolStripMenuItem = new ToolStripMenuItem("Save");
            loadToolStripMenuItem.Click += new System.EventHandler(this.LoadToolStripMenuItem_Click);
            saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            menuStrip1 = new MenuStrip();
            this.MainMenuStrip = menuStrip1;
            this.Controls.Add(menuStrip1);
            menuStrip1.Items.Add(loadToolStripMenuItem);
            menuStrip1.Items.Add(saveToolStripMenuItem);
            this.Controls.Add(this.pictureBox1);
            //  graphics = Graphics.FromImage(drawingArea);
            drawingArea = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            graphics = pictureBox1.CreateGraphics();
            syntaxChecker = new SyntaxChecker();
            button2.Click += new System.EventHandler(this.Syntax_Click);
            button1.Click += new System.EventHandler(this.button1_Click);
            /// <summary>
            /// Handles the click event of the syntax button, checking the syntax command in textBox1
            /// </summary>
            /// </param name="sender">The object that triggered the event</param>
            /// <param name="e">an EventArgs object containing the event data</param>
        }
        private void Syntax_Click(object sender, EventArgs e)
        {
            try
            {
                string command = textBox1.Text;
                syntaxChecker.CheckSyntax(command);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            /// <summary>
            /// Handles the valid commands in the textBox.
            /// </summary>
        }

        public class SyntaxChecker
        {
            private HashSet<string> validCommands; //list of valid commands
            public SyntaxChecker()
            {
                validCommands = new HashSet<string>
            {
                "moveTo",
                "drawTo",
                "Rectangle",
                "Circle",
                "clear",
                "Triangle",
                "green",
                "reset",
                "red",
                "blue",
                "fill"
            };
            }
            /// <summary>
            /// Handles the syntax of the provided drawing command
            /// </summary>
            /// <param name="command"></param>
            /// <exception cref="ArgumentException"></exception>
            public void CheckSyntax(string command)
            {
                string[] input = command.Split(' ');
                if (input.Length == 0)
                {
                    throw new ArgumentException("Empty command. Please enter a valid command.");
                }
                if (!validCommands.Contains(input[0]))
                {
                    throw new ArgumentException("Invalid command. Please enter a valid command.");
                }
                switch (input[0])
                {
                    case "drawTo":
                        if (input.Length != 3 || !int.TryParse(input[1], out _) || !int.TryParse(input[2], out _))
                        {
                            throw new ArgumentException("Invalid syntax for drawTo command.");
                        }
                        break;

                    case "moveTo":
                        if (input.Length != 3 || !int.TryParse(input[1], out _) || !int.TryParse(input[2], out _))
                        {
                            throw new ArgumentException("Invalid syntax for moveTo command.");
                        }
                        break;
                }
            }

        }
        /// <summary>
        /// Handles the Paint event for the PictureBox, drawing a small square at the top-left corner based on the current position.
        /// </summary>
        /// <param name="sender">The object that triggered the event</param>
        /// <param name="e">A PaintEventArgs object containing event data</param>
        /// </remarks>
        /// This method uses the Graphics object from the PaintEventArgs to draw a small black square at the current pen position 
        /// The square has a size of 5x5 pixels.

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //A small square at the top left corder of the picturebox
            e.Graphics.FillRectangle(Brushes.Black, penPosition.X, penPosition.Y, 5, 5);
        }
        /// <summary>
        /// The Graphics object used for drawing on the PictureBox.
        /// </summary>
   
        //private bool fillShapes = false;
        private Graphics graphics;
        /// <summary>
        /// Indicates if the error message has been thrown
        /// </summary>

        private bool errorDisplayed = false;
        /// <summary>
        /// Event handler for the Form1_Load event
        /// </summary>
        private EventHandler Form1_Load;
        /// <summary>
        /// Handles the click event for the button1 control
        /// Calls the <see cref="DoSave"/> method to process and save the drawing command.
        /// </summary>
        /// <param name="sender">The object that triggered the event</param>
        /// <param name="e">An EvenArgs object containing event data</param>
        




        private void button1_Click(object sender, EventArgs e)
        {
            DoSave();
        }
        public void DoSave()
        {

            //{
            {
                try
                {
                    string command = textBox1.Text;
                    syntaxChecker.CheckSyntax(command);

                    if (pictureBox1.Image == null)
                    {
                        pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    }

                    graphics = Graphics.FromImage(pictureBox1.Image);

                    string[] input = textBox1.Text.Split(' ');
                    if (input[0] == "Circle" && input.Length == 2 && int.TryParse(input[1], out int radius))
                    {
                        DrawCircle(radius);
                    }
                    else if (input[0] == "Rectangle" && input.Length == 3 && int.TryParse(input[1], out int width) && int.TryParse(input[2], out int height))
                    {
                        DrawRectangle(width, height);
                    }

                    else if (input[0] == "Triangle" && input.Length == 3 && int.TryParse(input[1], out int side1) && int.TryParse(input[2], out int side2))
                    {
                        DrawTriangle(side1, side2);
                    }
                    else if (input[0] == "clear")
                    {
                        ClearGraphicsArea();
                    }
                    else if (input[0] == "reset")
                    {
                        ResetPenPosition();
                    }
                    else if (input[0].StartsWith("moveTo") && input.Length == 3 && int.TryParse(input[1], out int x) && int.TryParse(input[2], out int y))
                    {
                        MoveToPosition(x, y);
                    }
                    else if (input[0].StartsWith("drawTo") && input.Length == 3 && int.TryParse(input[1], out int x2) && int.TryParse(input[2], out int y2))
                    {
                        DrawToPosition(x2, y2);
                    }
                    else if (input[0] == "red")
                    {
                        SetPenColor(Color.Red);
                    }
                    else if (input[0] == "green")
                    {
                        SetPenColor(Color.Green);
                    }
                    else if (input[0] == "blue")
                    {
                        SetPenColor(Color.Blue);
                    }
                    else if (input[0] == "fill" && input.Length == 2)
                    {
                        SetFillShapes(input[1]);
                    }
                    pictureBox1.Refresh();
                    errorDisplayed = false;
                }

                catch (ArgumentException ex)
                {

                    HandleArgumentException(ex);

                }

                //}
            }
        }
        /// <summary>
        /// Draws a circle
        /// </summary>
        /// <param name="radius"></param>
        public void DrawCircle(int radius)
        {
            try
            {
                if (fillShapes)
                {
                    graphics.FillEllipse(pen.Brush, 10, 10, radius * 2, radius * 2);
                }
                else
                {
                    graphics.DrawEllipse(pen, 10, 10, radius * 2, radius * 2);
                }
                circleDrawn = true;
                circleDiameter = radius * 2;
                MessageBox.Show("Circle drawn successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
        /// <summary>
        /// checks if circle is drawn
        /// </summary>
        /// <returns></returns> if a circle has been drawn.
        public bool IsCircleDrawn()
        {
            return circleDrawn;
        }
        /// <summary>
        /// Gets the diameter of the circle drawn
        /// </summary>
        /// <returns></returns>
        public int GetCircleDiameter()
        {
            return circleDiameter;
        }
        /// <summary>
        /// Draws a rectangle
        /// </summary>
        /// <param name="width">width of rectangle drawn</param>
        /// <param name="height">height of rectangle </param>

        // Other methods and properties in your Form1 class


        private void DrawRectangle(int width, int height)
        {
            if (fillShapes)
            {
                graphics.FillRectangle(pen.Brush, 10, 10, width, height);

            }
            else
            {
                graphics.DrawRectangle(pen, 10, 10, width, height);
            }
        }
        /// <summary>
        /// Draws a Triangle
        /// </summary>
        /// <param name="side1">side 1 of the triangle</param>
        /// <param name="side2">side 2 of the triangle</param>

        private void DrawTriangle(int side1, int side2)
        {
            if (fillShapes)
            {
                Point point1 = new Point(10, 10 + side2);
                Point point2 = new Point(10 + side1, 10 + side2);
                Point point3 = new Point(10 + side1 / 2, 10);

                Point[] trianglePoints = { point1, point2, point3 };

                graphics.FillPolygon(pen.Brush, trianglePoints);
            }
            else
            {
                Point point1 = new Point(10, 10 + side2);
                Point point2 = new Point(10 + side1, 10 + side2);
                Point point3 = new Point(10 + side1 / 2, 10);

                Point[] trianglePoints = { point1, point2, point3 };

                graphics.DrawPolygon(pen, trianglePoints);
            }
        }
        /// <summary>
        /// clears the graphic area
        /// </summary>
        private void ClearGraphicsArea()
        {
            //Clear the graphics area
            graphics.Clear(Color.Transparent);
        }
        /// <summary>
        /// resets the initial penposition
        /// </summary>
        private void ResetPenPosition()
        {
            penPosition = new Point(10, 10);
        }
        /// <summary>
        /// Moves pen position
        /// </summary>
        /// <param name="x">position x</param>
        /// <param name="y">position y</param>
        private void MoveToPosition(int x, int y)
        {
            penPosition = new Point(x, y);
        }
        /// <summary>
        /// drawTo penPosition
        /// </summary>
        /// <param name="x2">position x2</param>
        /// <param name="y2">position y2</param>
        private void DrawToPosition(int x2, int y2)
        {
            Point endPoint = new Point(x2, y2);
            graphics.DrawLine(pen, penPosition, endPoint);
            penPosition = endPoint; // update the pen position
        }
        /// <summary>
        /// sets the pen color
        /// </summary>
        /// <param name="color">the color pen will be set to</param>
        private void SetPenColor(Color color)
        {
            pen.Color = color;
        }
        /// <summary>
        /// sets fill on or off
        /// </summary>
        /// <param name="input">input to set fill</param>
        private void SetFillShapes(string input)
        {
            if (input == "on")
            {
                fillShapes = true;
            }
            else if (input == "off")
            {
                fillShapes = false;
            }
        }
        /// <summary>
        /// Handles an argument exception by displaying an error message in the message box if it was not displayed initially
        /// </summary>
        /// <param name="ex">The ArgumentException to handle</param>
        private void HandleArgumentException(ArgumentException ex)
        {
            if (!errorDisplayed)
            {
                MessageBox.Show(ex.Message, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                errorDisplayed = true;
            }
        }
        // pictureBox1.Refresh();
        // errorDisplayed = false;
        // }

        //catch (ArgumentException ex)
        //{


        //}
        //}



        /// <summary>
        /// Handles the textchanged event for the richTextBox1 control
        /// </summary>
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Gets the content of the last displayed messagebox
        /// </summary>
        /// <returns>String for the last displayed messageBox</returns>
        public string GetLastMessageBoxContent()
        {
            return lastMessageBoxContent;
        }
        /// <summary>
        /// displays messagebox
        /// </summary>
        /// <param name="message">Simulate showing a message</param>
        public void ShowMessageBox(string message)
        {
            // Simulate showing a message box and store the content
            lastMessageBoxContent = message;
        }
        /// <summary>
        /// Get the current penPosition on the drawing surface
        /// </summary>
        /// <returns>the point representing the current position</returns>
        public Point GetPenPosition()
        {
            return penPosition;
        }
        /// <summary>
        /// Sets the pencolor on the drawing surface
        /// </summary>
        /// <returns>the color of the pen</returns>
        public Color GetPenColor()
        {
            return pen.Color;
        }

        /// <summary>
        /// Retrieves the current view shapes status
        /// </summary>
        /// <returns>tre if fillshapes are enabled; else, false</returns>
        public bool GetFillShapes()
        {
            return fillShapes;
        }



    }
}
