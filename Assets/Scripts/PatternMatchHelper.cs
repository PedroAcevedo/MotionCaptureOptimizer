using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternMatchHelper
{
    public GameObject pattern;
    public float patternDiameter;

    public GameObject[] initialPattern;
    public Vector3[] transformations;

    private List<GameObject> patternInScene;

    public PatternMatchHelper(GameObject[] initialPattern, Vector3[] transformations)
    {
        this.initialPattern = initialPattern;
        this.transformations = transformations;
    }

    // Start is called before the first frame update
    public float calculatePatternCost()
    {
        patternInScene = new List<GameObject>();

        foreach (GameObject pattern in initialPattern)
        {
            patternInScene.Add(GameObject.Instantiate(pattern, this.pattern.transform.parent.position, Quaternion.identity));
        }

        //patternDiameter = diameter(pattern);

        float patternCostAverage = 0.0f;
    
        for (int i = 0; i < patternInScene.Count; i++)
        {
            float minHd = float.MaxValue;

            foreach (Vector3 rotation in transformations)
            {
                patternInScene[i].transform.Rotate(rotation.x, rotation.y, rotation.z);

                float diamaterB = diameter(patternInScene[i]);

                foreach (Transform marker in patternInScene[i].transform)
                {
                    marker.position = marker.position * (patternDiameter / diamaterB);
                }

                //Debug.Log("Final distances --> " + diamaterA + " - " + diamaterB);

                float HD = haurdoffDistance(getPointSet(pattern), getPointSet(patternInScene[i]));

                if(HD < minHd)
                {
                    minHd = HD;
                }

            }
            patternCostAverage += getPatternCost(patternDiameter, minHd);
        }

        for (int i = 0; i<patternInScene.Count; i++)
        {
            GameObject.Destroy(patternInScene[i]);
        }

        return patternCostAverage / patternInScene.Count;
    }

    public float getPatternCost(float diameter, float distance)
    {
        float cost = Mathf.Abs((diameter/2) - distance) / (diameter / 2);

        if(cost > 1.0f)
        {
            cost = 1.0f;
        }

        return cost;
    }

    public Vector3 getCentroid(GameObject markers)
    {
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        int childCount = 0;

        foreach (Transform marker in markers.transform)
        {
            x += marker.position.x;
            y += marker.position.y;
            z += marker.position.z;

            childCount++;
        }

        x = x / childCount;
        y = y / childCount;
        z = z / childCount;

        return new Vector3(x, y, z);
    }

    public float diameter(GameObject markers)
    {
        float maxDistance = 0.0f;

        for (int i = 0; i < markers.transform.childCount; i++)
        {
            for (int j = i + 1; j < markers.transform.childCount; j++)
            {
                float currentDistance = Vector3.Distance(markers.transform.GetChild(i).position, markers.transform.GetChild(j).position);

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

    public Vector3[] getPointSet(GameObject pattern)
    {
        Vector3[] points = new Vector3[pattern.transform.childCount];

        for (int i = 0; i < pattern.transform.childCount; i++)
        {
            points[i] = pattern.transform.GetChild(i).position;
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
            
            //Debug.Log("Distance between a--> " + a + " : " + minDistance);

            if (minDistance > maxDistance)
                maxDistance = minDistance;
        }

        return maxDistance;
    }
}
