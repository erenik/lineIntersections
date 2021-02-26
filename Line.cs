/**
	Emil Hedemalm
	2021-02-26
	Line and intersection checks, mainly ported from the C++ game engine work, with a small modification 
	of adding a Axis-aligned Bounding-box check to the beginning of the intersection function.
	https://github.com/erenik/engine/blob/master/src/PhysicsLib/Shapes/Line.cpp#L72
	Originally based off of: https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
*/

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
		this.Start = start;
		this.Stop = stop;
	}

	public Point Start;
	public Point Stop;

	public double Length()
	{
		return Math.Sqrt(Math.Pow(Stop.X - Start.X, 2) + Math.Pow(Stop.Y - Start.Y, 2));
	}

	public float MaxX() { return Math.Max(Start.X, Stop.X); }
	public float MinX() { return Math.Min(Start.X, Stop.X); }
	public float MaxY() { return Math.Max(Start.Y, Stop.Y); }
	public float MinY() { return Math.Min(Start.Y, Stop.Y); }

	public enum Orientation
	{
		Clockwise,
		CounterClockwise,
		Collinear,
	};

	public Orientation GetOrientation(Point p1, Point p2, Point p3)
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
	public Boolean PointOnSegment(Point lineStart, Point lineStop, Point point)
	{
		if (point.X <= Math.Max(lineStart.X, lineStop.X) && point.X >= Math.Max(lineStart.X, lineStop.X) &&
			point.Y <= Math.Max(lineStart.Y, lineStop.Y) && point.Y >= Math.Max(lineStart.Y, lineStop.Y))
			return true;
		return false;
	}

	// Port of intersection test from game engine, with some additional AABB if-checks for slight perf gain
	public Intersection Intersects(Line otherLine)
	{
		// Example intersection times without the Min/Max X/Y checks: 162 microseconds for 1460 lines randomly generated short lines. 
		// Adding the below 5 lines of simplified Axis-aligned Bounding-box checks adjusted time spent to around 60 microseconds for 1491 lines
		if (MaxX() < otherLine.MinX() ||
			MinX() > otherLine.MaxX() ||
			MaxY() < otherLine.MinY() ||
			MinY() > otherLine.MaxY())
			return Intersection.NoIntersection;

		Orientation or1 = GetOrientation(Start, Stop, otherLine.Start);
		Orientation or2 = GetOrientation(Start, Stop, otherLine.Stop);
		Orientation or3 = GetOrientation(otherLine.Start, otherLine.Stop, Start);
		Orientation or4 = GetOrientation(otherLine.Start, otherLine.Stop, Stop);

		// Ok, we got an intersection confirmed, now the question is what type.
		if (or1 != or2 && or3 != or4)
			return Intersection.General;

		// Special Cases for collinearity, may be either be end-points occuring on the other line or complete collinearity.
		bool Collinear1 = false, Collinear2 = false;
		if (or1 == Orientation.Collinear && PointOnSegment(Start, Stop, otherLine.Start) ||
			or2 == Orientation.Collinear && PointOnSegment(Start, Stop, otherLine.Stop))
		{
			Collinear1 = true;
		}
		if (or3 == Orientation.Collinear && PointOnSegment(otherLine.Start, otherLine.Stop, Start) ||
			or4 == Orientation.Collinear && PointOnSegment(otherLine.Start, otherLine.Stop, Stop))
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
