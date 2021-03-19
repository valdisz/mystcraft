namespace advisor.Persistence {
    using System.Collections.Generic;
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
}
