using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using HutongGames.PlayMaker;
using Random = UnityEngine.Random;

public static class Extensions
{
    // (?:\b|>|\W|^) = find start of word (bounds, tag, non word)
    // (?<!<link="") = negative look behind... DO NOT match inside a link tag
    // (?<!<\/) = negative look behind... DO NOT match inside an ending tag
    // (?<!-) = negative look behind. DO NOT match on hyphen words
    // ({0}...) = capture group the thing we want
    // (?!['’][a-zA-Z]) = stop word bound at either apostrophe. ex: looking for whale, won't find whale's
    // (?!>) = not and ending tag
    // (?:\b|>|\W|$) = end on a word bound
    // TL:DR - finds words on boundaries without a leading hyphen
    private const string OccurrencePattern = @"(?:\b|>|\W|^)(?<!<link="")(?<!<\/)(?<!-)({0}(?!['’][a-zA-Z])(?!>))(?:\b|>|\W|$)";
    private const string CommaDigits = @"(\d+,)+\d+";

	/// <summary>
	/// Replaces ‘ ’ and ' with substitute text
	/// </summary>
	/// <param name="source"></param>
	/// <param name="to"></param>
	/// <returns></returns>
	public static string ReplaceApostropheTypes(this string source, string to)
	{
		return source.Replace("‘", to).Replace("’", to).Replace("'", to);
	}

	/// <summary>
	/// Expand given Rect by X.
	/// </summary>
	/// <param name="rect"></param>
	/// <param name="by"></param>
	/// <returns></returns>
	public static Rect Expand(this Rect rect, float by)
	{
		return by == 0 ? rect : rect.Expand(by, by);
	}

	/// <summary>
	/// Expand given Rect by x width and y height.
	/// </summary>
	/// <param name="rect"></param>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns></returns>
	public static Rect Expand(this Rect rect, float x, float y)
	{
		rect.xMax += x;
		rect.yMax += y;
		rect.xMin -= x;
		rect.yMin -= y;
		return rect;
	}

	/// <summary>
	/// Merge rect A with B.
	/// </summary>
	/// <param name="a"></param>
	/// <param name="b"></param>
	/// <returns></returns>
	public static Rect Merge(this Rect a, Rect b)
	{
		if (b != default(Rect))
		{
			a.xMin = Mathf.Min(a.xMin, b.xMin);
			a.xMax = Mathf.Max(a.xMax, b.xMax);
			a.yMin = Mathf.Min(a.yMin, b.yMin);
			a.yMax = Mathf.Max(a.yMax, b.yMax);
		}
		return a;
	}

	/// <summary>
	/// Checks to see if one collider is completely contained within another.
	/// </summary>
	/// <param name="self">the containing collider</param>
	/// <param name="other">the collider that should be contained in self</param>
	/// <returns>true or false depending on if the collider is completely within the other collider</returns>
	public static bool ContainsAll(this Collider2D self, Collider2D other)
	{
		Bounds dropzoneBounds = self.bounds;
		Bounds dropObjBounds = other.bounds;

		//Get the corner positions of the dropped object.
		Vector2 topLeft = new Vector2(other.gameObject.transform.position.x - dropObjBounds.extents.x, other.gameObject.transform.position.y + dropObjBounds.extents.y);
		Vector2 topRight = new Vector2(other.gameObject.transform.position.x + dropObjBounds.extents.x, other.gameObject.transform.position.y + dropObjBounds.extents.y);
		Vector2 btmLeft = new Vector2(other.gameObject.transform.position.x - dropObjBounds.extents.x, other.gameObject.transform.position.y - dropObjBounds.extents.y);
		Vector2 btmRight = new Vector2(other.gameObject.transform.position.x + dropObjBounds.extents.x, other.gameObject.transform.position.y - dropObjBounds.extents.y);

		//Check to see if all 4 corners are in the drop zone.
		if (dropzoneBounds.Contains(topLeft) && dropzoneBounds.Contains(topRight) && dropzoneBounds.Contains(btmLeft) && dropzoneBounds.Contains(btmRight))
			return true;

		return false;
	}

