namespace advisor;

using System;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;

public class CustomNamingConventions : DefaultNamingConventions {
    public CustomNamingConventions() : base() { }

    public CustomNamingConventions(IDocumentationProvider documentationProvider) : base(documentationProvider) { }

    public override NameString GetTypeName(Type type, TypeKind kind) {
        var name = base.GetTypeName(type, kind);
        if (!name.IsEmpty && kind == TypeKind.Object && name.Value.StartsWith("Db")) {
            name = name.Value.Substring(2);
        }

        return name;
    }
}
