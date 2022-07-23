using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SymmetryHelperInScene : MonoBehaviour
{
    public bool is3D;
    public int K;

    private GameObject pointCloud;
    private List<Vector2> pointsCoord;
    private List<Vector2> pointsPolar;
    private List<Vector3> pointsCoord3D;
    private List<Vector3> pointsPolar3D;
    private const float e = 0.005f;

    private Vector2 pointsCentroid;
    private Vector2 pointsCentroid3D;
    private List<SymmetryCycle> cycleList = new List<SymmetryCycle>();
    private List<SymmetryCycle3D> cycleList3D = new List<SymmetryCycle3D>();
    Dictionary<float, List<Vector2>> cycles = new Dictionary<float, List<Vector2>>();
    Dictionary<float, List<Vector3>> cycles3D = new Dictionary<float, List<Vector3>>();

    // Start is called before the first frame update
    void Start()
    {
        pointCloud = this.gameObject;

        pointsCoord = new List<Vector2>();
        pointsPolar = new List<Vector2>();
        pointsCoord3D = new List<Vector3>();
        pointsPolar3D = new List<Vector3>();

        foreach (Transform point in pointCloud.transform)
        {
            if (is3D)
            {
                pointsCoord3D.Add(new Vector3(point.position.x, point.position.y, point.position.z));
            } else
            {
                pointsCoord.Add(new Vector2(point.position.x, point.position.y));
            }
        }

        if (is3D)
        {
            pointsCentroid3D = Utils.points3DCentroid(pointsCoord3D);
            symmetry3DPoints();

        } else
        {
            pointsCentroid = Utils.points2DCentroid(pointsCoord);
            symmetry2DPoints();
        }

    }

    public void symmetry2DPoints()
    {
        order(pointsCentroid);

        for (int k = 1; k <= K; k++)
        {
            foreach (SymmetryCycle cycle in cycleList)
            {
                if (cycle.isRotational(k)) Debug.Log(k + "-fold symmetry in cycle " + cycleList.IndexOf(cycle));
            }
        }

        int result = cycleList[0].fold;

        for (int i = 1; i < cycleList.Count; i++)
        {
            result = Utils.GCD(result, cycleList[i].fold);
        }

        if (result == 1)
        {
            Debug.Log("No symmetry");
        }
        else
        {
            Debug.Log("The points has a " + result + "-fold rotational symmetry");
        }
    }

    public void symmetry3DPoints()
    {
        order3D(pointsCentroid3D);

        for (int k = 1; k <= K; k++)
        {
            foreach (SymmetryCycle3D cycle in cycleList3D)
            {
                if (cycle.isRotational(k)) Debug.Log(k + "-fold symmetry in cycle " + cycleList3D.IndexOf(cycle));
            }
        }

        int result = cycleList3D[0].fold;

        for (int i = 1; i < cycleList3D.Count; i++)
        {
            result = Utils.GCD(result, cycleList3D[i].fold);
        }

        if (result == 1)
        {
            Debug.Log("No symmetry");
        }
        else
        {
            Debug.Log("The points has a " + result + "-fold rotational symmetry");
        }
    }

    public void order(Vector2 centroid)
    {
        for(int i = 0; i < pointsCoord.Count; i++)
        {
            pointsCoord[i] = pointsCoord[i] - centroid;
            pointCloud.transform.GetChild(i).position = pointsCoord[i];

            Vector2 polar = Utils.pointToPolarCoord(pointsCoord[i]);

            pointsPolar.Add(polar);

            Debug.Log(polar.x);

            if (!cycles.ContainsKey(polar[0]))
            {
                cycles[polar[0]] = new List<Vector2>();
            }
            
            cycles[polar[0]].Add(polar);
        }

        foreach (float key in cycles.Keys)
        {
            Debug.Log(cycles[key].Count);
            cycleList.Add(new SymmetryCycle(cycles[key]));
        }

        Debug.Log("Ciclos number --> " + cycleList.Count);
    }

    public void order3D(Vector3 centroid)
    {
        for (int i = 0; i < pointsCoord3D.Count; i++)
        {
            pointsCoord3D[i] = pointsCoord3D[i] - centroid;
            pointCloud.transform.GetChild(i).position = pointsCoord3D[i];

            Vector3 polar = Utils.pointToCylindricalCoord(pointsCoord3D[i]);

            pointsPolar3D.Add(polar);

            if (cycleList3D.Count > 0)
            {
                bool isInAnyCycle = false;

                for(int j = 0; j < cycleList3D.Count; j++)
                {
                    if (cycleList3D[j].isInThisCycle(polar.x, e))
                    {
                        cycleList3D[j].addPoint(polar);
                        isInAnyCycle = true;
                        break;
                    }
                }

                if (!isInAnyCycle)
                {
                    cycleList3D.Add(new SymmetryCycle3D(polar.x));
                    cycleList3D[cycleList3D.Count - 1].addPoint(polar);
                }
            }
            else
            {
                cycleList3D.Add(new SymmetryCycle3D(polar[0]));
                cycleList3D[0].addPoint(polar);
            }
        }

        foreach (SymmetryCycle3D cycle in cycleList3D)
        {
            cycle.initCycle();
        }

        Debug.Log("Ciclos number --> " + cycleList3D.Count);
    }

    public float distanceInPolar(Vector2 a, Vector2 b)
    {
        float result = Mathf.Sqrt(Mathf.Pow(a.x, 2.0f) + Mathf.Pow(b.x, 2.0f) - 2*a.x*b.x*Mathf.Cos(a.y - b.y)); Debug.Log(result != float.NaN);
        
        return float.IsNaN(result) ? 0.0f : result;
    }

    public float angleInPolar(Vector2 a, Vector2 b)
    {
        float result = Mathf.Atan((a.x * Mathf.Sin(a.y) - b.x * Mathf.Sin(b.y)) / (a.x * Mathf.Cos(a.y) - b.x * Mathf.Cos(b.y)));
        
        return float.IsNaN(result) ? 0.0f : (float)System.Math.Round(result, 3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