	/// <summary>
	/// Finds all occurrences of the given whole word (does not find occurrences that are a part of another word)
	/// </summary>
	/// <param name="source">the string to be searched</param>
	/// <param name="target">the whole word to find</param>
	/// <param name="startIndex">the zero-based index of the first character to begin search</param>
	/// <returns>a collection of word occurrences found</returns>
	public static MatchCollection FindWordOccurrences(string source, string target, int startIndex = 0)
	{
		var sourceCopy = source;

		if (startIndex > 0 && startIndex < source.Length)
			sourceCopy = source.Substring(startIndex);

		return Regex.Matches(sourceCopy, string.Format(OccurrencePattern, Regex.Escape(target)), RegexOptions.IgnoreCase);
	}

	/// <summary>
	/// Finds all occurrences of the given substring word (includes occurrences that are a part of another word)
	/// </summary>
	/// <param name="source">the string to be searched</param>
	/// <param name="target">the substring to find</param>
	/// <returns>a collection of substring occurrences found</returns>
	private static MatchCollection FindSubstringOccurrences(string source, string target)
	{
		return Regex.Matches(source, string.Format("({0})", Regex.Escape(target)), RegexOptions.IgnoreCase);
	}

	/// <summary>
	/// Returns index of specific occurrence of a whole word within string (not part of another word).
	/// </summary>
	/// <param name="source">the string to be searched</param>
	/// <param name="value">the whole word to find</param>
	/// <param name="occurance">instance of the word to find</param>
	/// <param name="startIndex">the zero-based index of the first character to begin search</param>
	/// <returns>index of the first letter of the given instance of the given word</returns>
	public static int IndexOfWordOccurrence(this string source, string value, int occurance = 0, int startIndex = 0)
	{
		if (startIndex < 0 || startIndex >= source.Length)
			return -1;
		var matches = FindWordOccurrences(source, value, startIndex);
		if (matches.Count == 0 || occurance >= matches.Count)
			return -1;
		return matches[occurance].Groups[1].Index + startIndex;
	}

	/// <summary>
	/// Returns index of specific occurrence of a substring (may be part of another word).
	/// </summary>
	/// <param name="source">the string to be searched</param>
	/// <param name="value">the substring to find</param>
	/// <param name="occurance">instance of the word to find</param>
	/// <returns>index of the first letter of the given instance of the given word</returns>
	public static int IndexOfSubstringOccurrence(this string source, string value, int occurance = 0)
	{
		var matches = FindSubstringOccurrences(source, value);
		if (matches.Count == 0 || occurance >= matches.Count)
			return -1;
		return matches[occurance].Groups[1].Index;
	}

	/// <summary>
	/// Returns the number of occurrences of a whole word in a string (not part of another word)
	/// </summary>
	/// <param name="source">the string to be searched</param>
	/// <param name="target">the substring to find</param>
	/// <returns></returns>
	public static int WordOccurrenceCount(this string source, string target)
	{
		return FindWordOccurrences(source, target).Count;
	}

	/// <summary>
	/// Returns the number of occurrences of a whole word in a string (may be part of another word)
	/// </summary>
	/// <param name="source">the string to be searched</param>
	/// <param name="target">the substring to find</param>
	/// <returns></returns>
	public static int SubstringOccurrenceCount(this string source, string target)
	{
		return FindSubstringOccurrences(source, target).Count;
	}

	/// <summary>
	/// Will retrieve the asked for component if it already exists or add it to the gameobject
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="gameObject"></param>
	/// <returns>added component</returns>
	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		var component = gameObject.GetComponent<T>();
		//you may be thinking why not just use the null coalescing operator but it may not do what you expect
		//https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (component == null)
		{
			return gameObject.AddComponent<T>();
		}

