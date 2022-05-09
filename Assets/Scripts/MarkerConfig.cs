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

    public MarkerConfig(MarkerConfig markerConfig, Vector3 position, Vector3[] vertices)
    {
        this.position = position;
        this.currentInstance = markerConfig.currentInstance;
        this.currentInstance.transform.position = position;
        this.config = deepCopyMarkers(markerConfig.currentInstance, markerConfig.config, vertices);
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

    public List<Marker> deepCopyMarkers(GameObject instance, List<Marker> previusConfig, Vector3[] vertices)
    {
        List<Marker> markers = new List<Marker>();

        for (int i = 0; i < previusConfig.Count; i++)
        {
            int randomIndex = (int)UnityEngine.Random.Range(0, vertices.Length - 1);

            Vector3 markerPosition = instance.transform.TransformPoint(vertices[randomIndex]);
            previusConfig[i].MarkerInstance.transform.position = markerPosition;

            markers.Add(new Marker(vertices[randomIndex], previusConfig[i].MarkerInstance));
        }

        return markers;
    }


    public float getScore(float positions)
    {
        return (this.score*100) / positions;
    }

    public string showScore(int iteration, float positions)
    {
        return iteration + " SCORE : " + this.getScore(positions) + "%";
    }

}
