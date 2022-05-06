using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker
{
    private Vector3 position;
    private GameObject markerInstance;
    private float score;

    public Marker(Vector3 position, GameObject markerInstance)
    {
        this.position = position;
        this.markerInstance = markerInstance;
        this.score = 0.0f;
    }

    public bool isMe(GameObject obj)
    {
        return obj == this.markerInstance;
    }

    public Vector3 Position
    {
        get { return position; }
        set { position = value; }
    }

    public GameObject MarkerInstance
    {
        get { return markerInstance; }
        set { markerInstance = value; }
    }

    public float Score
    {
        get { return score; }
        set { score = value; }
    }

    public Vector3 currentPosition()
    {
        return markerInstance.transform.position;
    }
}