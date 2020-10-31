using System.Collections.Generic;

using UnityEngine;
using static Prism;

public class QuadTree
{
    //the maximum number of prism in each node
    //public int Max_number;
    //the maximum depth of quad tree
    //public int Max_depth;
    //four children node of the node
    public QuadTree[] subtree;
    //depthth of node
    public int depth;
    //the region of this node represent
    public Rect rect;
    //Prism in each tree
    public List<Prism> objects = new List<Prism>();

    public QuadTree father;

    public QuadTree(Rect rect,int depth, QuadTree father)
    {
        this.rect = rect;
        this.depth = depth + 1;
        this.rect = rect;
        this.father = father;
        objects = new List<Prism>();
    }

    //add a prism to tree
    public void Addprism(Prism obj)
    {
        //get location of Prism
        //Debug.Log("2");
        float x = obj.prismObject.transform.localScale.x;
        float y = obj.prismObject.transform.localScale.z;

        //prism should be add in this node if subtree is null 
        if(subtree == null)
        {
            objects.Add(obj);
            //Debug.Log("3");
            //if maximum number is reached and maximum depth is not reached
            if(depth <= 5 && objects.Count > 5)
            {
                Split(depth);
            }
        }
        //prism should be add in children nodes of this node
        else
        {
            // Debug.Log(father.rect.o.x.Tostring());
            // Debug.DrawLine(new Vector3((float)father.rect.o.x,0,(float)father.rect.o.y),new Vector3((float)(father.rect.o.x+father.rect.Rwidth),0,(float)father.rect.o.y) , Color.red);
            //Debug.Log("4");
            int index = getIndex(obj);//获取所在对应子节点的想象
            
            if (index >= 0)
            {
                subtree[index].Addprism(obj);//调用该子节点的添加方法
            }
            else//如果象限小于 那就是这个物体在分界线上，因此属于该节点管理
            {
                objects.Add(obj);
            }
        }
        Debug.Log(objects.Count);
    } 

    public void Split(int depth)
    {
        //Debug.Log("5");
        subtree = new QuadTree[4];
        subtree[0] = new QuadTree(new Rect(rect.o.x + rect.Rwidth/2, rect.o.y + rect.Rheight/2, rect.Rwidth, rect.Rheight), depth, this);
        subtree[1] = new QuadTree(new Rect(rect.o.x + rect.Rwidth/2, rect.o.y - rect.Rheight/2, rect.Rwidth, rect.Rheight), depth, this);
        subtree[2] = new QuadTree(new Rect(rect.o.x - rect.Rwidth/2, rect.o.y - rect.Rheight/2, rect.Rwidth, rect.Rheight), depth, this);
        subtree[3] = new QuadTree(new Rect(rect.o.x - rect.Rwidth/2, rect.o.y + rect.Rheight/2, rect.Rwidth, rect.Rheight), depth, this);
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            int index = getIndex(objects[i]);
            if (index >= 0)
            {
                subtree[index].Addprism(objects[i]);
                objects.Remove(objects[i]);
            }
        }
    }

    // public void draw(Vector3 a,Vector3 b,Vector3 c,Vector3 d)
    // {
    //     Debug.DrawLine(a, b, yellow);
    //     Debug.DrawLine(b, c, yellow);
    //     Debug.DrawLine(c, d, yellow);
    //     Debug.DrawLine(d, a, yellow);
    // }

    public int getIndex(Prism target)
    {
        int indexOne = 0;
        int indexTwo = 0;
        int indexThree = 0;
        int indexFour = 0;
        for (int i = 0; i < target.points.Length; i++) 
        {
            //Debug.Log(rect.o.x + rect.Rwidth);
            //Debug.Log(rect.o.x);
            if ((target.points[i].x < (rect.o.x + rect.Rwidth)) && (target.points[i].x > rect.o.x) 
                    && (target.points[i].z < rect.o.y + rect.Rheight) && (target.points[i].z > rect.o.y))
            {
                indexOne++;
                //Debug.Log("indexOne");
            }
            if ((target.points[i].x < rect.o.x + rect.Rwidth) && (target.points[i].x > rect.o.x) 
                    && (target.points[i].z < rect.o.y) && (target.points[i].z > (rect.o.y - rect.Rheight)))
            {
                indexTwo++;
                //Debug.Log("indexTwo");
            }
            if ((target.points[i].x < rect.o.x) && (target.points[i].x > (rect.o.x - rect.Rwidth)) 
                   && (target.points[i].z < rect.o.y) && (target.points[i].z > (rect.o.y - rect.Rheight)))
            {
                    indexThree++;
                    //Debug.Log("indexThree");
            }
            if ((target.points[i].x < rect.o.x) && (target.points[i].x > rect.o.x - rect.Rwidth) 
                    && (target.points[i].z < (rect.o.y + rect.Rheight)) && (target.points[i].z > rect.o.y))
            {
                    indexFour++;
            //Debug.Log("indexFour");
            }
        }
        //Debug.Log("i" + indexOne);
        if (indexOne == target.points.Length)
        {
            indexOne = 0;
            return 0;
        }
        if (indexTwo == target.points.Length)
        {
            return 1;
        }
        if (indexThree == target.points.Length)
        {
            return 2;
        }
        if (indexFour == target.points.Length)
        {
            return 3;
        }
        return -1;
    }
}