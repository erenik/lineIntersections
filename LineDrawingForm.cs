/**
	Emil Hedemalm
	2021-02-26
	Small C# program where you draw lines where lines may not intersect.
*/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;

public class LineDrawingForm : System.Windows.Forms.Form
{
	private System.Windows.Forms.Panel _drawingPanel;

	private Bitmap _drawingImage;
	private Graphics _imageGraphics;


	private System.Windows.Forms.Label _numberOfLinesLabel;
	private System.Windows.Forms.Label _intersectionCalculationTimeLabel;
	private System.Windows.Forms.Button _clearButton;
	private System.Windows.Forms.Button _generateLinesButton;

	// Pens for drawing the lines.
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
		_drawingPanel = new System.Windows.Forms.Panel();
		_numberOfLinesLabel = new System.Windows.Forms.Label();
		_intersectionCalculationTimeLabel = new System.Windows.Forms.Label();
		_clearButton = new System.Windows.Forms.Button();
		_generateLinesButton = new System.Windows.Forms.Button();

		// Set up labels
		_numberOfLinesLabel.Location = new System.Drawing.Point(24, 504);
		_numberOfLinesLabel.Size = new System.Drawing.Size(392, 23);
		UpdateNumberOfLinesLabel();

		_intersectionCalculationTimeLabel.AutoSize = true;
		_intersectionCalculationTimeLabel.Location = new System.Drawing.Point(124, 504);
		_intersectionCalculationTimeLabel.Size = new System.Drawing.Size(35, 13);

		_drawingImage = new Bitmap(664, 460);
		_imageGraphics = Graphics.FromImage(_drawingImage);

		// Drawing panel
		_drawingPanel.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
		_drawingPanel.BackColor = System.Drawing.SystemColors.ControlDark;
		_drawingPanel.Location = new System.Drawing.Point(16, 16);
		_drawingPanel.Size = new System.Drawing.Size(664, 460);
		_drawingPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(OnDrawingPanelMouseUp);
		_drawingPanel.Paint += new System.Windows.Forms.PaintEventHandler(DrawingPanelPaint);
		_drawingPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(OnDrawingPanelMouseMove);
		_drawingPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(OnDrawingPanelMouseDown);

		// Clear Button
		_clearButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
		_clearButton.Location = new System.Drawing.Point(592, 504);
		_clearButton.TabIndex = 1;
		_clearButton.Text = "Clear";
		_clearButton.Click += new System.EventHandler(OnClearButtonClick);

		_generateLinesButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
		_generateLinesButton.Location = new System.Drawing.Point(482, 504);
		_generateLinesButton.Size = new System.Drawing.Size(100, _generateLinesButton.Size.Height);
		_generateLinesButton.TabIndex = 2;
		_generateLinesButton.Text = "Generate lines";
		_generateLinesButton.Click += new System.EventHandler(OnGenerateLinesClick);

		// Set up how the form should be displayed and add the controls to the form.
		this.ClientSize = new System.Drawing.Size(696, 534);
		this.Controls.AddRange(
			new System.Windows.Forms.Control[] {
				_intersectionCalculationTimeLabel,
				_generateLinesButton,
				_clearButton,
				_drawingPanel,
				_numberOfLinesLabel
			}
		);

		this.Text = "Line drawer - no intersections! ";
	}

	private void UpdateNumberOfLinesLabel()
	{
		_numberOfLinesLabel.Text = "# of lines: " + _lines.Count;
	}

	private Boolean LineIntersectsOthers(Line newLine)
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

	private void UpdateIntersectionCalcTimeLabel(TimeSpan withTimeSpan)
	{
		float micros = withTimeSpan.Ticks / (TimeSpan.TicksPerMillisecond / 1000);
		_intersectionCalculationTimeLabel.Text = "Intersection calc time: " + micros + " microseconds";
	}

	private void OnDrawingPanelMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		if (!_lineDrawingStarted)
		{
			_lineDrawingStarted = true;
			Point mouseDownLocation = new Point(e.X, e.Y);
			_currentLine.Start = mouseDownLocation;
			_currentLine.Stop = mouseDownLocation;
			_drawingPanel.Focus();
			_drawingPanel.Invalidate();
		}
	}

	private void OnDrawingPanelMouseMove(object sender, System.Windows.Forms.MouseEventArgs mouseEventArgs)
	{
		if (_lineDrawingStarted)
		{
			// Draw the hypothetical line
			_currentLine.Stop = new Point(mouseEventArgs.X, mouseEventArgs.Y);
			_drawingPanel.Invalidate();
		}
	}

	private void OnDrawingPanelMouseUp(object sender, System.Windows.Forms.MouseEventArgs mouseEventArgs)
	{
		// Draw line if clicked and dragged far enough or on 2nd click.
		if (_lineDrawingStarted)
		{
			_currentLine.Stop = new Point(mouseEventArgs.X, mouseEventArgs.Y); ;

			// Skip short lines as they are probably accidental.
			if (_currentLine.Length() < 5)
				return;

			_lineDrawingStarted = false;

			// Skip lines intersecting others (the red lines)
			if (LineIntersectsOthers(_currentLine))
			{
				_drawingPanel.Invalidate();
				return;
			}

			_lines.Add(_currentLine);

			// Update image of lines by adding the current line to it.
			_imageGraphics.DrawLine(_linesPen, _currentLine.Start, _currentLine.Stop);

			UpdateNumberOfLinesLabel();

			_drawingPanel.Invalidate();
		}
	}

	private void DrawingPanelPaint(object sender, System.Windows.Forms.PaintEventArgs paintEventArgs)
	{
		paintEventArgs.Graphics.DrawImage(_drawingImage, new Point(0, 0));

		if (_lineDrawingStarted)
		{
			if (LineIntersectsOthers(_currentLine))
				_hypotheticalPen.Color = Color.Red;
			else
				_hypotheticalPen.Color = Color.Green;
			paintEventArgs.Graphics.DrawLine(_hypotheticalPen, _currentLine.Start, _currentLine.Stop);
		}
	}

	private void OnClearButtonClick(object sender, System.EventArgs eventArgs)
	{
		// Clear the Panel display.
		_lines.Clear();
		_lineDrawingStarted = false;
		UpdateNumberOfLinesLabel();
		_imageGraphics.Clear(Color.Gray);
		_drawingPanel.Invalidate();
	}

	// Attempts to generate random lines to perf test the line-intersection checks
	private void OnGenerateLinesClick(object sender, System.EventArgs eventArgs)
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

				// Update image of lines.
				_imageGraphics.DrawLine(_linesPen, line.Start, line.Stop);
			}
		}
		UpdateNumberOfLinesLabel();
		_drawingPanel.Invalidate();
	}
}