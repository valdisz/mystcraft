namespace advisor;

using System;

public sealed record Error(string Message, Exception Exception = null) {
    public static readonly Error NotFound = new Error("Not found");
}
