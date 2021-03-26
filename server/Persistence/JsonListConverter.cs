namespace advisor.Persistence {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Newtonsoft.Json;

    public class JsonListConverter<T> : ValueConverter<List<T>, string> {
        public JsonListConverter()
            : base(
                v => (v == null || v.Count == 0) ? null : JsonConvert.SerializeObject(v),
                v => v == null || v == "" ? new () : JsonConvert.DeserializeObject<List<T>>(v)
            )
        {
        }
    }

    public class JsonListValueComparer<T> : ValueComparer<List<T>> {
        public JsonListValueComparer()
            : base(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            )
        {
        }
    }
}
