namespace GitWorktreeManager.Behaviors;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

internal static class ObservableCollectionExtension
{
    public static void Update<T>(this ObservableCollection<T> source, IEnumerable<T> newCollection)
    {
        var newItems = newCollection.ToList();

        if (source.Count > 0)
        {
            // Remove items that are not in the new list
            var toRemove = source.Except(newItems).ToList();
            foreach (var item in toRemove)
            {
                source.Remove(item);
            }

            // Reorder the common items so the source matches the order of the new collection
            // This will be a no-op if both collections are ordered
            var commonItemsInNewOrder = source.Intersect(newItems).ToList();
            for (int i = 0; i < commonItemsInNewOrder.Count; i++)
            {
                var newItem = commonItemsInNewOrder[i];
                var oldIndex = source.IndexOf(newItem);
                if (oldIndex != i && oldIndex >= 0)
                {
                    source.Move(oldIndex, i);
                }
            }
        }

        // Insert new items in order
        var toAdd = newItems.Except(source).ToList();
        foreach (var item in toAdd)
        {
            var index = newItems.IndexOf(item);
            source.Insert(index, item);
        }
    }
}
