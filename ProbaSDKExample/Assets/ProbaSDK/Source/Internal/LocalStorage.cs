using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProbaSDK.Internal
{
	internal static class LocalStorage
	{
		[Serializable]
		private class Storage<T>
		{
			public T[] items;
		}
		
		private static string Key(string key) => $"Proba.{key}";
		
		public static string GetString(string key, string def = "")
		{
			return PlayerPrefs.GetString(Key(key), def);
		}
		
		public static void SetString(string key, string value)
		{
			PlayerPrefs.SetString(Key(key), value);
		}
		
		public static bool GetBool(string key, bool def = false)
		{
			return PlayerPrefs.GetInt(Key(key), def ? 1 : 0) == 1;
		}
		
		public static void SetBool(string key, bool value)
		{
			PlayerPrefs.SetInt(Key(key), value ? 1 : 0);
		}
		
		public static T[] GetObjects<T>(string key) where T: class
		{
			var val = PlayerPrefs.GetString(Key(key), null);
			
			if (string.IsNullOrEmpty(val))
			{
				return new T[0];
			}

			return JsonUtility.FromJson<Storage<T>>(val)?.items ?? new T[0];
		}
		
		public static void SetObjects<T>(string key, IEnumerable<T> objects) where T: class
		{
			var value = JsonUtility.ToJson(new Storage<T> {items = objects.ToArray()});
			PlayerPrefs.SetString(Key(key), value);
		}
	}
}