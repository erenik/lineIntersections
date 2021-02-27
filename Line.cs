/**
	Emil Hedemalm
	2021-02-27
	Class for managing continuous lines based on LineSegments.
*/

using System;
using System.Collections.Generic;

class Line
{

	public Boolean Intersects(LineSegment otherLineSegment)
	{
		foreach (LineSegment lineSegment in LineSegments)
		{
			if (lineSegment.Intersects(otherLineSegment) != Intersection.NoIntersection)
				return true;
		}
		return false;
	}

	public void ExtendLine(LineSegment withSegment)
	{
		// Skip 0-length lines, they will be included in other segments.
		if (withSegment.Length() == 0)
			return;
		LineSegments.Add(withSegment);
	}

	public List<LineSegment> LineSegments { get; private set; } = new List<LineSegment>();

}
