using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject meshObject = Resources.Load<GameObject>("Prefabs/suzane_and_poisson");
        MeshFilter myMesh = meshObject.GetComponent<MeshFilter>();

        Debug.Log(myMesh.mesh.vertices);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
