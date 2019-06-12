using System.Collections.Generic;

namespace L10NSharp
{
	public static class L10NSharpListExtensions
	{
		public static void AddIfUniqueAndNotNull<T>(this List<T> list, T item)
		{
			if (item == null) return;

			if (!list.Contains(item))
				list.Add(item);
		}
	}
}
