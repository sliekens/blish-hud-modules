namespace SL.ChatLinks;

internal static class EnumerableExtensions
{
    public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
    {
        if (size < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be greater than 0.");
        }
        using IEnumerator<TSource> enumerator = source.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            yield break;
        }
        int arraySize = Math.Min(size, 4);
        int i;
        do
        {
            TSource[] array = new TSource[arraySize];
            array[0] = enumerator.Current;
            i = 1;
            if (size != array.Length)
            {
                for (; i < size && enumerator.MoveNext(); i++)
                {
                    if (i >= array.Length)
                    {
                        arraySize = (int)Math.Min((uint)size, 2 * (uint)array.Length);
                        Array.Resize(ref array, arraySize);
                    }
                    array[i] = enumerator.Current;
                }
            }
            else
            {
                TSource[] local = array;
                for (; (uint)i < (uint)local.Length && enumerator.MoveNext(); i++)
                {
                    local[i] = enumerator.Current;
                }
            }
            if (i != array.Length)
            {
                Array.Resize(ref array, i);
            }
            yield return array;
        } while (i >= size && enumerator.MoveNext());
    }
}
