using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class OptimizerReportController
{
    public static void reportCostLog(List<float> costData, string propName, string folder, int iteration)
    {
        string destination = Application.dataPath + "/Resources/Data/" + folder + "/" + iteration + "/costlog_" + propName + ".csv";

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

    public static void reportPropsData(List<float> bestValues, GameObject[] props, string evaluation, string folder, int testNumber)
    {
        string destination = Application.dataPath + "/Resources/Data/" + folder + "/final_" + evaluation + "_" + testNumber + "_results.csv";

        List<string> costInfo = new List<string>();
        costInfo.Add("Prop,Optimal");

        for (int i = 0; i < bestValues.Count; i++)
        {
            costInfo.Add(props[i].name + "," + bestValues[i]);
        }

        StreamWriter writer = new StreamWriter(destination, false);

        foreach (string info in costInfo)
            writer.WriteLine(info);

        writer.Close();
    }

    public static void reportExpertProp(List<float> bestValues, GameObject[] props)
    {
        string destination = Application.dataPath + "/Resources/Data/expert_results.csv";

        List<string> costInfo = new List<string>();
        costInfo.Add("Prop,Optimal");

        for (int i = 0; i < 10; i++)
        {
            costInfo.Add(props[i].name + "," + bestValues[i]);
        }

        StreamWriter writer = new StreamWriter(destination, false);

        foreach (string info in costInfo)
            writer.WriteLine(info);

        writer.Close();
    }
}