using System;

namespace Rogue
{
	public struct Point : IEquatable<Point> {
		public int X { get; private set; }
		public int Y { get; private set; }

		public Point(int x, int y){
			X = x;
			Y = y;
		}

		public Point(Point src){
			X = src.X;
			Y = src.Y;
		}

		public static Point operator + (Point a, Point b) {
			return new Point (a.X + b.X, a.Y + b.Y);
		}

		public static Point operator - (Point a, Point b) {
			return new Point (a.X - b.X, a.Y - b.Y);
		}

		public static Point operator + (Point a, Direction d) {
			return a + d.ToPos();
		}

		public static bool operator == (Point a, Point b) {
			return a.Equals(b);
		}
		public static bool operator != (Point a, Point b) {
			return !a.Equals(b);
		}

		public bool Equals (Point a) {
			return X == a.X && Y == a.Y;
		}
		public bool Equals (int _x, int _y) {
			return X == _x && Y == _y;
		}
		public override bool Equals (object obj) {
			if (obj == null || !(obj is Point)) {
				return false;
			}
			Point p = (Point)obj;
			return p.X == X && p.Y == Y;
		}

		public bool IsOrigin {
			get { return (X==0 && Y==0); }
		}

		public override int GetHashCode () {
			return X * 1000 + Y;
		}

		public override string ToString () {
			return "(" + X + ", " + Y + ")";
		}

		static int[] pos2dir = new int[] {
			(int)Direction.NorthWest, (int)Direction.North, (int)Direction.NorthEast,
			(int)Direction.West, (int)Direction.North, (int)Direction.East,
			(int)Direction.SouthWest, (int)Direction.South, (int)Direction.SouthEast,
		};

		public Direction ToDir () {
			if (Y >= -1 && Y <= 1 && X >= -1 && X <= 1) {
				return (Direction)pos2dir [(Y + 1) * 3 + X + 1];
			}
			return Direction.None;
		}

		public float Length () {
			return (float)System.Math.Sqrt(X * X + Y * Y);
		}

	}

}

