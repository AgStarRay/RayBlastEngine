using RayBlast;

public static partial class ArrayExtensions {
	//TODO: Lower the allocations in this class
	public static bool IsEquivalentTo<T>(this T[]? thisArray, T[]? array) {
		if(thisArray == null)
			return array == null;
		if(array == null)
			return false;
		int arrayLength = thisArray.Length;
		if(arrayLength != array.Length)
			return false;
		for(int i = 0; i < arrayLength; i++) {
			if(thisArray[i] == null) {
				if(array[i] != null)
					return false;
			}
			else if(!(thisArray[i]?.Equals(array[i]) ?? false)) {
				return false;
			}
		}
		return true;
	}

	public static bool IsEquivalentTo<T>(this List<T>? thisList, List<T>? list) {
		if(thisList == null)
			return list == null;
		if(list == null)
			return false;
		int listCount = thisList.Count;
		if(listCount != list.Count)
			return false;
		for(int i = 0; i < listCount; i++) {
			if(thisList[i] == null) {
				if(list[i] != null)
					return false;
			}
			else if(!(thisList[i]?.Equals(list[i]) ?? false)) {
				return false;
			}
		}
		return true;
	}

	public static bool IsEquivalentTo<T>(this T[]? thisArray, List<T>? list) {
		if(thisArray == null)
			return list == null;
		if(list == null)
			return false;
		int arrayLength = thisArray.Length;
		if(arrayLength != list.Count)
			return false;
		for(int i = 0; i < arrayLength; i++) {
			if(thisArray[i] == null) {
				if(list[i] != null)
					return false;
			}
			else if(!(thisArray[i]?.Equals(list[i]) ?? false)) {
				return false;
			}
		}
		return true;
	}

	public static bool IsEquivalentTo<T>(this List<T>? thisList, T[]? array) {
		if(thisList == null)
			return array == null;
		if(array == null)
			return false;
		int listCount = thisList.Count;
		if(listCount != array.Length)
			return false;
		for(int i = 0; i < listCount; i++) {
			if(thisList[i] == null) {
				if(array[i] != null)
					return false;
			}
			else if(!(thisList[i]?.Equals(array[i]) ?? false)) {
				return false;
			}
		}
		return true;
	}

	public static bool IsEquivalentTo<T>(this ReadOnlySpan<T> thisSpan, ReadOnlySpan<T> span) {
		if(thisSpan == null)
			return span == null;
		if(span == null)
			return false;
		int spanLength = thisSpan.Length;
		if(spanLength != span.Length)
			return false;
		for(int i = 0; i < spanLength; i++) {
			if(thisSpan[i] == null) {
				if(span[i] != null)
					return false;
			}
			else if(!(thisSpan[i]?.Equals(span[i]) ?? false)) {
				return false;
			}
		}
		return true;
	}

	public static bool Contains<T>(this T[] thisArray, T instance) where T : class {
		foreach(T t in thisArray) {
			if(t.Equals(instance))
				return true;
		}
		return false;
	}

	public static bool Contains(this char[] thisArray, char instance) {
		foreach(char t in thisArray) {
			if(t.Equals(instance))
				return true;
		}
		return false;
	}

	public static bool Contains(this S32X2[] thisArray, S32X2 instance) {
		foreach(S32X2 t in thisArray) {
			if(t.Equals(instance))
				return true;
		}
		return false;
	}

	public static bool ContainsAnyOf<T>(this IEnumerable<T> thisList, ICollection<T> list) {
		foreach(T t in thisList) {
			foreach(T t2 in list) {
				if(t == null) {
					if(t2 == null)
						return true;
				}
				else if(t.Equals(t2)) {
					return true;
				}
			}
		}
		return false;
	}

	public static int IndexOf<T>(this IList<T> thisArray, T instance) {
		int arrayLength = thisArray.Count;
		if(instance == null) {
			for(int i = 0; i < arrayLength; i++) {
				if(thisArray[i] == null)
					return i;
			}
		}
		else {
			for(int i = 0; i < arrayLength; i++) {
				if(instance.Equals(thisArray[i]))
					return i;
			}
		}
		return -1;
	}

	public static List<int> IndexesOf<T>(this IList<T> thisList, T instance) {
		var list = new List<int>();
		int listCount = thisList.Count;
		if(instance == null) {
			for(int i = 0; i < listCount; i++) {
				if(thisList[i] == null)
					list.Add(i);
			}
			return list;
		}
		for(int i = 0; i < listCount; i++) {
			if(instance.Equals(thisList[i]))
				list.Add(i);
		}
		return list;
	}

