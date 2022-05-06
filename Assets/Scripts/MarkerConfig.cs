using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerConfig
{
    private GameObject currentInstance;
    private Vector3 position;
    private List<Marker> config;
    private float score;

    public MarkerConfig(Vector3 position, GameObject currentInstance)
    {
        this.position = position;
        this.currentInstance = currentInstance;
        this.config = new List<Marker>();
        this.score = 0.0f;
    }

    public MarkerConfig(MarkerConfig markerConfig)
    {
        this.position = markerConfig.position + Vector3.zero;
        this.currentInstance = markerConfig.currentInstance;
        this.config = deepCopyMarkers(markerConfig.currentInstance, markerConfig.config);
        this.score = 0.0f;
    }

    public Vector3 Position
    {
        get { return position; }
        set { position = value; }
    }

    public float Score
    {
        get { return score; }
        set { score = value; }
    }

    public GameObject CurrentInstance
    {
        get { return currentInstance; }
        set { currentInstance = value; }
    }

    public void placeMarkets(int numberOfMarkers, Vector3[] vertices)
    {
        for (int i = 0; i < numberOfMarkers; i++)
        {
            int randomIndex = (int)UnityEngine.Random.Range(0, vertices.Length - 1);

            Vector3 markerPosition = this.currentInstance.transform.TransformPoint(vertices[randomIndex]);

            GameObject currentMarker = Optimizer.InstanceMarker(markerPosition);
            currentMarker.transform.parent = this.currentInstance.transform;

            config.Add(new Marker(vertices[randomIndex], currentMarker));
        }
    }
    
    int layermask = (int)(1 << 8);

    public void evaluateConfig(List<GameObject> cameras)
    {
        float currentScore = 0.0f; 

        for (int i = 0; i < this.config.Count; i++)
        {
            for (int j = 0; j < cameras.Count; j++)
            {
                RaycastHit objectHit;

                Vector3 dir = this.config[i].currentPosition() - cameras[j].GetComponent<Camera>().transform.position;
                if (Physics.Raycast(cameras[j].GetComponent<Camera>().transform.position, dir, out objectHit))
                {
                    if (this.config[i].isMe(objectHit.transform.gameObject))
                    {
                        this.config[i].Score += 1;
                    }
                }
            }

            currentScore += (this.config[i].Score / cameras.Count);
        }

        currentScore = currentScore / this.config.Count;

        this.score += currentScore;
    }

    public void changePosition(Vector3 position)
    {
        this.position = position;
        this.currentInstance.transform.position = this.position;
    }

    public void resetConfig()
    {
        this.currentInstance.transform.position = this.position;

        for (int i = 0; i < this.config.Count; i++)
        {
            Vector3 markerPosition = this.currentInstance.transform.TransformPoint(this.config[i].Position);
            this.config[i].MarkerInstance.transform.position = markerPosition;
            this.config[i].Score = 0;
        }
    }

    public List<Marker> deepCopyMarkers(GameObject instance, List<Marker> previusConfig)
    {
        List<Marker> markers = new List<Marker>();

        for (int i = 0; i < previusConfig.Count; i++)
        {
            Vector3 markerPosition = instance.transform.TransformPoint(previusConfig[i].Position);
            previusConfig[i].MarkerInstance.transform.position = markerPosition;

            markers.Add(new Marker(previusConfig[i].Position, previusConfig[i].MarkerInstance));
        }

        return markers;
    }


    public float getScore(int positions)
    {
        return this.score / positions;
    }

    public string showScore(int iteration)
    {
        return iteration + " SCORE : " + this.score ;
    }

}
