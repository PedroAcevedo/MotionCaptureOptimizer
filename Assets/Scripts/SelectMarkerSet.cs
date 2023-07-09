using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SelectMarkerSet : MonoBehaviour
{
    public GameObject[] props;
    public int numberOfMarkers;
    public List<GameObject> cameras;
    public GameObject mainCamera;
    public float evaluatePositions;
    public int k;

    //Terms
    [Range(0.0f, 1.0f)]
    public float targetVisibility;
    [Range(0.0f, 1.0f)]
    public float targetOverlap;
    [Range(1, 20)]
    public int targetMarkerNumber;
    [Range(0.0f, 1.0f)]
    public float targetSymmetry;

    //Weight
    [Range(0.0f, 1.0f)]
    public float weightVisibility;
    [Range(0.0f, 1.0f)]
    public float weightOverlap;
    [Range(0.0f, 1.0f)]
    public float weightMarkerNumber;
    [Range(0.0f, 1.0f)]
    public float weightSymmetry;

    private int currentCamera = 0;

    private float[] minAxis = { -2.0f, 0.6f, -2.0f };
    private float[] initialPropPosition = { 0.0f, 0.75f, 0.0f };
    private float[] roomDimensions = { 4.0f, 2.0f, 4.0f };
    private float[] maxAxis = { 0.0f, 0.0f, 0.0f };
    private float[] separation = { 0.0f, 0.0f, 0.0f };

    private float totalPositions = 1.0f;
    private float sphereRadius;
    private GameObject currentMotionMesh;
    private MarkerConfig tempConfig;
    private MarkerConfig currentConfig;
    private List<Vector2> cameraPair;
    private int currentProp = 0;
    private float posI;
    private float posJ;
    private float posK;

    private List<float> configCost;

    private Vector3[] meshVertices;
    private bool evalConfig = false;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 30;
    }

    // Start is called before the first frame update
    void Start()
    {
        cameraPair = new List<Vector2>();
        configCost = new List<float>();

        Optimizer.markerInstace = Resources.Load<GameObject>("Prefabs/Marker");

        for (int i = 0; i < 3; i++)
        {
            maxAxis[i] = minAxis[i] + roomDimensions[i];
            separation[i] = (maxAxis[i] - minAxis[i]) / (evaluatePositions - 1);
            totalPositions *= evaluatePositions;
        }

        for (int i = 0; i < props.Length; i++)
        {
            configCost.Add(0.0f);
        }

        PlaceObject();

        InstanceMesh();

        selectCameraPair();
    }

    // Update is called once per frame
    void Update()
    {
        if (!evalConfig)
        {
            if (Input.GetKeyDown("space"))
            {
                currentCamera++;

                if (currentCamera == cameras.Count)
                {
                    currentCamera = 0;
                }

                SwapCamera();
            }

            if (Input.GetKeyDown("z"))
            {
                cameras[currentCamera].SetActive(false);
                currentCamera = 0;
                mainCamera.SetActive(true);
            }

            if (Input.GetKeyDown("s"))
            {
                Debug.Log("EXPORT PREFAB");

                PrefabUtility.SaveAsPrefabAsset(currentConfig.CurrentInstance, "Assets/Resources/OptimizedMeshes/Expert/" + currentConfig.CurrentInstance.name.Split('(')[0] + "_expert_optimized.prefab");
            }

            if (Input.GetKeyDown("o"))
            {
                Debug.Log("RUN COST FUNCTION");
                InitCostEvaluation();
            }

            if (Input.GetKeyDown("x"))
            {
                ChangeMarkerConfig();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) && currentProp < (props.Length - 1))
            {
                currentProp++;
                nextObject();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) && currentProp > 0)
            {
                currentProp--;
                nextObject();
            }

            if (Input.GetKeyDown("r"))
            {
                OptimizerReportController.reportExpertProp(configCost, props);
            }
        }
    }

    void LateUpdate()
    {
        if (evalConfig)
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

            if (posI > maxAxis[0] && posK >= minAxis[2] + separation[2])
            {
                configCost[currentProp] = calculateCost(tempConfig);

                PlaceObject();

                tempConfig.changePosition(new Vector3(posI, posJ, posK));
                tempConfig.resetConfig();
                tempConfig.Score = 0;

                evalConfig = false;
            }
        }
    }

    private void InitCostEvaluation()
    {
        sphereRadius = Optimizer.markerInstace.GetComponent<SphereCollider>().radius * 0.02f;

        posI = minAxis[0];
        posJ = minAxis[1];
        posK = minAxis[2];

        evalConfig = true;
    }

    private void PlaceObject()
    {
        posI = initialPropPosition[0];
        posJ = initialPropPosition[1];
        posK = initialPropPosition[2];
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
        currentMotionMesh = Instantiate(props[currentProp], new Vector3(posI, posJ, posK), Quaternion.identity);

        MeshFilter currentMesh = currentMotionMesh.transform.GetChild(1).GetComponentInChildren<MeshFilter>();
        meshVertices = currentMesh.mesh.vertices;

        tempConfig = defineMarkerConfig();
        currentConfig = copyMarkerConfig(tempConfig);
    }

    private void ChangeMarkerConfig()
    {
        currentConfig.clearConfig();
        tempConfig = defineMarkerConfig();
        currentConfig = copyMarkerConfig(tempConfig);
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

    private float calculateCost(MarkerConfig markerConfig)
    {
        float costVisibility = Mathf.Abs(markerConfig.getScore(totalPositions) - targetVisibility);
        float costOverlap = Mathf.Abs(markerConfig.getOverlap(sphereRadius) - targetOverlap);
        float costMarkerNumber = 1 - markerConfig.getMarkerCost(targetMarkerNumber);
        float costSymmetry = Mathf.Abs(markerConfig.getSymmetry(k) - targetSymmetry);

        if (costSymmetry == 1)
            Debug.Log("There is a Symmetry");

        float totalCost = weightVisibility * costVisibility + weightOverlap * costOverlap + weightMarkerNumber * costMarkerNumber + weightSymmetry * costSymmetry;

        Debug.Log("Iteration " + 0 + " -> Cost: " + totalCost + ", Visibility: " + costVisibility + " Overlap: " + costOverlap + " # Marker cost: " + costMarkerNumber + " # Symmetry cost: " + costSymmetry + " # Markers: " + markerConfig.Config.Count);

        return totalCost;
    }

    private void nextObject()
    {
        Destroy(currentConfig.CurrentInstance);
        tempConfig = null;
        currentConfig = null;

        InstanceMesh();
    }

    private void selectCameraPair()
    {
        List<int> selectedCamera = new List<int>();

        for (int i = 0; i < cameras.Count; i++)
        {
            selectedCamera.Add(i);

            for (int j = cameras.Count - 1; j >= 0; j--)
            {
                if (i != j && !selectedCamera.Contains(j))
                {
                    if (isParallel(cameras[i].GetComponent<Camera>().transform.forward, cameras[j].GetComponent<Camera>().transform.forward))
                    {
                        Debug.Log("Camera parallel dir -> " + i + " - " + j);
                    }
                    else
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

        return ratioX == ratioY && ratioY == ratioZ;
    }
}