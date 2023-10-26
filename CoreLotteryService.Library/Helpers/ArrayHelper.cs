namespace CoreLotteryService.Library.Helpers;

/// <summary>
/// An utility class that hold some useful methods of handling a Array.
/// </summary
public static class ArrayHelper
{
    /// <summary>
    /// Creates an array of <see cref="T"/> int with a default value.
    /// </summary>
    /// <param name="value">The default value.</param>
    /// <param name="count">The array length.</param>
    /// <returns>
    /// A array of <see cref="T"/> with the given length and default value.
    /// </returns>  
    public static T[] FillArray<T>(T value, int count) where T : IConvertible
    {
        T[] result = new T[count];
        Array.Fill(result, value);
        return result;
    }
    /// <summary>
    /// Adds all values from some list to the target list.
    /// </summary>
    /// <param name="otherList">The other list</param>
    /// <returns>
    /// A List of <see cref="T"/> with both lists concatenated.
    /// </returns>  
    public static List<T> Extends<T>(this List<T> self, List<T> otherList) where T : IConvertible
    {
        if (self.Count == 0)
        {
            self.Clear();
            self = otherList;
        }
        else
        {
            self = self.Concat(otherList).ToList();
        }
        return self;
    }
}


