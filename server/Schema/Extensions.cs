namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using HotChocolate.Resolvers;
    using HotChocolate.Types;

    public static class GraphQLExtensions {
        public static ICollection<IFieldSelection> CollectSelections<T>(this IResolverContext context) {
            var list = new List<IFieldSelection>();

            CollectSelectionsRec<T>(context, context.Selection, list);

            return list;
        }

        public static ISet<string> CollectSelectedFields<T>(this IResolverContext context) {
            return new HashSet<string>(
                CollectSelections<T>(context)
                    .Select(x => x.Field.Member?.Name)
                    .Where(x => !string.IsNullOrEmpty(x))
                );
        }

        private static void CollectSelectionsRec<T>(
            IResolverContext context,
            IFieldSelection selection,
            ICollection<IFieldSelection> collected)
        {
            if (selection.Field.DeclaringType.RuntimeType == typeof(T)) {
                // if (selection.Type.IsLeafType()) {
                    collected.Add(selection);
                // }
            }
            else {
                if (selection.Type.NamedType() is ObjectType objectType) {
                    foreach (IFieldSelection child in context.GetSelections(objectType, selection.SyntaxNode.SelectionSet)) {
                        CollectSelectionsRec<T>(context, child, collected);
                    }
                }
            }
        }
    }
}
