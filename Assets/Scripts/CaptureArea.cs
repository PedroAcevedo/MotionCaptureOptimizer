using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureArea : MonoBehaviour
{
    public float evaluatePositions;
    public GameObject reference;

    private float posI;
    private float posJ;
    private float posK;
    private float[] separation = { 0.0f, 0.0f, 0.0f };
    private float[] roomDimensions = { 4.0f, 2.0f, 4.0f };
    private float[] minAxis = { -2.0f, 0.6f, -2.0f };
    private float[] maxAxis = { 0.0f, 0.0f, 0.0f };

    // Start is called before the first frame update
    void Start()
    {
        posI = minAxis[0];
        posJ = minAxis[1];
        posK = minAxis[2];

        for(int i = 0; i < 3; i++)
        {
            maxAxis[i] = minAxis[i] + roomDimensions[i];
            separation[i] = (maxAxis[i] - minAxis[i]) / (evaluatePositions - 1);
        }

        for(float i=minAxis[0]; i <= maxAxis[0]; i=i+separation[0])
            for(float j=minAxis[1]; j <= maxAxis[1]; j=j+separation[1])
                for(float k=minAxis[2]; k <= maxAxis[2]; k=k+separation[2])
                    GameObject.Instantiate(reference, new Vector3(i, j, k), Quaternion.identity); 

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
