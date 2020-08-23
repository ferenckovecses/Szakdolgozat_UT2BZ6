using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class Extensions
{
    public static float RandomLevelDifference(this float number)
    {
    	return number * Random.Range(0.9f, 1.1f);
    }

    public static T DeepClone<T>(this T a)
    {
    	using (MemoryStream stream = new MemoryStream())
    	{
    		BinaryFormatter formatter = new BinaryFormatter();
    		formatter.Serialize(stream, a);
    		stream.Position = 0;
    		return (T) formatter.Deserialize(stream);
    	}
    }
}