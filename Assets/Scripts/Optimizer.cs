using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Optimizer : MonoBehaviour
{

    #region  Public Fields

    public int numberOfMarkers;
    public int iterations;
    public float evaluatePositions;
    public List<GameObject> cameras;
    public GameObject motionMesh;
    public GameObject mainCamera;

    public static GameObject markerInstace;

    #endregion

    #region  Private Fields

    private GameObject currentMotionMesh;
    private List<MarkerConfig> configurations;
    private MarkerConfig initialConfig;
    private int BestConfig = 0;
    private float MAX_SCORE = -1;
    private int currentIteration = 0;
    private int currentCamera = 0;
    private bool complete = false;
    private Vector3[] meshVertices;
    private float posI;
    private float posJ;
    private float posK;
    private float initialTime;
    private float[] separation = { 0.0f, 0.0f, 0.0f };
    private float[] roomDimensions = { 4.0f, 2.0f, 4.0f };
    private float[] minAxis = { -2.0f, 0.6f, -2.0f };
    private float[] maxAxis = { 0.0f, 0.0f, 0.0f };
    private float totalPositions = 1.0f;

    #endregion

    #region MonoBehaviour Callbacks

    // Start is called before the first frame update
    void Start()
    {
        markerInstace = Resources.Load<GameObject>("Prefabs/Marker");

        configurations = new List<MarkerConfig>();

        posI = minAxis[0];
        posJ = minAxis[1];
        posK = minAxis[2];

        for(int i = 0; i < 3; i++)
        {
            maxAxis[i] = minAxis[i] + roomDimensions[i];
            separation[i] = (maxAxis[i] - minAxis[i]) / (evaluatePositions - 1);
            totalPositions *= evaluatePositions;
        }

        InstanceMesh();

        initialTime = Time.time;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!complete)
        {
            if (currentIteration < iterations)
            {
                configurations[currentIteration].evaluateConfig(cameras);

                configurations[currentIteration].changePosition(new Vector3(posI, posJ, posK));
                configurations[currentIteration].resetConfig();

                posK += separation[2];

                if (posK > maxAxis[2])
                {
                    posJ += separation[1];
                    posK = minAxis[2];

                    if (posJ > maxAxis[1])
                    {
                        posI += separation[0];
                        posJ = minAxis[1];
                    }
                }

                if (posI > maxAxis[0])
                {
                    Debug.Log(configurations[currentIteration].showScore(currentIteration, totalPositions));

                    if (configurations[currentIteration].Score > MAX_SCORE)
                    {
                        BestConfig = currentIteration;
                        MAX_SCORE = configurations[currentIteration].Score;
                    }

                    posI = minAxis[0];
                    posJ = minAxis[1];
                    posK = minAxis[2];

                    configurations.Add(nextMarkerConfig(configurations[currentIteration]));
                    currentIteration++;
                }
            }
            else
            {
                Debug.Log("BEST CONFIG: " + configurations[BestConfig].showScore(BestConfig, totalPositions));
                Debug.Log("TOTAL TIME: " + (Time.time - initialTime) + " SEG");
                configurations[BestConfig].changePosition(new Vector3(0.0f, minAxis[1], 0.0f));
                configurations[BestConfig].resetConfig();
                complete = true;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("space")) //&& complete)
        {
            currentCamera++;

            if (currentCamera == cameras.Count)
            {
                currentCamera = 0;
            }

            SwapCamera();
        }
    }

    #endregion

    #region Public Methods

    public static GameObject InstanceMarker(Vector3 position)
    {
        return Instantiate(markerInstace, position, Quaternion.identity);
    }

    #endregion

    #region Private Methods

    private void SwapCamera()
    {
        mainCamera.SetActive(true);

        foreach (GameObject c in cameras)
        {
            c.SetActive(false);
        }

        cameras[currentCamera].SetActive(true);
        mainCamera.SetActive(false);
    }

    private void InstanceMesh()
    {
        if (currentIteration < iterations)
        {
            currentMotionMesh = Instantiate(motionMesh, new Vector3(posI, posJ, posK), Quaternion.identity);

            MeshFilter currentMesh = currentMotionMesh.transform.GetChild(1).GetComponentInChildren<MeshFilter>();
            meshVertices = currentMesh.mesh.vertices;

            initialConfig = defineMarkerConfig();
        }
    }

    private MarkerConfig defineMarkerConfig()
    {
        MarkerConfig config = new MarkerConfig(new Vector3(posI, posJ, posK), currentMotionMesh);

        config.placeMarkets(numberOfMarkers, meshVertices);

        configurations.Add(config);

        return config;
    }

    private MarkerConfig nextMarkerConfig(MarkerConfig currentConfig)
    {
        return new MarkerConfig(currentConfig, new Vector3(posI, posJ, posK), meshVertices);
    }

    #endregion
}
