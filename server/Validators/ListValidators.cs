namespace advisor.Validators;

using System;
using System.Linq;
using System.Collections.Generic;

public static class ListValidators {
    public static Validation<Error, List<A>> NotEmpty<A>(List<A> input) =>
        Optional(input)
            .Where(x => x.Count > 0)
            .ToValidation(Error.New("List cannot be empty."));

    public static Func<List<A>, Validation<Error, List<A>>> WithinLength<A>(Option<int> min, Option<int> max) =>
        input => {
            var minLen = min.Match(Some: x => x, None: 0);
            var maxLen = max.Match(Some: x => x, None: int.MaxValue);

            return Optional(input)
                .Where(x => x.Count >= minLen && x.Count <= maxLen)
                .ToValidation(Error.New($"List must have between {minLen} and {maxLen} items."));
        };

    public static Func<List<A>, Validation<Error, List<A>>> Unique<A>() =>
        input =>
            Optional(input)
                .Where(x => x.Distinct().Count() == x.Count)
                .ToValidation(Error.New("List must have unique items."));
}
