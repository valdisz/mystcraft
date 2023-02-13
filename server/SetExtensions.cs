using System;

namespace advisor;

public static class SetExtensions {
    public static bool In<T>(this T self, params T[] values) {
        return values is not null && values.Length > 0 && Array.IndexOf(values, self) >= 0;
    }
}
