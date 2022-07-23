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
    public GameObject[] initialPattern;
    public Vector3[] transformations;
    public int k;

    //Terms
    [Range(0.0f, 1.0f)]
    public float targetVisibility;
    [Range(0.0f, 1.0f)]
    public float targetOverlap;
    [Range(1, 20)]
    public int targetMarkerNumber;
    [Range(0.0f, 1.0f)]
    public int targetSymmetry;

    //Weight
    [Range(0.0f, 1.0f)]
    public float weightVisibility;
    [Range(0.0f, 1.0f)]
    public float weightOverlap;
    [Range(0.0f, 1.0f)]
    public float weightMarkerNumber;
    [Range(0.0f, 1.0f)]
    public float weightSymmetry;

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
    private bool complete = false;
    private bool isAccepted = false;
    private bool initialEvaluation = true;
    private int lastIteration;
    private List<Vector2> cameraPair;
    private bool continueIteration = true;
    private float selectedScore;
    private PatternMatchHelper patternHelper;
    private int currentMove;

    #endregion

    #region MonoBehaviour Callbacks

    void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 30;
    }

    // Start is called before the first frame update
    void Start()
    {
        patternHelper = new PatternMatchHelper(initialPattern, transformations);

        markerInstace = Resources.Load<GameObject>("Prefabs/Marker");
        movements = new List<string>();
        cameraPair = new List<Vector2>();
        sphereRadius = markerInstace.GetComponent<SphereCollider>().radius * 0.01f;

        //Define movements ADD: 0, MODIFY: 1, DELETE: 2
        movements.Add(MotionCaptureConstants.MOVE_ACTION_ADD);
        movements.Add(MotionCaptureConstants.MOVE_ACTION_MODIFY);
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

        InstanceMesh();

        initialTime = Time.time;

        selectCameraPair();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!complete && continueIteration)
        {
            tempConfig.evaluateConfig(cameras, cameraPair);
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

            if (posI > maxAxis[0] && posK >= minAxis[2] +  separation[2])
            {
                //continueIteration = false;

                if (isCompleted())
                {
                    placeBestSolution();
                    return;
                }

                configurationScores.Add(calculateCost(tempConfig));

                posI = minAxis[0];
                posJ = minAxis[1];
                posK = minAxis[2];

                tempConfig.changePosition(new Vector3(posI, posJ, posK));
                tempConfig.resetConfig();

                if (!initialEvaluation)
                {
                    float prob = Random.Range(0.0f, 1.0f);

                    float boltz = metropolisCriterion(selectedScore, configurationScores[currentIteration]);

                    Debug.Log("Current Acceptance interval " + boltz);

                    if (prob < boltz)
                    {
                        isAccepted = true;
                    }

                    if (configurationScores[currentIteration] < selectedScore || isAccepted == true)
                    {
                        acceptSolution();
                    }
                    else
                    {
                        tempConfig.revertMove();
                        nextMove();
                    }
                }
                else
                {
                    initialEvaluation = false;
                    bestoScores.Add(configurationScores[currentIteration]);
                    selectedScore = configurationScores[currentIteration];
                    nextMove();
                }

                currentIteration++;
                validateAcceptanceInterval();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown("space") && complete)
        {
            currentCamera++;

            if (currentCamera == cameras.Count)
            {
                currentCamera = 0;
            }

            SwapCamera();
        }

        if (Input.GetKeyDown("m"))
        {
            continueIteration = true;        
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

    private void selectCameraPair()
    {
        List<int> selectedCamera = new List<int>();

        for(int i = 0; i < cameras.Count; i++)
        {
            selectedCamera.Add(i);

            for (int j = cameras.Count - 1; j >= 0; j--)
            {
                if(i != j && !selectedCamera.Contains(j))
                {
                    if (isParallel(cameras[i].GetComponent<Camera>().transform.forward, cameras[j].GetComponent<Camera>().transform.forward))
                    {
                        Debug.Log("Camera parallel dir -> " + i + " - " + j);
                    } else
                    {
                        selectedCamera.Add(j);
                        cameraPair.Add(new Vector2(i, j));
                        Debug.Log("Camera pair -> " + cameraPair[cameraPair.Count - 1]);
                        break;
                    }
                }
                    
            }
        }

    }

    private bool isParallel(Vector3 a, Vector3 b)
    {
        float ratioX = (a.x / b.x);
        float ratioY = (a.y / b.y);
        float ratioZ = (a.z / b.z);

        return  ratioX == ratioY && ratioY == ratioZ;
    }

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
        bool isvalidMove = false;

        while (!isvalidMove)
        {
            float nextMoveSelector = Random.Range(0.0f, 1.0f);

            if (nextMoveSelector < MotionCaptureConstants.MOVE_ACTION_ADD_PROBABILITY) //0 40
            {
                currentMove = 0;
            }
            else
            {
                if (nextMoveSelector < (MotionCaptureConstants.MOVE_ACTION_ADD_PROBABILITY + MotionCaptureConstants.MOVE_ACTION_MODIFY_PROBABILITY))
                {
                    currentMove = 1;
                }
                else
                {
                    currentMove = 2;
                }
            }

            switch ((string)movements[currentMove])
            {
                case MotionCaptureConstants.MOVE_ACTION_ADD:

                    isvalidMove = tempConfig.addMarker(meshVertices, MAX_NUMBER_OF_MARKERS);
                    
                    if (isvalidMove)
                    {
                        Debug.Log("MOVE: ADD MARKER");
                    }

                    break;
                case MotionCaptureConstants.MOVE_ACTION_MODIFY:

                    isvalidMove = tempConfig.relocateMarker(meshVertices);
                    if (isvalidMove)
                    {
                        Debug.Log("MOVE: MODIFY MARKER");
                    }

                    break;
                case MotionCaptureConstants.MOVE_ACTION_DELETE:

                    isvalidMove = tempConfig.deleteMarker(MIN_NUMBER_OF_MARKERS);
                    if (isvalidMove)
                    {
                        Debug.Log("MOVE: DELETE MARKER");
                    }

                    break;
            }
        }

    }

    private float calculateCost(MarkerConfig markerConfig)
    {
        float costVisibility = Mathf.Abs(markerConfig.getScore(totalPositions) - targetVisibility);
        float costOverlap = Mathf.Abs(markerConfig.getOverlap(sphereRadius) - targetOverlap);
        float costMarkerNumber = 1 - markerConfig.getMarkerCost(targetMarkerNumber);
        float costSymmetry = Mathf.Abs(markerConfig.getSymmetry(k));

        if (costSymmetry == 1)
            Debug.Log("There is a Symmetry");

        // patternHelper.patternDiameter = patternHelper.diameter(this.currentConfig.MarkerList);
        // patternHelper.pattern = this.currentConfig.MarkerList;

        // float costPatternMatch = Mathf.Abs(patternHelper.calculatePatternCost() - targetOverlap); ;

        float totalCost = weightVisibility * costVisibility + weightOverlap * costOverlap + weightMarkerNumber * costMarkerNumber + weightSymmetry * costSymmetry;

        Debug.Log("Iteration " + currentIteration + " -> Cost: " + totalCost + ", Visibility: " + costVisibility + " Overlap: " + costOverlap + " # Marker cost: " + costMarkerNumber + " # Symmetry cost: " + costSymmetry + " # Markers: " + markerConfig.Config.Count);

        return totalCost;
    }

    private void validateAcceptanceInterval()
    {
        switch (currentIteration)
        {
            case 150:
                temperature = 0.75f;
                break;
            case 300:
                temperature = 0.5f;
                break;
            case 450:
                temperature = 0.25f;
                break;
            case 600:
                temperature = 0.002f;
                break;
        }
    }

    private void acceptSolution()
    {
        isAccepted = false;
        lastIteration = currentIteration;
        currentConfig = copyMarkerConfig(tempConfig);
        BestConfig = currentIteration;
        selectedScore = configurationScores[currentIteration];
        bestoScores.Add(selectedScore);

        Debug.Log("Iteration "+ currentIteration + "Accepted solutions " + bestoScores.Count + " current cost -> " + configurationScores[currentIteration]);

        nextMove();
    }

    private void placeBestSolution()
    {
        complete = true;

        tempConfig.clearConfig();
        currentConfig.changePosition(new Vector3(0.0f, minAxis[1], 0.0f));
        currentConfig.resetConfigToCurrent();

        OptimizerReportController.reportCostLog(bestoScores);
        
        Debug.Log("BEST CONFIG: ");
        calculateCost(currentConfig);
        Debug.Log("TOTAL TIME: " + (Time.time - initialTime) + " SEG");
    }

    private bool isCompleted()
    {
        bool isCompleted = true; 
        
        if(bestoScores.Count > 20 && temperature < 0.1)
        {
            int lastCosts = bestoScores.Count - 20;
            for (int i = 0; i < 20; i++)
            {
                if (Mathf.Abs((bestoScores[bestoScores.Count - 1] - bestoScores[lastCosts + i]) / bestoScores[bestoScores.Count - 1]) > 0.05f)
                {
                    isCompleted = false;
                }
            }
        } else
        {
            isCompleted = false;
        }

        if (currentIteration > (lastIteration + 100) && temperature < 0.1)
        {
            isCompleted = true;
        }

        return isCompleted;
    }

    private float BoltzmannOF(float totalCost)
    {
        return Mathf.Exp((-1.0f * totalCost) / temperature);
    }

    private float metropolisCriterion(float currentCost, float proposedCost)
    {
        float f = BoltzmannOF(currentCost);
        float fa = BoltzmannOF(proposedCost);

        switch ((string)movements[currentMove])
        {
            case MotionCaptureConstants.MOVE_ACTION_ADD:
                return Mathf.Min(1.0f, (MotionCaptureConstants.MOVE_ACTION_DELETE_PROBABILITY / MotionCaptureConstants.MOVE_ACTION_ADD_PROBABILITY) * (fa / f));
                break;
            case MotionCaptureConstants.MOVE_ACTION_MODIFY:
                return Mathf.Min(1.0f, (fa / f));
                break;
            case MotionCaptureConstants.MOVE_ACTION_DELETE:
                return Mathf.Min(1.0f, (MotionCaptureConstants.MOVE_ACTION_ADD_PROBABILITY / MotionCaptureConstants.MOVE_ACTION_DELETE_PROBABILITY) * (fa / f));
                break;
        }

        return 0.0f;
    }

    #endregion
}
