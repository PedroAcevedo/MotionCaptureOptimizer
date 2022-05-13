using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class OptimizerReportController
{

    public static void reportCostLog(List<float> costData)
    {
        string destination = Application.dataPath + "/Resources/CostLog.csv";

        List<string> costInfo = new List<string>();
        costInfo.Add("Iteration,Cost");

        for (int i=0; i < costData.Count; i++)
        {
            costInfo.Add(i + "," + costData[i]);
        }

        StreamWriter writer = new StreamWriter(destination, false);

        foreach (string info in costInfo)
            writer.WriteLine(info);

        writer.Close();
    }

 
}