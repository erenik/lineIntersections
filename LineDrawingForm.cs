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
using System.Linq;

public class LineDrawingForm : Form
{
	private Panel _drawingPanel;

	private Bitmap _drawingImage;
	private Graphics _imageGraphics;


	private Label _numberOfLinesLabel;
	private Label _numberOfLineSegments;
	private Label _intersectionCalculationTimeLabel;
	private Button _clearButton;

	// Pens for drawing the lines.
	private Pen _linesPen = new Pen(Color.White, 2);
	private Pen _hypotheticalPen = new Pen(Color.Red, 3);

	private List<Line> _lines = new List<Line>();

	private Line _currentLine = new Line();
	private Point _startPosition;
	private Boolean _lineDrawingStarted = false;
	private Boolean _linesColliding = false;

	[STAThread]
	static void Main()
	{
		Application.Run(new LineDrawingForm());
	}

	public LineDrawingForm()
	{
		_drawingPanel = new Panel();
		_numberOfLinesLabel = new Label();
		_numberOfLineSegments = new Label();
		_intersectionCalculationTimeLabel = new Label();
		_clearButton = new Button();

		// Set up labels
		_numberOfLinesLabel.Location = new Point(24, 504);
		_numberOfLinesLabel.Size = new Size(100, 23);

		_numberOfLineSegments.Location = new Point(124, 504);
		_numberOfLineSegments.Size = new Size(200, 23);

		UpdateNumberOfLinesAndSegmentsLabels();

		_intersectionCalculationTimeLabel.AutoSize = true;
		_intersectionCalculationTimeLabel.Location = new Point(324, 504);
		_intersectionCalculationTimeLabel.Size = new Size(35, 13);

		_drawingImage = new Bitmap(664, 460);
		_imageGraphics = Graphics.FromImage(_drawingImage);

		// Drawing panel
		_drawingPanel.Anchor = ((AnchorStyles.Top | AnchorStyles.Left)
			| AnchorStyles.Right);
		_drawingPanel.BackColor = SystemColors.ControlDark;
		_drawingPanel.Location = new Point(16, 16);
		_drawingPanel.Size = new Size(664, 460);
		_drawingPanel.MouseUp += new MouseEventHandler(OnDrawingPanelMouseUp);
		_drawingPanel.Paint += new PaintEventHandler(DrawingPanelPaint);
		_drawingPanel.MouseMove += new MouseEventHandler(OnDrawingPanelMouseMove);

		// Clear Button
		_clearButton.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
		_clearButton.Location = new Point(592, 504);
		_clearButton.TabIndex = 1;
		_clearButton.Text = "Clear";
		_clearButton.Click += new EventHandler(OnClearButtonClick);

		// Set up how the form should be displayed and add the controls to the form.
		this.ClientSize = new Size(696, 534);
		this.Controls.AddRange(
			new Control[] {
				_drawingPanel,
				_numberOfLinesLabel,
				_numberOfLineSegments,
				_intersectionCalculationTimeLabel,
				_clearButton,
			}
		);

		this.Text = "Line drawer - no intersections! ";
	}

	private void UpdateNumberOfLinesAndSegmentsLabels()
	{
		_numberOfLinesLabel.Text = "# of lines: " + _lines.Count;
		_numberOfLineSegments.Text = "# of line segments: "
		 + _lines.Select(line => line.LineSegments.Count)
		 		 .Sum();
	}

	private Boolean LineSegmentIntersectsOtherLines(LineSegment newLineSegment)
	{
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		TimeSpan stopwatchElapsed;

		foreach (Line line in _lines)
		{
			if (line.Intersects(newLineSegment))
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

	private void OnDrawingPanelMouseMove(object sender, MouseEventArgs mouseEventArgs)
	{
		if (_lineDrawingStarted)
		{
			// Draw the hypothetical line
			Point mousePosition = new Point(mouseEventArgs.X, mouseEventArgs.Y);
			LineSegment newSegment = new LineSegment(_startPosition, mousePosition);
			_currentLine.ExtendLine(newSegment);

			if (LineSegmentIntersectsOtherLines(newSegment))
			{
				_linesColliding = true;
			}

			// Update position for next frame.
			_startPosition = mousePosition;
			_drawingPanel.Invalidate();
		}
	}

	private void OnDrawingPanelMouseUp(object sender, MouseEventArgs mouseEventArgs)
	{
		// Start drawing on first click.
		if (!_lineDrawingStarted)
		{
			Point mouseDownLocation = new Point(mouseEventArgs.X, mouseEventArgs.Y);
			_startPosition = mouseDownLocation;
			_currentLine = new Line();
			_lineDrawingStarted = true;
		}

		// Stop on second.
		else if (_lineDrawingStarted)
		{
			_lineDrawingStarted = false;

			// Skip the new line if it was intersecting others
			if (_linesColliding)
			{
				_currentLine = new Line();
				_linesColliding = false;
				_drawingPanel.Invalidate();
				return;
			}

			// Otherwise add it to the list and update the graphics.
			_lines.Add(_currentLine);
			foreach (LineSegment segment in _currentLine.LineSegments)
			{
				_imageGraphics.DrawLine(_linesPen, segment.Start, segment.Stop);
			}

			UpdateNumberOfLinesAndSegmentsLabels();

			_drawingPanel.Invalidate();
			_linesColliding = false;
		}
	}

	private void DrawingPanelPaint(object sender, PaintEventArgs paintEventArgs)
	{
		paintEventArgs.Graphics.DrawImage(_drawingImage, new Point(0, 0));

		if (_lineDrawingStarted)
		{
			if (_linesColliding)
				_hypotheticalPen.Color = Color.Red;
			else
				_hypotheticalPen.Color = Color.Green;

			foreach (LineSegment segment in _currentLine.LineSegments)
			{
				paintEventArgs.Graphics.DrawLine(_hypotheticalPen, segment.Start, segment.Stop);
			}
		}
	}

	private void OnClearButtonClick(object sender, EventArgs eventArgs)
	{
		// Clear the Panel display.
		_lines.Clear();
		_currentLine = new Line();
		_lineDrawingStarted = false;
		UpdateNumberOfLinesAndSegmentsLabels();
		_imageGraphics.Clear(Color.Gray);
		_drawingPanel.Invalidate();
	}
}