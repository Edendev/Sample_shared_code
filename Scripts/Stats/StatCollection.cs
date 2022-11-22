using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.ScriptableObjects.Stats;
using Game.Interfaces;

namespace Game.Generic.Stats
{
    [System.Serializable]
    public class StatCollection : IResetteable
    {
        private Dictionary<StatType, Stat> stats = new Dictionary<StatType, Stat>();

        [SerializeField] List<StatStruct> statsList = new List<StatStruct>();

        public StatCollection() { }
        public StatCollection(StatCollection _root)
        {
            statsList = _root.GetStatsStructList();
        }

        public void Initialize()
        {
            stats = new Dictionary<StatType, Stat>();
            foreach (StatStruct statStruct in statsList) AddStat(statStruct.StatType, statStruct.StatSO);
        }

        void AddStat(StatType statType, StatSO statSO)
        {
            Stat newStat = null;
            if (stats.ContainsKey(statType)) return;
            if (statType == StatType.Health) newStat = new Attribute(statSO);
            else newStat = new PrimaryStat(statSO);
            newStat.Reset();
            stats.Add(statType, newStat);
        }

        /// <summary>
        /// Returns the baseValue from the StatSO searching in the statsList given from the inspector (can be used before Initialize)
        /// </summary>
        public int GetBaseStatValue(StatType statType)
        {
            StatStruct statStruct = statsList.Find(x => x.StatType == statType);
            if (statStruct.StatSO == null) return -1;
            return statStruct.StatSO.BaseValue;
        }

        public Stat GetStat(StatType statType)
        {
            if (!HasStat(statType)) return null;
            return stats[statType];
        }

        public bool HasStat(StatType statType)
        {
            return stats.ContainsKey(statType);
        }

        public void SubscribeToOnModifierAdded(StatType statType, System.Action target)
        {
            Stat stat = GetStat(statType);
            if (stat == null) return;
            stat.OnModifierAdded += target;
        }
        public void UnSubscribeToOnModifierAdded(StatType statType, System.Action target)
        {
            Stat stat = GetStat(statType);
            if (stat == null) return;
            stat.OnModifierAdded -= target;
        }
        public void SubscribeToOnModifierRemoved(StatType statType, System.Action target)
        {
            Stat stat = GetStat(statType);
            if (stat == null) return;
            stat.OnModifierRemoved += target;
        }
        public void UnSubscribeToOnModifierRemoved(StatType statType, System.Action target)
        {
            Stat stat = GetStat(statType);
            if (stat == null) return;
            stat.OnModifierRemoved -= target;
        }
        public void SubscribeToOnCurrentValueIsZero(StatType statType, System.Action target)
        {
            Attribute attribute = GetStat(statType) as Attribute;
            if (attribute == null) return;
            attribute.OnCurrentValueIsZero += target;
        }
        public void UnSubscribeToOnCurrentValueIsZero(StatType statType, System.Action target)
        {
            Attribute attribute = GetStat(statType) as Attribute;
            if (attribute == null) return;
            attribute.OnCurrentValueIsZero -= target;
        }
        public void SubscribeToOnCurrentValueChanged(StatType statType, System.Action<int,int> target)
        {
            Attribute attribute = GetStat(statType) as Attribute;
            if (attribute == null) return;
            attribute.OnCurrentValueChanged += target;
        }
        public void UnSubscribeToOnCurrentValueChanged(StatType statType, System.Action<int, int> target)
        {
            Attribute attribute = GetStat(statType) as Attribute;
            if (attribute == null) return;
            attribute.OnCurrentValueChanged -= target;
        }
        public void SubscribeToOnValueChanged(StatType statType, System.Action<int> target)
        {
            Stat stat = GetStat(statType);
            if (stat == null) return;
            stat.OnValueChanged += target;
        }
        public void UnSubscribeToOnValueChanged(StatType statType, System.Action<int> target)
        {
            Stat stat = GetStat(statType);
            if (stat == null) return;
            stat.OnValueChanged -= target;
        }

        public void Reset()
        {
            foreach (Stat stat in stats.Values) stat.Reset();
        }

        /// <summary>
        /// Should only used to create a new stats collection from an existing stats collection
        /// </summary>
        /// <returns></returns>
        public List<StatStruct> GetStatsStructList() => statsList;
    }
}

