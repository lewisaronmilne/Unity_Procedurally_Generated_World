using System;

public class Coord
{
    public int x { get; private set; }
    public int y { get; private set; }

    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override bool Equals(Object obj)
    {
        if(obj == null || GetType() != obj.GetType())
            return false;

        Coord c = (Coord)obj;
        return (x == c.x) && (y == c.y);
    }

    public override int GetHashCode()
    {
        return x^y;
    }

    public static float DistanceBetween(Coord a, Coord b)
    {
        return (float) Math.Sqrt((a.x - b.x)*(a.x - b.x) + (a.y - b.y)*(a.y - b.y));
    } 
}
