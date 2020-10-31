using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrismManager : MonoBehaviour
{
    public float speed = 0;
    public int prismCount = 10;
    public float prismRegionRadiusXZ = 5;
    public float prismRegionRadiusY = 5;
    public float maxPrismScaleXZ = 5;
    public float maxPrismScaleY = 5;
    public GameObject regularPrismPrefab;
    public GameObject irregularPrismPrefab;

    private List<Prism> prisms = new List<Prism>();
    private List<GameObject> prismObjects = new List<GameObject>();
    private GameObject prismParent;
    private Dictionary<Prism,bool> prismColliding = new Dictionary<Prism, bool>();

    private const float UPDATE_RATE = 0.5f;

    #region Unity Functions

    void Start()
    {
        Random.InitState(0);    //10 for no collision
        prismParent = GameObject.Find("Prisms");
        for (int i = 0; i < prismCount; i++)
        {
            var randPointCount = Mathf.RoundToInt(3 + Random.value * 7);
            var randYRot = Random.value * 360;
            var randScale = new Vector3((Random.value - 0.5f) * 2 * maxPrismScaleXZ, (Random.value - 0.5f) * 2 * maxPrismScaleY, (Random.value - 0.5f) * 2 * maxPrismScaleXZ);
            var randPos = new Vector3((Random.value - 0.5f) * 2 * prismRegionRadiusXZ, (Random.value - 0.5f) * 2 * prismRegionRadiusY, (Random.value - 0.5f) * 2 * prismRegionRadiusXZ);

            GameObject prism = null;
            Prism prismScript = null;
            if (Random.value < 0.5f)
            {
                prism = Instantiate(regularPrismPrefab, randPos, Quaternion.Euler(0, randYRot, 0));
                prismScript = prism.GetComponent<RegularPrism>();
            }
            else
            {
                prism = Instantiate(irregularPrismPrefab, randPos, Quaternion.Euler(0, randYRot, 0));
                prismScript = prism.GetComponent<IrregularPrism>();
            }
            prism.name = "Prism " + i;
            prism.transform.localScale = randScale;
            prism.transform.parent = prismParent.transform;
            prismScript.pointCount = randPointCount;
            prismScript.prismObject = prism;

            prisms.Add(prismScript);
            prismObjects.Add(prism);
            prismColliding.Add(prismScript, false);
        }

        StartCoroutine(Run());
    }
    
    void Update()
    {
        #region Visualization

        DrawPrismRegion();
        DrawPrismWireFrames();

#if UNITY_EDITOR
        if (Application.isFocused)
        {
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        }
#endif

        #endregion
    }

    IEnumerator Run()
    {
        yield return null;

        while (true)
        {
            foreach (var prism in prisms)
            {
                prismColliding[prism] = false;
            }

            foreach (var collision in PotentialCollisions())
            {
                if (CheckCollision(collision))
                {
                    prismColliding[collision.a] = true;
                    prismColliding[collision.b] = true;

                    ResolveCollision(collision);
                }
            }

            yield return new WaitForSeconds(UPDATE_RATE);
        }
    }

    #endregion

    #region Incomplete Functions

    private IEnumerable<PrismCollision> PotentialCollisions()
    {                                              
        QuadTree quadtree = new QuadTree(new Rect(0, 0, 10, 10), 0, null);
        for (int i = 0; i < prisms.Count; i++)
        {
            Debug.Log("1");
            prisms[i].check = false;
            quadtree.Addprism(prisms[i]);
        }

        foreach (PrismCollision check in QuadtreeRecursion(quadtree))
        {
            yield return check;
        }
    }
    

    private bool CheckCollision(PrismCollision collision)
    {
        
        var prismA = collision.a;
        var prismB = collision.b;
        List<Vector3> points;
        bool check = false;

        //Minkowski Difference Computation
        points = new List<Vector3>();
        points.Clear();
        foreach(var point1 in prismA.points)
        {
          foreach (var point2 in prismB.points)
          {
              var result = Vector3.zero;
              result = point1 - point2;
              points.Add(result);
          }
        }
        //gjk
        bool[] col = new bool[4];
        for (int i = 1; i < col.Length; i++)
        {
            col[i] = false;
        }
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].x > 0 && points[i].z > 0)
            {
                col[0] = true;
            }
            if (points[i].x > 0 && points[i].z < 0)
            {
                col[1] = true;
            }
            if (points[i].x < 0 && points[i].z < 0)
            {
                col[2] = true;
            }
            if (points[i].x < 0 && points[i].z > 0)
            {
                col[3] = true;
            }
            if (points[i].x > 0 && points[i].z == 0)
            {
                col[0] = true;
                col[1] = true;
            }
            if (points[i].x < 0 && points[i].z == 0)
            {
                col[2] = true;
                col[3] = true;
            }
            if (points[i].x == 0 && points[i].z > 0)
            {
                col[0] = true;
                col[3] = true;
            }
            if (points[i].x == 0 && points[i].z < 0)
            {
                col[1] = true;
                col[2] = true;
            }
        }
        int count = 0;
        for (int i = 0; i < col.Length; i++)
        {
            if (col[i] == true)
            {
                count++;
            }
        }
        if (count == 4)
        {
            check = true;
        }
        else
        {
            int far = 0;
            for (int i = 0; i < points.Count - 2; i++)
            {
                if(points[far].sqrMagnitude < points[1].sqrMagnitude)
                {
                    far = i;
                }
            }
            for (int i = 0; i < points.Count - 1; i++)
            {
                if (check)
                {
                    break;
                }
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (i != far && j != far) {
                        if (whichSide(points[far], points[i], points[j]) == whichSide(points[far], points[i], new Vector3(0, 0, 0))
                        && whichSide(points[i], points[j], points[far]) == whichSide(points[i], points[j], new Vector3(0, 0, 0))
                        && whichSide(points[j], points[far], points[i]) == whichSide(points[j], points[far], new Vector3(0, 0, 0)))
                        {
                            check = true;
                            break;
                        }
                    }
                } 
            }
        }
        if(!check){
            return false;
        }
        //epa
        Vector3 speedV = new Vector3(0, 0, 0);
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 0; j < points.Count; j++)
            {
                if (i != j)
                {
                    bool edge = true;
                    for (int k = 0; k < points.Count; k++)
                    {
                        if (!(k == i || k == j))
                        {
                            if (whichSide(points[i], points[j], points[k]) != whichSide(points[i], points[j], new Vector3(0, 0, 0)))
                            {
                                edge = false;
                            }
                        }
                    }
                    if (edge && (((perpendicular(points[i], points[j]).sqrMagnitude < speedV.sqrMagnitude) || (speedV.x == 0 && speedV.y == 0 && speedV.z == 0))))
                    {
                        speedV = perpendicular(points[i], points[j]);
                    }
                }
            }
        }
        collision.penetrationDepthVectorAB = new Vector3(speed * speedV.x, 0 , speed * speedV.z);
        return true;
    }

  private float PointToLine(Vector3 p, Vector3 a, Vector3 b)
  {
    var newVec = p - a;
    var dir = b - a;
    var tangent = Vector3.Cross(dir, Vector3.up).normalized;
    var result = Vector3.Dot(newVec, tangent) / (newVec.magnitude) * newVec.magnitude;
    return result;
  }
  private Vector3 PointToLineTangent(Vector3 p, Vector3 a, Vector3 b)
  {
    var newVec = p - a;
    var dir = b - a;
    var tangent = Vector3.Cross(dir, Vector3.up).normalized;

    var result = Vector3.Dot(newVec, tangent) / (newVec.magnitude) * newVec.magnitude;

    return tangent * result;
  }

  private int MinIndex(List<float> a)
  {
    float b = a[0];
    int index = 0;
    for(int i = 0; i < a.Count; i++)
    {
      if(b > a[i])
      {
        b = a[i];
        index = i;
      }     
    }
    return index;
  }
    #endregion

    #region Private Functions
    
    private void ResolveCollision(PrismCollision collision)
    {
        var prismObjA = collision.a.prismObject;
        var prismObjB = collision.b.prismObject;

        var pushA = -collision.penetrationDepthVectorAB / 2;
        var pushB = collision.penetrationDepthVectorAB / 2;

        for (int i = 0; i < collision.a.pointCount; i++)
        {
            collision.a.points[i] += pushA;
        }
        for (int i = 0; i < collision.b.pointCount; i++)
        {
            collision.b.points[i] += pushB;
        }
        //prismObjA.transform.position += pushA;
        //prismObjB.transform.position += pushB;

        Debug.DrawLine(prismObjA.transform.position, prismObjA.transform.position + collision.penetrationDepthVectorAB, Color.cyan, UPDATE_RATE);
    }

    private IEnumerable<PrismCollision> QuadtreeRecursion(QuadTree quadtree)
    {
        if (quadtree.subtree != null)
        {
            Debug.Log("20");
            foreach (PrismCollision check in QuadtreeRecursion(quadtree.subtree[0]))
                yield return check;
            foreach (PrismCollision check in QuadtreeRecursion(quadtree.subtree[1]))
                yield return check;
            foreach (PrismCollision check in QuadtreeRecursion(quadtree.subtree[2]))
                yield return check;
            foreach (PrismCollision check in QuadtreeRecursion(quadtree.subtree[3]))
                yield return check;
        }

        if (quadtree.father != null)
        {
            for (int i = 0; i < quadtree.objects.Count; i++)
            {
                foreach (PrismCollision check in fatherRecursion(quadtree.father, quadtree.objects[i]))
                    yield return check;
            }
        }
        for (int i = 0; i < quadtree.objects.Count - 1; i++)
        {
            if (quadtree.objects[i].check == false)
            {
                for (int j = i + 1; j < quadtree.objects.Count; j++)
                {
                    if (quadtree.objects[i].prismObject.name != quadtree.objects[j].prismObject.name)
                    {
                        Debug.Log("30");
                        var check = new PrismCollision();
                        check.a = quadtree.objects[i];
                        check.b = quadtree.objects[j];
                        quadtree.objects[i].check = true;
                        yield return check;
                    }
                }
            }
        }
    }

    private IEnumerable<PrismCollision> fatherRecursion(QuadTree quadFather, Prism obj)
    {
        if (quadFather.father != null)
        {
            foreach (PrismCollision check in fatherRecursion(quadFather.father, obj))
                yield return check;
        }
        for (int i = 0; i < quadFather.objects.Count; i++)
        {
            if (quadFather.objects[i].check == false)
            {
                Debug.Log("50");
                var check = new PrismCollision();
                check.a = quadFather.objects[i];
                check.b = obj;
                yield return check;
            }
        }
    }

    public static Vector3 perpendicular(Vector3 a, Vector3 b)
    {

        Vector3 ab = b - a;
        Vector3 ao = Vector3.zero - a;

        float projection = Vector3.Dot(ab, ao) / ab.sqrMagnitude;
        return a+ab * projection;
    }

    public static int whichSide(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        float cross = ab.x * ac.z - ab.z * ac.x;
        return cross > 0 ? 1 : (cross < 0 ? -1 : 0);
    }



    private bool IntersectingZ(Prism a, Prism b)
    {
        if (a.max.z > b.min.z && a.min.z < b.max.z)
        {
            return true;
        }
        if (b.max.z > a.min.z && b.min.z < a.max.z)
        {
            return true;
        }
        return false;
    }

    #endregion

    #region Visualization Functions

    private void DrawPrismRegion()
    {
        var points = new Vector3[] { new Vector3(1, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 1) }.Select(p => p * prismRegionRadiusXZ).ToArray();
        
        var yMin = -prismRegionRadiusY;
        var yMax = prismRegionRadiusY;

        var wireFrameColor = Color.yellow;

        foreach (var point in points)
        {
            Debug.DrawLine(point + Vector3.up * yMin, point + Vector3.up * yMax, wireFrameColor);
        }

        for (int i = 0; i < points.Length; i++)
        {
            Debug.DrawLine(points[i] + Vector3.up * yMin, points[(i + 1) % points.Length] + Vector3.up * yMin, wireFrameColor);
            Debug.DrawLine(points[i] + Vector3.up * yMax, points[(i + 1) % points.Length] + Vector3.up * yMax, wireFrameColor);
        }
    }

    private void DrawPrismWireFrames()
    {
        for (int prismIndex = 0; prismIndex < prisms.Count; prismIndex++)
        {
            var prism = prisms[prismIndex];
            var prismTransform = prismObjects[prismIndex].transform;

            var yMin = prism.midY - prism.height / 2 * prismTransform.localScale.y;
            var yMax = prism.midY + prism.height / 2 * prismTransform.localScale.y;

            var wireFrameColor = prismColliding[prisms[prismIndex]] ? Color.red : Color.green;

            foreach (var point in prism.points)
            {
                Debug.DrawLine(point + Vector3.up * yMin, point + Vector3.up * yMax, wireFrameColor);
            }

            for (int i = 0; i < prism.pointCount; i++)
            {
                Debug.DrawLine(prism.points[i] + Vector3.up * yMin, prism.points[(i + 1) % prism.pointCount] + Vector3.up * yMin, wireFrameColor);
                Debug.DrawLine(prism.points[i] + Vector3.up * yMax, prism.points[(i + 1) % prism.pointCount] + Vector3.up * yMax, wireFrameColor);
            }
        }
    }

    #endregion

    #region Utility Classes

    private class PrismCollision
    {
        public Prism a;
        public Prism b;
        public Vector3 penetrationDepthVectorAB;
    }

    private class Tuple<K,V>
    {
        public K Item1;
        public V Item2;

        public Tuple(K k, V v) {
            Item1 = k;
            Item2 = v;
        }
    }

    #endregion
}
