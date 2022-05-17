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
    public int MAX_NUMBER_OF_MARKERS = 10;
    public int MIN_NUMBER_OF_MARKERS = 4;

    //Terms
    [Range(0.0f, 1.0f)]
    public float targetVisibility;
    [Range(0.0f, 1.0f)]
    public float targetOverlap;

    //Weight
    [Range(0.0f, 1.0f)]
    public float weightVisibility;
    [Range(0.0f, 1.0f)]
    public float weightOverlap;

    public static GameObject markerInstace;

    #endregion

    #region  Private Fields

    private GameObject currentMotionMesh;
    private List<float> configurationScores;
    private List<float> bestoScores;
    private List<string> movements;
    private MarkerConfig tempConfig;
    private MarkerConfig currentConfig;
    private int BestConfig = 0;
    private int currentIteration = 0;
    private int currentCamera = 0;
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
    private float sphereRadius;
    private float temperature = 1.0f;
    private float currentAcceptanceInterval = 1.0f;
    private bool complete = false;
    private bool isAccepted = false;
    private bool initialEvaluation = true;
    private int lastIteration;

    #endregion

    #region MonoBehaviour Callbacks

    void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;
    }

    // Start is called before the first frame update
    void Start()
    {
        markerInstace = Resources.Load<GameObject>("Prefabs/Marker");
        movements = new List<string>();
        sphereRadius = markerInstace.GetComponent<SphereCollider>().radius * 0.01f;

        //Define movements
        movements.Add(MotionCaptureConstants.MOVE_ACTION_ADD);
        movements.Add(MotionCaptureConstants.MOVE_ACTION_RELOCATE);
        movements.Add(MotionCaptureConstants.MOVE_ACTION_DELETE);

        configurationScores = new List<float>();
        bestoScores = new List<float>();

        posI = minAxis[0];
        posJ = minAxis[1];
        posK = minAxis[2];

        for(int i = 0; i < 3; i++)
        {
            maxAxis[i] = minAxis[i] + roomDimensions[i];
            separation[i] = (maxAxis[i] - minAxis[i]) / (evaluatePositions - 1);
            totalPositions *= evaluatePositions;
        }

        Debug.Log("Positions ss" + totalPositions);

        InstanceMesh();

        initialTime = Time.time;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!complete)
        {
            tempConfig.evaluateConfig(cameras);

            tempConfig.changePosition(new Vector3(posI, posJ, posK));
            tempConfig.resetConfig();

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
                configurationScores.Add(calculateCost(tempConfig));

                posI = minAxis[0];
                posJ = minAxis[1];
                posK = minAxis[2];

                if (!initialEvaluation)
                {
                    float prob = Random.Range(0.0f, 1.0f);

                    //currentAcceptanceInterval = Mathf.Exp(-(configurationScores[currentIteration] - configurationScores[BestConfig]) / temperature);

                    if (prob < temperature)
                    {
                        isAccepted = true;
                    }

                    if ((configurationScores[currentIteration] < configurationScores[BestConfig]) || isAccepted)
                    {
                        isAccepted = false;
                        lastIteration = currentIteration;
                        BestConfig = configurationScores.Count - 1;
                        currentConfig = copyMarkerConfig(tempConfig);
                        bestoScores.Add(configurationScores[currentIteration]);
                        Debug.Log("Accepted solutions " + bestoScores.Count);
                        Debug.Log("Iteration " + currentIteration + " -> Cost: " + configurationScores[BestConfig]);

                    }
                    else
                    {
                        if (temperature == 0)
                        {
                            if (isCompleted())
                            {
                                tempConfig.clearConfig();
                                currentConfig.changePosition(new Vector3(0.0f, minAxis[1], 0.0f));
                                currentConfig.resetConfigToCurrent();
                                OptimizerReportController.reportCostLog(bestoScores);
                                Debug.Log("BEST CONFIG: ");
                                //calculateCost(currentConfig);
                                Debug.Log("TOTAL TIME: " + (Time.time - initialTime) + " SEG");
                                complete = true;
                            }
                        } 
                    }

                    nextMove();
                }
                else
                {
                    initialEvaluation = false;
                    nextMove();
                }

                currentIteration++;
                validateAcceptanceInterval();
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

            tempConfig = defineMarkerConfig();
            currentConfig = copyMarkerConfig(tempConfig);
        }
    }

    private MarkerConfig defineMarkerConfig()
    {
        MarkerConfig config = new MarkerConfig(new Vector3(posI, posJ, posK), currentMotionMesh);

        config.placeMarkets(numberOfMarkers, meshVertices);

        return config;
    }

    private MarkerConfig copyMarkerConfig(MarkerConfig currentConfig)
    {
        return new MarkerConfig(currentConfig, new Vector3(posI, posJ, posK));
    }

    private void nextMove()
    {
        tempConfig.Score = 0.0f;
        int move = -1;
        bool isvalidMove = false;

        while (!isvalidMove)
        {
            float nextMoveSelector = Random.Range(0.0f, 1.0f);

            if (nextMoveSelector < 0.3)
            {
                move = 0;
            }
            else
            {
                if (nextMoveSelector < 0.6)
                {
                    move = 2;
                }
                else
                {
                    move = 1;
                }
            }

            switch ((string)movements[move])
            {
                case MotionCaptureConstants.MOVE_ACTION_ADD:

                    isvalidMove = tempConfig.addMarker(meshVertices, MAX_NUMBER_OF_MARKERS);
                    if (isvalidMove)
                    {
                        Debug.Log("MOVE: ADD MARKER");
                    }
                    else
                    {
                        Debug.Log("MOVE: ADD MARKER FAILED");
                    }
                    break;
                case MotionCaptureConstants.MOVE_ACTION_RELOCATE:

                    isvalidMove = tempConfig.relocateMarker(meshVertices);
                    if (isvalidMove)
                    {
                        Debug.Log("MOVE: RELOCATE MARKER");
                    }
                    else
                    {
                        Debug.Log("MOVE: RELOCATE MARKER FAILED");
                    }

                    break;
                case MotionCaptureConstants.MOVE_ACTION_DELETE:

                    isvalidMove = tempConfig.deleteMarker(MIN_NUMBER_OF_MARKERS);
                    if (isvalidMove)
                    {
                        Debug.Log("MOVE: DELETE MARKER");
                    }
                    else
                    {
                        Debug.Log("MOVE: DELETE MARKER FAILED");
                    }

                    break;
            }
        }

    }

    private float calculateCost(MarkerConfig markerConfig)
    {
        float costVisibility = Mathf.Abs(markerConfig.getScore(totalPositions) - targetVisibility);
        float costOverlap = Mathf.Abs(markerConfig.getOverlap(sphereRadius) - targetOverlap);

        float totalCost = costVisibility * weightVisibility + costOverlap * weightOverlap;

        //Debug.Log("Iteration " + currentIteration + " -> Cost: " + totalCost + ", Visibility: " + costVisibility + " Overlap: " + costOverlap + " Number of Markers: " + markerConfig.Config.Count);

        return totalCost;
    }

    private void validateAcceptanceInterval()
    {
        if (currentIteration == 50)
        {
            temperature = 0.75f;
        } else
        {
            if (currentIteration == 100)
            {
                temperature = 0.50f;
            }
            else
            {
                if (currentIteration == 150)
                {
                    temperature = 0.25f;
                }
                else
                {
                    if (currentIteration == 200)
                    {
                        temperature = 0.0f;
                    }
                }
            }
        }
    }

    private bool isCompleted()
    {
        bool isCompleted = true; 
        
        if(bestoScores.Count > 2)
        {
            if (Mathf.Abs((bestoScores[bestoScores.Count - 1] - bestoScores[bestoScores.Count - 2]) / bestoScores[bestoScores.Count - 1]) > 0.05f)
            {
                isCompleted = false;
            }
        } else
        {
            isCompleted = false;
        }

        if (currentIteration > (lastIteration + 100))
        {
            isCompleted = true;
        }

        return isCompleted;
    }

    #endregion
}
