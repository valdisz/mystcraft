namespace advisor.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

public static class JsonValueConversion {
    public static PropertyBuilder<T> HasConversionJson<T>(this PropertyBuilder<T> propertyBuilder, DatabaseProvider databaseType) where T : class, new() {
        var converter = new ValueConverter<T, string>
        (
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<T>(v) ?? new T()
        );

        var comparer = new ValueComparer<T>
        (
            (l, r) => JsonConvert.SerializeObject(l) == JsonConvert.SerializeObject(r),
            v => v == null ? 0 : JsonConvert.SerializeObject(v).GetHashCode(),
            v => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(v))
        );

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueConverter(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);

        switch (databaseType) {
            case DatabaseProvider.PgSQL:
                propertyBuilder.HasColumnType("jsonb");
                break;

            case DatabaseProvider.MsSQL:
                propertyBuilder.HasColumnType("nvarchar(max)");
                break;

            case DatabaseProvider.SQLite:
                propertyBuilder.HasColumnType("TEXT");
                break;
        }

        return propertyBuilder;
    }
}
