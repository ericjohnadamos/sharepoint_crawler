namespace SureStone.Infrastructure.Extensions;

public static class CustomLinqExtensions
{
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        using var enumerator = source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return GetBatch(enumerator, batchSize).ToList();
        }
    }

    private static IEnumerable<T> GetBatch<T>(IEnumerator<T> source, int batchSize)
    {
        do 
        {
            yield return source.Current;
        } while (--batchSize > 0 && source.MoveNext());
    }
}