		return component;
	}

	/// <summary>
	/// Will retrieve the asked for component if it already exists or add it to the gameobject.
	/// This variant uses a Type variable for use with Playmaker Actions.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="gameObject"></param>
	/// <returns>added component</returns>
	public static Component GetOrAddComponent(this GameObject gameObject, Type typeToCreate)
	{
		var component = gameObject.GetComponent(typeToCreate);
		//you may be thinking why not just use the null coalescing operator but it may not do what you expect
		//https://blogs.unity3d.com/2014/05/16/custom-operator-should-we-keep-it/
		// ReSharper disable once ConvertIfStatementToReturnStatement
		if (component == null)
		{
			return gameObject.AddComponent(typeToCreate);
		}

		return component;
	}
	
	/// <summary>
	/// Convert string '#XXYYZZ' to color
	/// </summary>
	/// <param name="c"></param>
	/// <returns></returns>
	public static string ToHEX(this Color color)
	{
		return "#" + ColorUtility.ToHtmlStringRGBA(color);
	}

	private static byte ToByte(float f)
	{
		f = Mathf.Clamp01(f);
		return (byte)(f * 255);
	}

	public static TValue GetValueOrSetDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
	{
		TValue value;
		if (dictionary.TryGetValue(key, out value))
			return value;
		dictionary[key] = defaultValue;
		return defaultValue;
	}

	public static void AddOrSetKeyValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
	{
		if (dictionary.ContainsKey(key))
		{
			dictionary[key] = value;
		}
		else
		{
			dictionary.Add(key, value);
		}
	}

	public static void AddIfNotExists<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
	{
		if (!dictionary.ContainsKey(key))
		{
			dictionary.Add(key, value);
		}
	}

	public static T GetRandomObject<T>(this IEnumerable<T> objects)
	{
		var index = Random.Range(0, objects.Count());
		return objects.ElementAt(index);
	}

    public static T ToEnum<T>(this string enumString)
    {
        if (string.IsNullOrEmpty(enumString))
            return default (T);
		try 
		{
        	return (T) Enum.Parse(typeof(T), enumString, true);
		} 
		catch (ArgumentException) 
		{
			return default (T);
		}
    }

	public static void Shuffle<T>(this List<T> list)
	{
		int n = list.Count();
		while (n > 1)
		{
			n--;
			int k = Random.Range(0, n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	public static FsmArray ShufflePositions(this FsmArray list)
	{
		if (list == null)
		{
			Debug.LogErrorFormat("Can't shuffle positions of array since the array is empty.");
			return null;
		}

		if (list.Values.Any(item => item == null))
		{
			Debug.LogErrorFormat("Can't shuffle when element is null.");
			return list;
		}

		if (list.ElementType != VariableType.GameObject)
		{
			Debug.LogErrorFormat("Can't shuffle positions of array type {0}. Must be of type GameObject.", list.ElementType);
			return list;
		}

		if (list.Length == 0)
		{
			Debug.LogWarningFormat("Can't shuffle positions of array since the array is empty.");
			return list;
		}

		var originalPostionsList = new List<Vector3>();
		foreach (GameObject item in list.Values)
		{
			var vector3 = item.transform.position;
			originalPostionsList.Add(vector3);
		}

		// Shuffle the array
		FsmArray[] arrays = { list };
		SyncArraysShuffle(arrays);
		var shuffledArray = new List<object>(arrays[0].Values);

		for (int i = 0; i < list.Length; i++)
		{
			var originalPosition = originalPostionsList[i];
			var sortedListGameObjectObject = shuffledArray[i] as GameObject;
			sortedListGameObjectObject.transform.position = originalPosition;
		}
		list.Values = shuffledArray.ToArray();
		return list;
	}


	public static FsmArray[] SyncArraysShuffle(this FsmArray[] arrays)
    {
        List<List<object>> fullList = arrays.Select(array => new List<object>(array.Values)).ToList();

        List<object> _list = fullList[0];

        int start = 0;
        int end = _list.Count - 1;

        // Knuth-Fisher-Yates algo

        //	for (int i = proxy.arrayList.Count - 1; i > 0; i--)
        for (int i = end; i > start; i--)
        {
            // Set swapWithPos a random position such that 0 <= swapWithPos <= i
            int swapWithPos = Random.Range(start, i + 1);

            // Swap the value at the "current" position (i) with value at swapWithPos
            foreach (var list in fullList)
            {
                object temp = list[i];
                list[i] = list[swapWithPos];
                list[swapWithPos] = temp;
            }
        }

        var count = 0;
        foreach (var l in fullList)
        {
			SetArrayValues (arrays [count], l);
            count++;
        }
        return arrays;
    }

	private static void SetArrayValues( FsmArray arrayToSetValuesFor, List<object> listOfValues )
	{
		// Set the values array. 
		arrayToSetValuesFor.Values = listOfValues.ToArray();

		// If this is a primitive type, rearrage their arrays as well. 
		switch (arrayToSetValuesFor.TypeConstraint) 
		{
			case VariableType.Bool:
			{
				if (arrayToSetValuesFor.boolValues.Length == 0)
					break;
				
				for(int currentValue = 0; currentValue < listOfValues.Count; ++currentValue)
				{
					bool value = (bool)listOfValues[currentValue];
					arrayToSetValuesFor.boolValues [currentValue] = value;
				}
				break;
			}

			case VariableType.Int:
			{
				if (arrayToSetValuesFor.intValues.Length == 0)
					break;
				
				for(int currentValue = 0; currentValue < listOfValues.Count; ++currentValue)
				{
					int value = (int)listOfValues[currentValue];
					arrayToSetValuesFor.intValues [currentValue] = value;
				}
				break;
			}

			case VariableType.Float:
			{
				if (arrayToSetValuesFor.floatValues.Length == 0)
					break;
				
				for(int currentValue = 0; currentValue < listOfValues.Count; ++currentValue)
				{
					float value = (float)listOfValues [currentValue];
					arrayToSetValuesFor.floatValues [currentValue] = value;
				}
				break;
			}

			case VariableType.String:
			{
				if (arrayToSetValuesFor.stringValues.Length == 0)
					break;
				
				for(int currentValue = 0; currentValue < listOfValues.Count; ++currentValue)
				{
					string value = (string)listOfValues[currentValue];
					arrayToSetValuesFor.stringValues [currentValue] = value;
				}
				break;
			}

			default:
				// Not sorting values because the object references and vector 4 fields seem a bit inconsistent.
				// Add that logic if necessary.
				break;
		}
	}
		
	public static List<object> Shuffle(this object[] objects)
	{
		List<object> list = new List<object>(objects);

		int start = 0;
		int end = list.Count - 1;

		// Knuth-Fisher-Yates algo

		//	for (int i = proxy.arrayList.Count - 1; i > 0; i--)
		for (int i = end; i > start; i--)
		{
			// Set swapWithPos a random position such that 0 <= swapWithPos <= i
			int swapWithPos = Random.Range(start, i + 1);

			// Swap the value at the "current" position (i) with value at swapWithPos
			object tmp = list[i];
			list[i] = list[swapWithPos];
			list[swapWithPos] = tmp;
		}
		return list;
	}

	public static void RaiseEvent(this EventHandler handler, object sender, EventArgs e)
	{
		//the copy here avoids threading issues
		var discreetHandler = handler;
		if (discreetHandler != null)
			discreetHandler(sender, e);
	}

	public static void RaiseEvent<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs
	{
		var handlerCopy = handler;
		if (handlerCopy != null)
		{
			handlerCopy(sender, e);
		}
	}

	/// <summary>
	/// Limit a float to a couple of digits precision
	/// </summary>
	/// <param name="value"></param>
	/// <param name="digits"></param>
	/// <returns></returns>
	public static float Truncate(this float value, int digits)
	{
		double mult = Math.Pow(10.0, digits);
		double result = Math.Truncate(mult * value) / mult;
		return (float)result;
	}


	public static bool IsNullOrEmpty<T>(this T[] array)
	{
		return array == null || array.Length == 0;
	}

	/// <summary>
	/// Playmaker's Values field on an array actually is an object array and highly susceptible to cast errors even if all the items are apparently strings!
	/// Use this method to safely get the string array from an FSMArray 
	/// </summary>
	/// <param name="array"></param>
	/// <returns></returns>
	public static string[] GetStringArray(this FsmArray array)
	{
		return array.Values.OfType<string>().ToArray();
	}

	/// <summary>
	/// Playmaker's Values field on an array actually is an object array and highly susceptible to cast errors even if all the items are apparently ints!
	/// Use this method to safely get the string array from an FSMArray 
	/// </summary>
	/// <param name="array"></param>
	/// <returns></returns>
	public static int[] GetIntArray(this FsmArray array)
	{
		return array.Values.OfType<int>().ToArray();
	}

	public static string AddForwardSlashOnEndIfNone(this string url)
	{
		if (!url.EndsWith("/"))
		{
			url += "/";
		}
		return url;
	}

	/// <summary>
	/// Returns an array of all the children of the transform.
	/// </summary>
	/// <param name="transform"></param>
	/// <returns></returns>
	public static Transform[] GetChildren(this Transform transform)
	{
		return transform.Cast<Transform>().ToArray();
	}

	/// <summary>
	/// Takes a vector3 that has been populated with old style points from the old client and converts them to Unity units
	/// </summary>
	/// <param name="vector"></param>
	/// <returns></returns>
	public static Vector3 ConvertToUnityUnits(this Vector3 vector)
	{
		return new Vector3((-400f + vector.x)/100f, (300f - vector.y)/100f);
	}

	/// <summary>
	/// Alter's a sprite's pivot point based on the regpoint coordinate system in the old client
	/// </summary>
	/// <param name="sprite"></param>
	/// <param name="regPoint"></param>
	/// <returns></returns>
	public static Sprite AlterPivotPoint(this Sprite sprite, Vector2 regPoint)
	{
		var unityx = regPoint.x / sprite.rect.width;
		var unityy = 1 - (regPoint.y / sprite.rect.height);

		return Sprite.Create(sprite.texture, sprite.rect, new Vector2(unityx, unityy));
	}

	/// <summary>
	/// Wrap comma separated numbers in a link tag (otherwise TMP will make them comma separated words)
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	public static string ClickableDigitGroups(string text)
	{
		foreach (Match match in Regex.Matches(text, CommaDigits))
		{
			text = text.Replace(match.Value, string.Format(@"<link=""{0}"">{0}</link>", match.Value));
		}
		return text;
	}

	/// <summary>
	/// Convert a UI (assuming PPU is 100) point to a world point
	/// </summary>
	/// <param name="self"></param>
	/// <param name="isUi">if false, return point as is</param>
	/// <returns></returns>
	public static Vector3 UiToWorld(this Vector3 self, bool isUi = true)
	{
		if (isUi)
		{
			self /= 100f;
		}
		return self;
	}

    /// <summary>
    /// Checks to see if a parameter exists in an animator controller by name.
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    public static bool ContainsParameter(this Animator animator, string parameterName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == parameterName) return true;
        }
        return false;
    }

    /// <summary>
    /// Remaps a float in a given range to another target range. e.g. A value of 5 in given range 0 to 10 will remap to 2.5 in target range 0 to 5, or 10 in target range 0 to 20.
    /// </summary>
    /// <param name="value">  </param>
    /// <param name="initialRangeLow"> Lower bound of given range. </param>
    /// <param name="initialRangeHigh"> Upper bound of given range. </param>
    /// <param name="targetRangeLow"> Lower bound of target range. </param>
    /// <param name="targetRangeHigh"> Upper bound of target range. </param>
    /// <returns></returns>
    public static float Remap(this float value, float initialRangeLow, float initialRangeHigh, float targetRangeLow, float targetRangeHigh)
    {
        return targetRangeLow + (value - initialRangeLow) * (targetRangeHigh - targetRangeLow) /
               (initialRangeHigh - initialRangeLow);
    }

	/// <summary>
	/// Convert a Vector2Int to a Vector2
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public static Vector2 ToVector2(this Vector2Int source)
	{
		return new Vector2(source.x, source.y);
	}

}
