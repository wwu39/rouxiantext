using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static float Pow(float b, int p)
    {
        float result = 1;
        for (int i = 0; i < p; ++i) result *= b;
        return result;
    }
}
