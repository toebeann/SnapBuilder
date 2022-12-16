using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tobey.SnapBuilder;
public class ColliderCache : MonoBehaviour
{
    private const int CleanUpSeconds = 5;
    private const int MaxRecords = 50;

    private Dictionary<Collider, ColliderRecord> records = new Dictionary<Collider, ColliderRecord>();

    /// <summary>
    /// The active <see cref="ColliderRecord"/>
    /// </summary>
    public ColliderRecord Record { get; private set; }

    /// <summary>
    /// Returns or initialises the <see cref="Collider"/> for a given <see cref="Collider"/>
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    public ColliderRecord GetRecord(Collider collider) => records.TryGetValue(collider, out ColliderRecord record) switch
    {
        true => record,
        false => AddRecord(collider)
    };

    private ColliderRecord AddRecord(Collider collider)
    {
        while (records.Count >= MaxRecords)
        {
            var pair = records.OrderBy(pair => pair.Value.Timestamp).First();
            pair.Value.Revert();
            records.Remove(pair.Key);
        }

        return Record = records[collider] = new ColliderRecord(collider);
    }

    public void RevertAll()
    {
        Record = null;
        foreach (var record in records.Values)
        {
            record.Revert();
        }
    }

    private void Update()
    {
        foreach (var collider in records
            .Where(pair => !pair.Value.IsImproved
                           && DateTime.UtcNow > pair.Value.Timestamp + TimeSpan.FromSeconds(CleanUpSeconds))
            .Select(pair => pair.Key).ToHashSet())
        {
            records.Remove(collider);
        }
    }
}
