using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternScale : MonoBehaviour
{
    public GameObject[] pattern1;
    public GameObject[] pattern2;

    private bool isDistanceCalculated = false;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 centroid = getCentroid(pattern1);
        Vector3 centroid2 = getCentroid(pattern2);
        float diamaterA = diameter(pattern1);
        float diamaterB = diameter(pattern2);


        foreach (GameObject marker in pattern2)
        {
            marker.transform.position = marker.transform.position * (diamaterA / diamaterB) - new Vector3(0.5f, 1.0f, 0.0f);
        }

        centroid2 = getCentroid(pattern2);
        diamaterB = diameter(pattern2);

        Debug.Log("Final distances --> " + diamaterA + " - " + diamaterB);

    }

    public Vector3 getCentroid(GameObject[] markers)
    {
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        foreach (GameObject marker in markers)
        {
            x += marker.transform.position.x;
            y += marker.transform.position.y;
            z += marker.transform.position.z;
        }

        x = x / markers.Length;
        y = y / markers.Length;
        z = z / markers.Length;

        return new Vector3(x, y, z);
    }

    public float diameter(GameObject[] markers)
    {
        float maxDistance = 0.0f;

        for (int i = 0; i < markers.Length; i++)
        {
            for (int j = i + 1; j < markers.Length; j++)
            {
                float currentDistance = Vector3.Distance(markers[i].transform.position, markers[j].transform.position);

                if (currentDistance > maxDistance)
                {
                    maxDistance = currentDistance;
                }
            }
        }
        
        return maxDistance;
    }

    public static Vector3 becnorm(Vector3 x)
    {
        return x * (1 / Mathf.Sqrt(x.x * x.x + x.y * x.y + x.z * x.z));
    }

    public Vector3[] getPointSet(GameObject[] pattern)
    {
        Vector3[] points = new Vector3[pattern.Length];

        for (int i = 0; i < pattern.Length; i++)
        {
            points[i] = pattern[i].transform.position;
        }

        return points;
    }

    public float haurdoffDistance(Vector3[] A, Vector3[] B)
    {
        float maxDistance = float.MinValue;

        foreach (Vector3 a in A)
        {
            float minDistance = float.MaxValue;
            
            float currentDistance = 0;
            foreach (Vector3 b in B)
            {
                currentDistance = Vector3.Distance(a, b);


                if (currentDistance < minDistance)
                    minDistance = currentDistance;
            }
            
            Debug.Log("Distance between a--> " + a + " : " + minDistance);

            if (minDistance > maxDistance)
                maxDistance = minDistance;
        }

        return maxDistance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDistanceCalculated)
        {
            isDistanceCalculated = true;
            Debug.Log("Haurdoff distance --> " + haurdoffDistance(getPointSet(pattern1), getPointSet(pattern2)));
        }
    }
}
