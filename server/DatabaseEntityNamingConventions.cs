namespace advisor;

using System;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

public class DatabaseEntityNamingConventions : DefaultNamingConventions {
    public DatabaseEntityNamingConventions() : base() { }

    public DatabaseEntityNamingConventions(IDocumentationProvider documentationProvider) : base(documentationProvider) { }

    public override NameString GetTypeName(Type type, TypeKind kind) {
        var name = base.GetTypeName(type, kind);
        if (!name.IsEmpty && kind == TypeKind.Object && name.Value.StartsWith("Db")) {
            name = name.Value[2..];
        }

        return name;
    }
}
