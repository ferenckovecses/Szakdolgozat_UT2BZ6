using UnityEngine;

public static class Extensions
{
    public static float RandomLevelDifference(this float number)
    {
    	return number * Random.Range(0.9f, 1.1f);
    }
}