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
    private float minY = 0.4f;
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
    private float separation;

    #endregion

    #region MonoBehaviour Callbacks

    // Start is called before the first frame update
    void Start()
    {
        markerInstace = Resources.Load<GameObject>("Prefabs/Marker");

        configurations = new List<MarkerConfig>();
        
        posI = -2.0f;
        posJ = minY;
        posK = -2.0f;

        separation = 4 / evaluatePositions;

        InstanceMesh();

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

                posK += separation;

                if (posK > 2)
                {
                    posJ += separation;
                    posK = -2;

                    if (posJ > 2)
                    {
                        posI += separation;
                        posJ = minY;
                    }
                }

                if (posI > 2)
                {
                    Debug.Log(configurations[currentIteration].showScore(currentIteration));

                    if (configurations[currentIteration].Score > MAX_SCORE)
                    {
                        BestConfig = currentIteration;
                        MAX_SCORE = configurations[currentIteration].Score;
                    }

                    configurations[currentIteration].CurrentInstance.SetActive(false);
                    currentIteration++;
                    posI = -2.0f;
                    posJ = minY;
                    posK = -2.0f;
                    InstanceMesh();
                }

            }
            else
            {
                Debug.Log("BEST CONFIG " + configurations[BestConfig].showScore(BestConfig));
                configurations[BestConfig].CurrentInstance.SetActive(true);
                configurations[BestConfig].changePosition(new Vector3(0.0f, minY, 0.0f));
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

    private Vector3 getRandomPosition()
    {
        return new Vector3(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(minY, 2), UnityEngine.Random.Range(-2, 2));
    }

    #endregion
}
