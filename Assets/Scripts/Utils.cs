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
        return permutations(n) / (permutations(n - r));
    }

    public static float r(float x, float y)
    {
        return Mathf.Sqrt(Mathf.Pow(x, 2.0f) + Mathf.Pow(y, 2.0f));
    }

    public static float theta(float x, float y)
    {
        return Mathf.Atan(y / x);
    }

    public static Vector2 pointToPolarCoord(Vector2 point)
    {
        return new Vector2(r(point.x, point.y), theta(point.x, point.y));
    }

    public static Vector3 pointToCylindricalCoord(Vector3 point)
    {
        return new Vector3(r(point.x, point.y), theta(point.x, point.y), point.z);
    }

    public static Vector2 points2DCentroid(List<Vector2> points)
    {
        float x = 0.0f;
        float y = 0.0f;

        for (int i = 0; i < points.Count; i++)
        {
            x += points[i].x;
            y += points[i].y;
        }

        x = x / points.Count;
        y = y / points.Count;

        return new Vector2(x, y);
    }

    public static Vector2 points3DCentroid(List<Vector3> points)
    {
        float x = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        for (int i = 0; i < points.Count; i++)
        {
            x += points[i].x;
            y += points[i].y;
            z += points[i].z;
        }

        x = x / points.Count;
        y = y / points.Count;
        z = z / points.Count;

        return new Vector3(x, y, z);
    }

    public static int GCD(int num1, int num2)
    {
        int Remainder;

        while (num2 != 0)
        {
            Remainder = num1 % num2;
            num1 = num2;
            num2 = Remainder;
        }

        return num1;
    }

    public static bool KMP(string pattern, string text)
    {
        int[] lps = LPS(pattern);
        int j = 0;
        int i = 0;

        while (i < text.Length)
        {
            if(pattern[j] == text[i])
            {
                j++;
                i++;
            }

            if (j == pattern.Length)
            {
                j = lps[j - 1];
                return true;
            }
            else if(i < text.Length && pattern[j] != text[i])
            {
                if(j != 0)
                {
                    j = lps[j - 1];
                } else
                {
                    i++;
                }
            }
        }

        return false;
    }

    public static float angleDiff(Vector2 a, Vector2 b)
    {
        return (float)System.Math.Round(a.y - b.y, 3);
    }

    public static float angleDiff(Vector3 a, Vector3 b)
    {
        return (float)System.Math.Round(a.y - b.y, 3);
    }

    // Longest proper prefix which is also suffix
    public static int[] LPS(string text)
    {
        // length of the previous longest prefix suffix
        int len = 0;
        int i = 1;
        int[] lps = new int[text.Length];

        lps[0] = 0; // lps[0] is always 0

        // the loop calculates lps[i] for i = 1 to text.Length-1
        while (i < text.Length)
        {
            if (text[i] == text[len])
            {
                len++;
                lps[i] = len;
                i++;
            }
            else // (pat[i] != pat[len])
            {
                // This is tricky. Consider the example.
                // AAACAAAA and i = 7. The idea is similar
                // to search step.
                if (len != 0)
                {
                    len = lps[len - 1];

                    // Also, note that we do not increment
                    // i here
                }
                else // if (len == 0)
                {
                    lps[i] = len;
                    i++;
                }
            }
        }

        return lps;
    }
}
