using System;
using System.Collections.Generic;

namespace Sercher
{
    public interface IConsistentHashLoadBalance<T>
    {
        int ServerDBTotalNum { get; }

        void AddHashMap(T serverDB, Func<KeyValuePair<long, T>, long> MaxKeySelector);
        List<string> AddHashMap(T serverDB, Func<KeyValuePair<long, T>, long> MaxKeySelector, Func<T, List<string>> EmigrationSouceMap, Action<T, List<string>> ImmigrationAction);
        T FindCloseServerDBsByValue(string world);
        IEnumerable<T> GetServerNodes();
        void RemoveHashMap(T serverDB, bool ReSetServerDBCount = true);
    }
}