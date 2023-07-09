using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShoots : MonoBehaviour
{
    public List<GameObject> propList;
    public Transform meshList;
    public bool showOptimal;

    private List<GameObject> optimizedMeshes;
    private int currentProp = 0;
    private int propsNumber;

    // Start is called before the first frame update
    void Start()
    {
        optimizedMeshes = new List<GameObject>();

        foreach (Transform child in meshList)
        {
            optimizedMeshes.Add(child.gameObject);
        }

        if (showOptimal)
        {
            propList[0].SetActive(false);
            propsNumber = optimizedMeshes.Count;
        }
        else
        {
            optimizedMeshes[0].SetActive(false);
            propsNumber = propList.Count;
        }
    }

    void Update()
    {

        if (Input.GetKeyDown("x"))
        {
            if (showOptimal)
            {
                optimizedMeshes[currentProp].SetActive(false);
            }
            else
            {
                propList[currentProp].SetActive(false);
            }

            currentProp += 1;
            currentProp = currentProp % propsNumber;
            changeCurrentProp();
        }

        if (Input.GetMouseButtonDown(0))
        { // capture screen shot on left mouse button down
          // ​
            string folderPath = "Assets/Screenshot"; // the path of your project folder

            if (!System.IO.Directory.Exists(folderPath)) // if this path does not exist yet
                System.IO.Directory.CreateDirectory(folderPath);  // it will get created

            var screenshotName =
                                    "Motion_" +
                                    System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + // puts the current time right into the screenshot name
                                    ".png"; // put youre favorite data format here
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName), 2); // takes the sceenshot, the "2" is for the scaled resolution, you can put this to 600 but it will take really long to scale the image up
            Debug.Log(folderPath + screenshotName); // You get instant feedback in the console
        }
    }

    void changeCurrentProp()
    {
        if (showOptimal)
        {
            optimizedMeshes[currentProp].SetActive(true);
        }
        else
        {
            propList[currentProp].SetActive(true);
        }
    }
}
