using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentalConditions
{
    private static readonly Dictionary<string, Func<EnvironmentalConditions, float>> getterMapper = new()
    {
        ["humidity"] = (EnvironmentalConditions env) => env.Humidity,
        ["temperature"] = (EnvironmentalConditions env) => env.Temperature
    };

    public static bool getGetter(string textField, out Func<EnvironmentalConditions, float> getter)
    {
        return getterMapper.TryGetValue(textField, out getter);
    }

    public float Humidity {get; set;}
    public float Temperature {get; set;}
}
