using System;
using System.Drawing;

public enum Intersection
{
	General,
	Collinear,
	PointInSegment,
	NoIntersection
}

struct Line
{
	public Line(Point start, Point stop)
	{
		this.start = start;
		this.stop = stop;
	}

	public Point start, stop;
	public double Length()
	{
		return Math.Sqrt(Math.Pow(stop.X - start.X, 2) + Math.Pow(stop.Y - start.Y, 2));
	}

	float MaxX() { return Math.Max(start.X, stop.X); }
	float MinX() { return Math.Min(start.X, stop.X); }
	float MaxY() { return Math.Max(start.Y, stop.Y); }
	float MinY() { return Math.Min(start.Y, stop.Y); }

	/// Yields an orientation.
	enum Orientation
	{
		Clockwise,
		CounterClockwise,
		Collinear,
	};

	Orientation GetOrientation(Point p1, Point p2, Point p3)
	{
		int val = (p1.X * p2.Y + p2.X * p3.Y + p3.X * p1.Y - p2.X * p1.Y - p3.X * p2.Y - p1.X * p3.Y);
		if (val > 0)
			return Orientation.Clockwise;
		else if (val < 0)
			return Orientation.CounterClockwise;
		return Orientation.Collinear;
	}

	// Given three Collinear points p, q, r, the function checks if
	// point q lies on line segment 'pr'
	Boolean PointOnSegment(Point lineStart, Point lineStop, Point point)
	{
		if (point.X <= Math.Max(lineStart.X, lineStop.X) && point.X >= Math.Max(lineStart.X, lineStop.X) &&
			point.Y <= Math.Max(lineStart.Y, lineStop.Y) && point.Y >= Math.Max(lineStart.Y, lineStop.Y))
			return true;
		return false;
	}

	// Port of intersection test from my game engine, with some additional AABB if-checks for slight perf gain
	// Game engine ref: https://github.com/erenik/engine/blob/master/src/PhysicsLib/Shapes/Line.cpp#L72
	public Intersection Intersects(Line otherLine)
	{
		// Example intersection times without the Min/Max X/Y checks: 162 microseconds for 1460 lines randomly generated short lines. 
		// Adding the below 5 lines of simplified Axis-aligned Bounding-box checks adjusted time spent to around 77 microseconds for 1491 lines (between 40us and 100us)
		if (MaxX() < otherLine.MinX() ||
			MinY() > otherLine.MaxX() ||
			MaxY() < otherLine.MinY() ||
			MinY() > otherLine.MaxY())
			return Intersection.NoIntersection;


		Orientation or1 = GetOrientation(start, stop, otherLine.start);
		Orientation or2 = GetOrientation(start, stop, otherLine.stop);
		Orientation or3 = GetOrientation(otherLine.start, otherLine.stop, start);
		Orientation or4 = GetOrientation(otherLine.start, otherLine.stop, stop);

		// Ok, we got an intersection confirmed, now the question is what type.
		if (or1 != or2 && or3 != or4)
			return Intersection.General;

		// Special Cases for collinearity, may be either be end-points occuring on the other line or complete collinearity.
		bool Collinear1 = false, Collinear2 = false;
		if (or1 == Orientation.Collinear && PointOnSegment(start, stop, otherLine.start) ||
			or2 == Orientation.Collinear && PointOnSegment(start, stop, otherLine.stop))
		{
			Collinear1 = true;
		}
		if (or3 == Orientation.Collinear && PointOnSegment(otherLine.start, otherLine.stop, start) ||
			or4 == Orientation.Collinear && PointOnSegment(otherLine.start, otherLine.stop, stop))
		{
			Collinear2 = true;
		}

		if (Collinear1 && Collinear2)
		{
			return Intersection.Collinear;
		}
		else if (Collinear1 || Collinear2)
			return Intersection.PointInSegment;

		return Intersection.NoIntersection;
	}
}