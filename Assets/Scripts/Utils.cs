using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static bool checkCollision(Vector3 a, Vector3 b, float radiusA, float radiusB)
    {
        return Mathf.Pow(Vector3.Distance(a, b), 2) < (radiusA + radiusB) * (radiusA + radiusB);
    }

    public static float permutations(int n)
    {
        float permutate = 1;

        for (int i = 1; i <= n; ++i)
            permutate *= i;

        return permutate;
    }

    public static float permutationsWithoutRepetitions(int n, int r)
    {
        return permutations(n) / permutations(n - r);
    }
}
