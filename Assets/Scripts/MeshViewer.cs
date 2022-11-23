using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshViewer : MonoBehaviour
{
    public List<GameObject> cameras;
    public GameObject mainCamera;
    public List<GameObject> meshes;

    private int currentCamera = 0;
    private int currentMesh = 0;

    void Start()
    {
        meshes[0].SetActive(true);
    }
    // Update is called once per frame
    void Update()
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

        if (Input.GetKeyDown(KeyCode.RightArrow) && currentMesh < (meshes.Count - 1))
        {
            meshes[currentMesh].SetActive(false);
            currentMesh++;
            meshes[currentMesh].SetActive(true);

        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentMesh > 0)
        {
            meshes[currentMesh].SetActive(false);
            currentMesh--;
            meshes[currentMesh].SetActive(true);
        }
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
}
