using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptimizerIterations : MonoBehaviour
{
    public int numberOfMarkers;
    public List<GameObject> cameras;

    private GameObject motionMesh;
    private GameObject currentMotionMesh;
    private GameObject marker;
    private Vector3[] vertices;
    private float minY = 0.4f;
    private List<Marker> markers;

    // Start is called before the first frame update
    void Start()
    {
        markers = new List<Marker>();

        motionMesh = Resources.Load<GameObject>("Prefabs/MotionMesh");
        marker = Resources.Load<GameObject>("Prefabs/Marker");
        InstanceMesh(new Vector3(0.0f, minY, 0.0f));
        placeMarkets();
        evaluateConfig();
    }

    void InstanceMesh(Vector3 position)
    {
        currentMotionMesh = Instantiate(motionMesh, position, Quaternion.identity);

        MeshFilter currentMesh = currentMotionMesh.transform.GetChild(1).GetComponentInChildren<MeshFilter>();

        vertices = currentMesh.mesh.vertices;
    }

    void placeMarkets()
    {
        for(int i = 0; i < numberOfMarkers; i++)
        {
            int randomIndex = (int)UnityEngine.Random.Range(0, vertices.Length - 1);

            Vector3 markerPosition = currentMotionMesh.transform.TransformPoint(vertices[randomIndex]);

            GameObject currentMarker = Instantiate(marker, markerPosition, Quaternion.identity);
            currentMarker.transform.parent = currentMotionMesh.transform;

            markers.Add(new Marker(markerPosition, currentMarker));
        }
    }
    
    void evaluateConfig()
    {
        for (int i = 0; i < numberOfMarkers; i++)
        {
            for (int j = 0; j < cameras.Count; j++)
            {
                RaycastHit objectHit;
                // Shoot raycast
                if (Physics.Linecast(cameras[j].transform.position, markers[i].Position, out objectHit))
                {
                    if (markers[i].isMe(objectHit.transform.gameObject)) 
                    {
                        markers[i].Score += 1;
                    }
                }
            }

            Debug.Log("INSTACE 1 - MARKER" + i + " TOTAL SCORE: " + markers[i].Score);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
