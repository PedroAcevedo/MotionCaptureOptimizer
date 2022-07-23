using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymmetryCycle3D
{
    public int fold;

    private List<Vector3> points;
    private float n;
    private string codedSet = "";
    private string searchingSet = "";
    private float radius;

    public SymmetryCycle3D(float radius)
    {
        this.points = new List<Vector3>();
        this.n = points.Count;
        this.fold = 1;
        this.codedSet = "";
        this.searchingSet = "";
        this.radius = radius;
    }

    public void addPoint(Vector3 point)
    {
        points.Add(point);
    }

    public void initCycle()
    {
        this.points.Sort((a, b) => a.y.CompareTo(b.y));

        if(this.points.Count > 1)
        {
            encoded();
        }
    }

    public void encoded()
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            string element = Utils.angleDiff(points[i], points[i + 1]) + ",";

            codedSet += element;
            if (i != 0) searchingSet += element;
        }

        codedSet = codedSet.Substring(0, codedSet.Length - 1);
        if(searchingSet != "")
            searchingSet = searchingSet.Substring(0, searchingSet.Length - 1) + "," + codedSet;
    }

    public bool isRotational(int k)
    {
        bool result = false;

        if (points.Count > 1)
        {
            result = Utils.KMP(transformation(k), searchingSet);
            if (result)
            {
                if(fold != 1)
                {
                    fold = fold > k ? k : fold;
                }
                else
                {
                    fold = k;
                }
            }

        }

        return result;
    }

    public string transformation(int k)
    {
        List<Vector3> transformPoints = new List<Vector3>();

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = new Vector3(points[i].x, points[i].y + (360 / (float)k) * Mathf.Deg2Rad, points[i].z);
            transformPoints.Add(point);
        }

        string transformString = "";

        List<int> indexes = generatedRotateIndex(k, transformPoints.Count);

        for (int i = 0; i < indexes.Count - 1; i++)
        {
            string element = Utils.angleDiff(transformPoints[indexes[i]], transformPoints[indexes[i + 1]]) + ",";

            transformString += element;
        }

        transformString = transformString.Substring(0, transformString.Length - 1);

        return transformString;
    }

    public List<int> generatedRotateIndex(int k, int n)
    {
        List<int> indexes = new List<int>();

        for (int i = k % n; i < n; i++)
        {
            indexes.Add(i);
        }

        if (indexes.Count < n)
        {
            for (int i = 0; i < k % n; i++)
            {
                indexes.Add(i);
            }
        }

        return indexes;
    }

    public bool isInThisCycle(float r, float e)
    {
        return r >= (this.radius) && r <= (this.radius + e);
    }
}
