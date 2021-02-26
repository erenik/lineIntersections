using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;

public class LineDrawingForm : System.Windows.Forms.Form
{
	private System.Windows.Forms.Panel _drawingPanel;
	private System.Windows.Forms.Label _numberOfLinesLabel;
	private System.Windows.Forms.Label _intersectionCalculationTimeLabel;
	private System.Windows.Forms.Button _clearButton;
	private System.Windows.Forms.Button _generateLinesButton;

	// Pens for drawing the _lines.
	private Pen _linesPen = new Pen(Color.White, 2);
	private Pen _hypotheticalPen = new Pen(Color.Red, 3);

	private List<Line> _lines = new List<Line>();

	private Line _currentLine;
	private Boolean _lineDrawingStarted = false;

	[STAThread]
	static void Main()
	{
		Application.Run(new LineDrawingForm());
	}

	public LineDrawingForm()
	{
		this._drawingPanel = new System.Windows.Forms.Panel();
		this._numberOfLinesLabel = new System.Windows.Forms.Label();
		this._intersectionCalculationTimeLabel = new System.Windows.Forms.Label();
		this._clearButton = new System.Windows.Forms.Button();
		this._generateLinesButton = new System.Windows.Forms.Button();

		// Set up labels
		this._numberOfLinesLabel.Location = new System.Drawing.Point(24, 504);
		this._numberOfLinesLabel.Size = new System.Drawing.Size(392, 23);
		Update_numberOfLinesLabel();

		this._intersectionCalculationTimeLabel.AutoSize = true;
		this._intersectionCalculationTimeLabel.Location = new System.Drawing.Point(124, 504);
		this._intersectionCalculationTimeLabel.Size = new System.Drawing.Size(35, 13);

		// Drawing panel
		this._drawingPanel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
		this._drawingPanel.BackColor = System.Drawing.SystemColors.ControlDark;
		this._drawingPanel.Location = new System.Drawing.Point(16, 16);
		this._drawingPanel.Size = new System.Drawing.Size(664, 460);
		this._drawingPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this._drawingPanel_MouseUp);
		this._drawingPanel.Paint += new System.Windows.Forms.PaintEventHandler(this._drawingPanel_Paint);
		this._drawingPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this._drawingPanel_MouseMove);
		this._drawingPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this._drawingPanel_MouseDown);

		// Clear Button
		this._clearButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
		this._clearButton.Location = new System.Drawing.Point(592, 504);
		this._clearButton.TabIndex = 1;
		this._clearButton.Text = "Clear";
		this._clearButton.Click += new System.EventHandler(this._clearButton_Click);

		this._generateLinesButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
		this._generateLinesButton.Location = new System.Drawing.Point(482, 504);
		this._generateLinesButton.Size = new System.Drawing.Size(100, this._generateLinesButton.Size.Height);
		this._generateLinesButton.TabIndex = 2;
		this._generateLinesButton.Text = "Generate lines";
		this._generateLinesButton.Click += new System.EventHandler(this._generateLines_Click);

		// Set up how the form should be displayed and add the controls to the form.
		this.ClientSize = new System.Drawing.Size(696, 534);
		this.Controls.AddRange(new System.Windows.Forms.Control[] {
										this._intersectionCalculationTimeLabel, this._generateLinesButton,
										this._clearButton,this._drawingPanel,this._numberOfLinesLabel
			});
		this.Text = "Mouse Event Example";
	}

	void Update_numberOfLinesLabel()
	{
		this._numberOfLinesLabel.Text = "# of lines: " + _lines.Count;
	}

	Boolean LineIntersectsOthers(Line newLine)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		TimeSpan stopwatchElapsed;

		foreach (Line line in _lines)
		{
			if (newLine.Intersects(line) != Intersection.NoIntersection)
			{
				stopwatch.Stop();
				stopwatchElapsed = stopwatch.Elapsed;
				UpdateIntersectionCalcTimeLabel(stopwatchElapsed);
				return true;
			}
		}
		stopwatch.Stop();
		stopwatchElapsed = stopwatch.Elapsed;
		UpdateIntersectionCalcTimeLabel(stopwatchElapsed);
		return false;
	}

	void UpdateIntersectionCalcTimeLabel(TimeSpan withTimeSpan)
	{
		float micros = withTimeSpan.Ticks / (TimeSpan.TicksPerMillisecond / 1000);
		_intersectionCalculationTimeLabel.Text = "Intersection calc time: " + micros + " microseconds";
	}

	private void _drawingPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		if (!_lineDrawingStarted)
		{
			_lineDrawingStarted = true;
			Point mouseDownLocation = new Point(e.X, e.Y);
			_currentLine.start = mouseDownLocation;
			_currentLine.stop = mouseDownLocation;
			_drawingPanel.Focus();
			_drawingPanel.Invalidate();
		}
	}

	private void _drawingPanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		if (_lineDrawingStarted)
		{
			// Update the mouse path that is drawn onto the Panel.
			int mouseX = e.X;
			int mouseY = e.Y;

			// Draw the hypothetical line
			_currentLine.stop = new Point(mouseX, mouseY);
			_drawingPanel.Invalidate();
		}
	}

	private void _drawingPanel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		// Draw line if clicked and dragged far enough or on 2nd click.
		if (_lineDrawingStarted)
		{
			_drawingPanel.Invalidate();

			Point mouseUpLocation = new System.Drawing.Point(e.X, e.Y);
			_currentLine.stop = mouseUpLocation;

			// Skip short _lines as they are probably accidental.
			if (_currentLine.Length() < 5)
				return;

			_lineDrawingStarted = false;

			// Skip _lines intersecting others (the red _lines)
			if (LineIntersectsOthers(_currentLine))
				return;

			_lines.Add(_currentLine);
			Update_numberOfLinesLabel();

			_drawingPanel.Invalidate();
		}
	}

	private void _drawingPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs paintEventArgs)
	{

		// Perform the painting of the Panel.
		foreach (Line line in _lines)
		{
			paintEventArgs.Graphics.DrawLine(_linesPen, line.start, line.stop);
		}

		if (_lineDrawingStarted)
		{
			if (LineIntersectsOthers(_currentLine))
				_hypotheticalPen.Color = Color.Red;
			else
				_hypotheticalPen.Color = Color.Green;
			paintEventArgs.Graphics.DrawLine(_hypotheticalPen, _currentLine.start, _currentLine.stop);
		}
	}

	private void _clearButton_Click(object sender, System.EventArgs e)
	{
		// Clear the Panel display.
		_lines.Clear();
		_lineDrawingStarted = false;
		Update_numberOfLinesLabel();
		_drawingPanel.Invalidate();
	}

	// Attempts to generate 1000 random lines to perf test the line-intersection checks
	private void _generateLines_Click(object sender, System.EventArgs eventArgs)
	{
		Random random = new Random();
		int randomLengthMax = 15;
		for (int i = 0; i < 100; ++i)
		{
			int y = random.Next(_drawingPanel.Size.Height);
			int x = random.Next(_drawingPanel.Size.Width);
			Point start = new Point(x, y);
			Point stop = new Point(x + random.Next(-randomLengthMax, randomLengthMax), y + random.Next(-randomLengthMax, randomLengthMax));
			Line line = new Line(start, stop);
			if (!LineIntersectsOthers(line))
			{
				_lines.Add(line);
			}
		}
		Update_numberOfLinesLabel();
		_drawingPanel.Invalidate();
	}
}