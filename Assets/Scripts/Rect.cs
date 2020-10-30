
using UnityEngine;

public enum QuadrantEnum : int
{
    RU=0,
    LU,
    LB,
    RB
}

public class Point
{
    public double x;
    public double y;

    public Point(double x, double y)
    {
        this.x = x;
        this.y = y;
    }
}

public class Rect  {

    public Point o;
    public double Rwidth, Rheight;

    public Rect(double x, double y, double w, double h)
    {
        o = new Point(x, y);
        Debug.DrawLine(new Vector3((float)(x + w / 2), 0, (float)(y + w / 2)), new Vector3((float)(x + w / 2), 0,(float)(y - w / 2)), Color.red);
        Debug.DrawLine(new Vector3((float)(x + w / 2), 0, (float)(y + w / 2)), new Vector3((float)(x - w / 2), 0,(float)(y + w / 2)), Color.red);
        Debug.DrawLine(new Vector3((float)(x - w / 2), 0, (float)(y - w / 2)), new Vector3((float)(x + w / 2), 0,(float)(y - w / 2)), Color.red);
        Debug.DrawLine(new Vector3((float)(x - w / 2), 0, (float)(y - w / 2)), new Vector3((float)(x - w / 2), 0,(float)(y + w / 2)), Color.red);
        Rwidth = w / 2;
        Rheight = h / 2;
    }

    public Rect(Point point, double w, double h)
    {
        o = point;
        Rwidth = w / 2;
        Rheight = h / 2;
    }

    public bool IsInclude(Prism target)
    {
        if((Mathf.Abs((float)(target.prismObject.transform.position.x-o.x))<=(Rwidth))&&(Mathf.Abs((float)(target.prismObject.transform.position.z - o.y)) <= (Rheight)))
            return true;
        return false;
    }
}