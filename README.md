# Optimizing retroreflective marker set for motion capturing props

#### *A method that finds an optimal marker set configuration for any given prop (3D object).*

---
### Some information

If you cite this work, remember to use the following Bibtex entry to cite our paper.

```
Coming soon!
```

Citation for non-latex users:

```
Coming soon!
```

---
### Get Started

<b>Step 1) </b> 

Download and Install Unity Hub

https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe

<b>Step 2) </b> 

Download the Unity 2020.3.18f1 version from the "Installs" tab.

<b>Step 3) </b> 

Clone the project from GitHub as follows:

```
git clone https://github.com/PedroAcevedo/MotionCaptureOptimizer
```
<b>Step 4) </b> 

Open the project with Unity Hub.

<b>Step 5) </b> 

Go to Assets/Scenes/MainScene.unity and select the scene on Unity.

<b>Step 6) </b> 

Press the play button on the top menu. And the algorithm will execute like this:

![marker-set-method-run-3](https://github.com/PedroAcevedo/pcg-side-scroller-game/assets/25890069/3e49c5d8-4fb8-47f9-8b2c-006bb718c891)

<b>Step 7) </b> 

You can change parameters to obtain more results and analyze different props from the props list in the editor config.

![Unity environment and config script](https://github.com/PedroAcevedo/pcg-side-scroller-game/assets/25890069/c3280e7f-35f4-4028-acf5-bb58aebc7ae1)

### Reproduce Figure 5

To reproduce one of the outputs from Figure 5, we can follow the next steps:

1. Select the scene Assets/Scenes/MainScene.unity.
2. Select the GameObject "Optimizer" to visualize their properties on the Inspector window.
3. On the component "Optimizer.cs" in the Inspector window, there is a list of parameters to edit the initial configuration. 
4. In this example, we will run the method for the number of markers evaluation with the five markers constraint. 
5. Change the parameters in the Inspector as follows:

![Optimizer inspector Config](https://github.com/PedroAcevedo/MotionCaptureOptimizer/assets/25890069/a085dbc8-23ee-4693-a509-9ba8854cef71)
  
7. Then run the algorithm and let it execute until it stops. This experiment will show the five markers optimization for the Umbrella prop.
8. Once the algorithm is finished, you can review the result in the folder "Resources/OptimizedMeshes/1/" and look for the Prefab "Umbrella_test_optimized.prefab" to see the optimized marker set layout on the prop.
9. To have the same view as Figure 5, we need to open the scene "Images.unity", where there is a list of different optimized markers set as a child of the GameObject "OptimizedMeshes.”
10. Select the prefab "Umbrella_test_optimized.prefab" and place it on the scene environment.
11. Select the GameObject "Umbrella_penalty-only_optimized" and copy the Transform component by right-clicking the three dots at the corner of the component. 
12. Select back the recently placed prefab and paste the values of the components by right-clicking on the three dots and selecting the option "Paste Component Values.”
13. In the same GameObject on the component "Rigidbody," uncheck the property "UseGravity.”
14. Then, look for a child GameObject on the prefab child "umbrella-resized"; this one should be called "default."
15. Go to the Materials folder at "Assets/Materials" on the Unity project window and drag and drop the material "MeshColor" on the "default" GameObject previously selected.
16. Select the second Umbrella and deactivate it by unchecking the check box next to the GameObject name on the Inspector.
17. Run and visualize the prefab on a white background. You can take a screenshot by pressing the left mouse button, which will be saved in the folder "Assets/Screenshot.” 
18. Press the 'X' button to visualize different props and marker set layouts on the “OptmizedMeshes” GameObject.
