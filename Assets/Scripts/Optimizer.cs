using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Optimizer : MonoBehaviour
{

    #region  Public Fields

    public int numberOfMarkers;
    public int iterations;
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

    #endregion

    #region MonoBehaviour Callbacks

    // Start is called before the first frame update
    void Start()
    {
        markerInstace = Resources.Load<GameObject>("Prefabs/Marker");

        configurations = new List<MarkerConfig>();

        InstanceMesh(getRandomPosition());

    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!complete)
        {
            if (currentIteration < iterations)
            {
                configurations[currentIteration].evaluateConfig(cameras);

                Debug.Log(configurations[currentIteration].showScore(currentIteration));

                if (configurations[currentIteration].Score > MAX_SCORE)
                {
                    BestConfig = currentIteration;
                    MAX_SCORE = configurations[currentIteration].Score;
                }

                initialConfig.changePosition(getRandomPosition());

                configurations.Add(new MarkerConfig(initialConfig));

                currentIteration++;
            }
            else
            {
                configurations[BestConfig].resetConfig();
                Debug.Log("BEST CONFIG " + BestConfig + ": " + configurations[BestConfig].Score + "% at Position: " + configurations[BestConfig].Position);

                complete = true;
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

    private void InstanceMesh(Vector3 position)
    {
        currentMotionMesh = Instantiate(motionMesh, position, Quaternion.identity);
        MeshFilter currentMesh = currentMotionMesh.transform.GetChild(1).GetComponentInChildren<MeshFilter>();

        initialConfig = new MarkerConfig(position, currentMotionMesh);

        initialConfig.placeMarkets(numberOfMarkers, currentMesh.mesh.vertices);

        configurations.Add(new MarkerConfig(initialConfig));
    }

    private Vector3 getRandomPosition()
    {
        return new Vector3(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(minY, 2), UnityEngine.Random.Range(-2, 2));
    }

    #endregion
}
