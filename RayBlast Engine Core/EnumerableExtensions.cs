namespace RayBlast; 

public static class EnumerableExtensions {
	public static T RemoveAtAndReturn<T>(this IList<T> list, int index) {
		T item = list[index];
		list.RemoveAt(index);
		return item;
	}
}
