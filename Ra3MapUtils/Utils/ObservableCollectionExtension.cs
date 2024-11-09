using System.Collections.ObjectModel;

namespace Ra3MapUtils.Utils;

public static class ObservableCollectionExtension
{
    public static int IndexOf<T>(this ObservableCollection<T> collection, T item)
    {
        return collection.ToList().IndexOf(item);
    }
    
    public static void Exchange<T>(this ObservableCollection<T> collection, int index1, int index2)
    {
        if (index1 < 0 || index1 >= collection.Count || index2 < 0 || index2 >= collection.Count)
        {
            return;
        }
        var tmp = collection[index1];
        collection[index1] = collection[index2];
        collection[index2] = tmp;
    }
    
    public static void SortBy<T>(this ObservableCollection<T> collection, Func<T, object> keySelector)
    {
        var sorted = collection.OrderBy(keySelector).ToList();
        for (var i = 0; i < collection.Count; i++)
        {
            collection[i] = sorted[i];
        }
    }
    
}