using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.SnapBuilder
{
    internal class ColliderCache : MonoBehaviour
    {
        private const int CleanUpSeconds = 5;

        private static ColliderCache main;
        public static ColliderCache Main => main == null
            ? new GameObject("ColliderCache").AddComponent<ColliderCache>()
            : main;

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
        public ColliderRecord GetRecord(Collider collider) => Record = records.TryGetValue(collider, out ColliderRecord record)
            ? record
            : records[collider] = new ColliderRecord(collider);

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
                .Select(pair => pair.Key).ToList())
            {
                records.Remove(collider);
            }
        }

        private void Awake()
        {
            if (main != null && main != this)
            {
                Destroy(this);
            }
            else
            {
                main = this;
                transform.SetParent(AimTransform.Main.transform, false);
            }
        }
    }
}
