namespace GitWorktreeManager.Behaviors;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

internal static class ObservableCollectionExtension
{
    public static void Update<T>(this ObservableCollection<T> source, IEnumerable<T> newCollection)
    {
        var oldItems = source.ToList();
        var newItems = newCollection.ToList();

        // Items to remove
        var toRemove = oldItems.Except(newItems).ToList();
        foreach (var item in toRemove)
        {
            source.Remove(item);
        }

        // Items to add
        var toAdd = newItems.Except(oldItems).ToList();
        foreach (var item in toAdd)
        {
            source.Add(item);
        }

        // Items to update
        var toUpdate = oldItems.Union(newItems).ToList();
        // TODO: Call Update() on old items by passing in the new item

        // Items to reorder
        for (int i = 0; i < newItems.Count; i++)
        {
            var newItem = newItems[i];
            var oldIndex = source.IndexOf(newItem);
            if (oldIndex != i && oldIndex >= 0)
            {
                source.RemoveAt(oldIndex);
                source.Insert(i, newItem);
            }
        }
    }
}
