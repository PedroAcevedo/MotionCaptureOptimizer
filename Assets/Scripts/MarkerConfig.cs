using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerConfig
{

    const int layerMask = 1 << 3;

    private GameObject currentInstance;
    private Vector3 position;
    private List<Marker> config;
    private float score;
    private string lastMove;

    private int selectedMarker;
    private Vector3 lastMarkerPosition;
    private Color[] laserColor = { Color.green, Color.red, Color.blue, Color.yellow };

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
            lastMove = MotionCaptureConstants.MOVE_ACTION_ADD;
        }

        return isValid;

    }

    public bool relocateMarker(Vector3[] vertices)
    {
        lastMove = MotionCaptureConstants.MOVE_ACTION_MODIFY;

        int randomMarker = (int)UnityEngine.Random.Range(0, config.Count - 1);
        int randomIndex = (int)UnityEngine.Random.Range(0, vertices.Length - 1);

        selectedMarker = randomMarker;
        lastMarkerPosition = config[randomMarker].Position;

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

            selectedMarker = randomMarker;
            lastMarkerPosition = config[randomMarker].Position;

            GameObject.Destroy(config[randomMarker].MarkerInstance);
            config.RemoveAt(randomMarker);
            isValid = true;
            lastMove = MotionCaptureConstants.MOVE_ACTION_DELETE;
        }

        return isValid;
    }

    public void revertMove()
    {
        switch (lastMove)
        {
            case MotionCaptureConstants.MOVE_ACTION_ADD:
                Debug.Log("MOVE: ADD MARKER FAILED");

                GameObject.Destroy(config[this.config.Count - 1].MarkerInstance);
                config.RemoveAt(this.config.Count - 1);

                break;
            case MotionCaptureConstants.MOVE_ACTION_MODIFY:
                Debug.Log("MOVE: MODIFY MARKER FAILED");

                this.config[selectedMarker].Position = lastMarkerPosition;
                this.config[selectedMarker].MarkerInstance.transform.position = this.currentInstance.transform.TransformPoint(this.config[selectedMarker].Position);
                
                break;
            case MotionCaptureConstants.MOVE_ACTION_DELETE:
                Debug.Log("MOVE: DELETE MARKER FAILED");

                Vector3 markerPosition = this.currentInstance.transform.TransformPoint(lastMarkerPosition);

                GameObject currentMarker = Optimizer.InstanceMarker(markerPosition);
                currentMarker.transform.parent = this.currentInstance.transform;

                config.Insert(selectedMarker, new Marker(lastMarkerPosition, currentMarker));

                break;
        }
    }

    public void placeMarkets(int numberOfMarkers, Vector3[] vertices)
    {
        for (int i = 0; i < numberOfMarkers; i++)
        {
            addMarker(vertices, 100);
        }
    }
    
    public void evaluateConfig(List<GameObject> cameras, List<Vector2> cameraPair)
    {
        float currentScore = 0.0f; 

        for (int i = 0; i < this.config.Count; i++)
        {
            for (int j = 0; j < cameraPair.Count; j++)
            {
                RaycastHit objectHit;

                int camera1 = (int)cameraPair[j].x;
                int camera2 = (int)cameraPair[j].y;

                Ray camera1Ray = cameras[camera1].GetComponent<Camera>().ScreenPointToRay(cameras[camera1].GetComponent<Camera>().WorldToScreenPoint(this.config[i].currentPosition()));
                Ray camera2Ray = cameras[camera2].GetComponent<Camera>().ScreenPointToRay(cameras[camera2].GetComponent<Camera>().WorldToScreenPoint(this.config[i].currentPosition()));

                Debug.DrawLine(camera1Ray.origin, camera1Ray.direction * 500, laserColor[j]);
                Debug.DrawLine(camera2Ray.origin, camera2Ray.direction * 500, laserColor[j]);

                if (Physics.Raycast(camera1Ray, out objectHit, 500, layerMask) && Physics.Raycast(camera2Ray, out objectHit, 500, layerMask))
                {
                    if (this.config[i].isMe(objectHit.transform.gameObject))
                    {
                        this.config[i].Score += 1;
                    }
                }
            }

            currentScore += (this.config[i].Score / cameraPair.Count);
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

    float alphaGaussian = 1.0f;
    
    public float getMarkerCost(int targetMarker)
    {
        return Mathf.Exp(-(1 / (2 * Mathf.Pow(alphaGaussian, 2))) * Mathf.Pow((float)(this.config.Count - targetMarker), 2));
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