	public static bool IsSubsetOf<T>(this IList<T> thisList, List<T> encapsulatingList) {
		return thisList.All(encapsulatingList.Contains);
	}

	public static bool IsEqualSetOf<T>(this IList<T> thisList, IList<T> encapsulatingList) {
		return thisList.All(encapsulatingList.Contains) && encapsulatingList.All(thisList.Contains);
	}

	public static T[] SubArray<T>(this IList<T> list, int startIndex,
								  int length) {
		int arrayLength = list.Count;
		if(startIndex < 0)
			throw new ArgumentOutOfRangeException(nameof(startIndex), "cannot be negative");
		if(length < 0)
			throw new ArgumentOutOfRangeException(nameof(length), "cannot be negative");
		if(startIndex > arrayLength)
			throw new ArgumentOutOfRangeException(nameof(startIndex), "cannot exceed the length of the array");
		if(length == 0)
			return Array.Empty<T>();
		if(startIndex + length > arrayLength)
			throw new ArgumentOutOfRangeException(nameof(length), "goes past the last index");
		if(startIndex == 0 && length == arrayLength)
			return list.ToArray();
		var obJsonArray = new T[length];
		for(int i = startIndex, j = 0; j < length; i++, j++) {
			obJsonArray[j] = list[i];
		}
		return obJsonArray;
	}

	public static T Choose<T>(this T[] thisArray, RNG rng) {
		if(thisArray == null)
			throw new NullReferenceException("Array cannot be null");
		int arrayLength = thisArray.Length;
		if(arrayLength == 0)
			throw new ArgumentException("Array is empty");
		return thisArray[rng.Exclusive(arrayLength)];
	}

	public static T Choose<T>(this IList<T> thisList, RNG rng) {
		if(thisList == null)
			throw new NullReferenceException("List cannot be null");
		int listCount = thisList.Count;
		if(listCount == 0)
			throw new ArgumentException("List is empty");
		return thisList[rng.Exclusive(listCount)];
	}

	public static void Shuffle<T>(this List<T> thisList, RNG rng) {
		int n = thisList.Count;
		while(n-- > 1) {
			int k = rng.Inclusive(n);
			(thisList[k], thisList[n]) = (thisList[n], thisList[k]);
		}
	}

	public static T Last<T>(this IList<T> thisList) {
		if(thisList == null)
			throw new NullReferenceException("List cannot be null");
		int listCount = thisList.Count;
		if(listCount == 0)
			throw new ArgumentException("List is empty");
		return thisList[listCount - 1];
	}

	public static void RemoveLast<T>(this IList<T> thisList) {
		if(thisList == null)
			throw new NullReferenceException("List cannot be null");
		int listCount = thisList.Count;
		if(listCount == 0)
			throw new ArgumentException("List is empty");
		thisList.RemoveAt(listCount - 1);
	}

	public static int TrueSum(this IEnumerable<bool> thisArray) {
		return thisArray.Sum(b => b ? 1 : 0);
	}

	public static int TrueSum(this bool[,] thisArray) {
		int sum = 0;
		foreach(bool b in thisArray) {
			if(b)
				sum++;
		}
		return sum;
	}

	public static int FalseSum(this IEnumerable<bool> thisArray) {
		return thisArray.Sum(b => b ? 0 : 1);
	}

	public static int FalseSum(this bool[,] thisArray) {
		int sum = 0;
		foreach(bool b in thisArray) {
			if(!b)
				sum++;
		}
		return sum;
	}

	public static BigNumber Sum(this IEnumerable<BigNumber> thisArray) {
		return thisArray.Aggregate<BigNumber, BigNumber>(0, (current, d) => current + d);
	}

	public static int Sum(this IEnumerable<int> thisList) {
		return Enumerable.Sum(thisList);
	}

	public static float Sum(this IEnumerable<float> thisList) {
		return Enumerable.Sum(thisList);
	}

	public static double Sum(this IEnumerable<double> thisList) {
		return Enumerable.Sum(thisList);
	}

	public static string ContentsAsString<T>(this IEnumerable<T> thisList) {
		return $"{{ {string.Join(", ", thisList)} }}";
	}

	public static bool And(this IEnumerable<bool> thisList) {
		return thisList.All(b => b);
	}

	public static bool Or(this IEnumerable<bool> thisList) {
		return thisList.Any(b => b);
	}
}
