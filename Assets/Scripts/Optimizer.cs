using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

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
    public GameObject[] props;
    public GameObject[] constrainedProps;
    public int evaluatedObjects;
    public string evaluationLabel;
    public string folder;
    public int testNumber;

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
    private List<float> iterationCost;
    private List<float> bestScores;
    private List<float> optimalScores;
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
    private GameObject floor;
    private float prevBotz;
    private float currentBotz;

    private int currentProp = 0;
    private float iterationTime = 0;
    private float OneIterationTime = 0;

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
        sphereRadius = markerInstace.GetComponent<SphereCollider>().radius * 0.02f;
        floor = GameObject.Find("Floor");

        //Define movements ADD: 0, MODIFY: 1, DELETE: 2
        movements.Add(MotionCaptureConstants.MOVE_ACTION_ADD);
        movements.Add(MotionCaptureConstants.MOVE_ACTION_MODIFY);
        movements.Add(MotionCaptureConstants.MOVE_ACTION_DELETE);

        configurationScores = new List<float>();
        iterationCost = new List<float>();
        bestScores = new List<float>(); 
        optimalScores = new List<float>(); 
        
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

        OneIterationTime = Time.time;
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
                iterationCost.Add(configurationScores[configurationScores.Count - 1]);

                posI = minAxis[0];
                posJ = minAxis[1];
                posK = minAxis[2];

                tempConfig.changePosition(new Vector3(posI, posJ, posK));
                tempConfig.resetConfig();

                if (!initialEvaluation)
                {
                    float prob = Random.Range(0, 100);

                    float boltz = metropolisCriterion(selectedScore, configurationScores[currentIteration]) * 100.0f;

                    if (prob < boltz)
                    {
                        isAccepted = true;
                    }

                    if (configurationScores[currentIteration] < selectedScore)
                    {
                        acceptSolution();
                    }
                    else
                    {
                        iterationCost[iterationCost.Count - 1] = selectedScore;
                        tempConfig.revertMove();
                        nextMove();
                    }
                }
                else
                {
                    initialEvaluation = false;
                    bestScores.Add(configurationScores[currentIteration]);
                    selectedScore = configurationScores[currentIteration];
                    prevBotz = BoltzmannOF(selectedScore);
                    nextMove();
                }

                currentIteration++;
                //if(currentIteration % 50 == 0) temperature = temperature * 0.95f;
                validateAcceptanceInterval();
                OneIterationTime = Time.time - OneIterationTime;
                iterationTime += OneIterationTime;
                OneIterationTime = Time.time;
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
            currentMotionMesh = Instantiate(props[currentProp], new Vector3(posI, posJ, posK), Quaternion.identity);

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
        float nextMoveSelector = Random.Range(0.0f, 1.0f);
        
        if (nextMoveSelector < MotionCaptureConstants.MOVE_ACTION_ADD_PROBABILITY)
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
                tempConfig.addMarker(meshVertices, MAX_NUMBER_OF_MARKERS);
                Debug.Log("MOVE: ADD MARKER");
                break;
            case MotionCaptureConstants.MOVE_ACTION_MODIFY:
                tempConfig.relocateMarker(meshVertices);
                Debug.Log("MOVE: MODIFY MARKER");
                break;
            case MotionCaptureConstants.MOVE_ACTION_DELETE:
                tempConfig.deleteMarker(MIN_NUMBER_OF_MARKERS);
                Debug.Log("MOVE: DELETE MARKER");
                break;
        }


    }

    private float calculateCost(MarkerConfig markerConfig)
    {
        float costVisibility = Mathf.Abs(markerConfig.getScore(totalPositions) - targetVisibility);
        float costOverlap = Mathf.Abs(markerConfig.getOverlap(sphereRadius) - targetOverlap);
        float costMarkerNumber = 1 - markerConfig.getMarkerCost(targetMarkerNumber);
        float costSymmetry = Mathf.Abs(markerConfig.getSymmetry(k) - targetSymmetry);

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
            case 50:
                temperature = 0.75f;
                break;
            case 100:
                temperature = 0.5f;
                break;
            case 150:
                temperature = 0.25f;
                break;
            case 200:
                temperature = 0.00000000000000001f;
                break;
        }
    }

    private void acceptSolution()
    {
        isAccepted = false;
        prevBotz = currentBotz;
        lastIteration = currentIteration;
        currentConfig = copyMarkerConfig(tempConfig);
        BestConfig = currentIteration;
        selectedScore = configurationScores[currentIteration];
        bestScores.Add(selectedScore);

        Debug.Log("Iteration "+ currentIteration + " Accepted solutions " + bestScores.Count + " Temperature " + temperature + " current cost -> " + configurationScores[currentIteration]);

        nextMove();
    }

    private void placeBestSolution()
    {
        complete = true;

        tempConfig.clearConfig();

        floor.GetComponent<BoxCollider>().enabled = true;

        GameObject mesh = currentMotionMesh.transform.GetChild(0).GetChild(0).gameObject;

        currentConfig.changePosition(new Vector3(0.0f, 5.0f, 0.0f));
        currentConfig.resetConfigToCurrent();

        OptimizerReportController.reportCostLog(iterationCost, props[currentProp].name + "_" + evaluationLabel, folder, testNumber);

        Debug.Log("BEST CONFIG: " + bestScores[bestScores.Count - 1]);

        optimalScores.Add(bestScores[bestScores.Count - 1]);

        mesh.AddComponent<BoxCollider>();
        currentMotionMesh.AddComponent<Rigidbody>();

        currentConfig.changePosition(new Vector3(0.0f, 5.0f, 0.0f));
        
        Debug.Log("TOTAL TIME: " + (Time.time - initialTime) + " SEG");

        PrefabUtility.SaveAsPrefabAsset(currentConfig.CurrentInstance, "Assets/Resources/OptimizedMeshes/" + testNumber + "/" + currentConfig.CurrentInstance.name.Split('(')[0] + "_" +  evaluationLabel + "_optimized.prefab");


        Debug.Log("AVERAGE TIME PER ITERATION: " + (iterationTime / currentIteration) + " SEG");

        //nextObject();

    }

    private bool isCompleted()
    {
        bool isCompleted = true;

        if (iterationCost.Count > 200)
        {
            int lastCosts = iterationCost.Count - 50;
            for (int i = 0; i < 50; i++)
            {
                if (Mathf.Abs((iterationCost[iterationCost.Count - 1] - iterationCost[lastCosts + i]) / iterationCost[iterationCost.Count - 1]) > 0.03f)
                {
                    isCompleted = false;
                }
            }
        } else
        {
            isCompleted = false;
        }

        if (isCompleted)
        {
            Debug.Log("Completed by differences");
        }

        //if (currentIteration > (lastIteration + 20) && temperature < 0.1)
        //{
        //    Debug.Log("Completed by not found anything");

        //    isCompleted = true;
        //}

        return isCompleted;
    }

    private float BoltzmannOF(float totalCost)
    {
        return Mathf.Exp((-1.0f * totalCost) / temperature);
    }

    private float metropolisCriterion(float currentCost, float proposedCost)
    {
        currentBotz = BoltzmannOF(proposedCost);
        float f = prevBotz;
        float fa = currentBotz;

        switch ((string)movements[currentMove])
        {
            case MotionCaptureConstants.MOVE_ACTION_ADD:
                return Mathf.Min(1.0f, (MotionCaptureConstants.MOVE_ACTION_DELETE_PROBABILITY / MotionCaptureConstants.MOVE_ACTION_ADD_PROBABILITY) * ((MAX_NUMBER_OF_MARKERS - currentConfig.Config.Count) / tempConfig.Config.Count) * (fa / f));
                break;
            case MotionCaptureConstants.MOVE_ACTION_MODIFY:
                return Mathf.Min(1.0f, (fa / f));
                break;
            case MotionCaptureConstants.MOVE_ACTION_DELETE:
                return MAX_NUMBER_OF_MARKERS - tempConfig.Config.Count != 0? Mathf.Min(1.0f, (MotionCaptureConstants.MOVE_ACTION_ADD_PROBABILITY / MotionCaptureConstants.MOVE_ACTION_DELETE_PROBABILITY) * (currentConfig.Config.Count / (MAX_NUMBER_OF_MARKERS - tempConfig.Config.Count)) * (fa / f)) : 0.0f;
                break;
        }

        return 0.0f;
    }

    private void nextObject()
    {
        currentProp++;

        if(currentProp < evaluatedObjects)
        {
            complete = false;
            isAccepted = false;

            configurationScores.Clear();
            iterationCost.Clear();
            bestScores.Clear();

            posI = minAxis[0];
            posJ = minAxis[1];
            posK = minAxis[2];

            temperature = 1.0f;
            currentIteration = 0;
            initialEvaluation = true;

            Destroy(currentConfig.CurrentInstance);
            tempConfig = null;
            currentConfig = null;

            InstanceMesh();
        } else
        {
            if(currentProp == evaluatedObjects)
            {
                OptimizerReportController.reportPropsData(optimalScores, props, evaluationLabel, folder, testNumber);
            }
        }
    }
    
    #endregion
}
