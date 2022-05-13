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

    public MarkerConfig(MarkerConfig markerConfig, Vector3 position)
    {
        this.position = position;
        this.currentInstance = markerConfig.CurrentInstance;
        this.currentInstance.transform.position = position;
        this.config = deepCopyMarkers(markerConfig.CurrentInstance, markerConfig.Config);
        this.score = markerConfig.Score;
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

    public List<Marker> Config
    {
        get { return config; }
        set { config = value; }
    }

    //MOVE

    public bool addMarker(Vector3[] vertices, int max)
    {
        bool isValid = false;

        if(this.config.Count < max)
        {
            int randomIndex = (int)UnityEngine.Random.Range(0, vertices.Length - 1);

            Vector3 markerPosition = this.currentInstance.transform.TransformPoint(vertices[randomIndex]);

            GameObject currentMarker = Optimizer.InstanceMarker(markerPosition);
            currentMarker.transform.parent = this.currentInstance.transform;

            config.Add(new Marker(vertices[randomIndex], currentMarker));
            isValid = true;
        }

        return isValid;

    }

    public bool relocateMarker(Vector3[] vertices)
    {
        int randomMarker = (int)UnityEngine.Random.Range(0, config.Count - 1);
        int randomIndex = (int)UnityEngine.Random.Range(0, vertices.Length - 1);
        
        config[randomMarker].Position = vertices[randomIndex];
        Vector3 markerPosition = this.currentInstance.transform.TransformPoint(vertices[randomIndex]);

        config[randomMarker].MarkerInstance.transform.position = markerPosition;

        return true;
    }

    public bool deleteMarker(int min)
    {
        bool isValid = false;

        if (this.config.Count > min)
        {
            int randomMarker = (int)UnityEngine.Random.Range(0, config.Count - 1);
            GameObject.Destroy(config[randomMarker].MarkerInstance);
            config.RemoveAt(randomMarker);
            isValid = true;
        }

        return isValid;
    }

    public void placeMarkets(int numberOfMarkers, Vector3[] vertices)
    {
        for (int i = 0; i < numberOfMarkers; i++)
        {
            addMarker(vertices, 100);
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

    public void clearConfig()
    {
        for (int i = 0; i < this.config.Count; i++)
        {
            GameObject.Destroy(this.config[i].MarkerInstance);
        }
    }

    public void resetConfigToCurrent()
    {
        for (int i = 0; i < this.config.Count; i++)
        {
            Vector3 markerPosition = this.currentInstance.transform.TransformPoint(this.config[i].Position);

            GameObject currentMarker = Optimizer.InstanceMarker(markerPosition);
            currentMarker.transform.parent = this.currentInstance.transform;
            this.config[i].MarkerInstance = currentMarker;
        }
    }

    public float getScore(float positions)
    {
        return this.score / positions;
    }

    public string showScore(int iteration, float positions)
    {
        return iteration + " SCORE : " + this.getScore(positions) + "%";
    }

    public float getOverlap(float radius)
    {
        float overlap = 0;

        for (int i = 0; i < this.config.Count; i++)
        {
            for (int j = 0; j < this.config.Count; j++)
            {
                if(i != j)
                {
                    if (Utils.checkCollision(this.config[i].MarkerInstance.transform.position, this.config[j].MarkerInstance.transform.position, radius, radius))
                    {
                        overlap += 1.0f;
                    }
                }
            }
        }

        return overlap / Utils.permutationsWithoutRepetitions(this.config.Count, 2);
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
}
