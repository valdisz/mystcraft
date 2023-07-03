namespace advisor.Validators;

using System;

public static class StringValidators {
    public static Validation<Error, string> NotEmpty(string input) =>
        Optional(input)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToValidation(E_VALUE_CANNOT_BE_EMPTY);

    public static Func<string, Validation<Error, string>> WithinLength(Option<int> min, Option<int> max) =>
        input => {
            var minLen = min.Match(Some: x => x, None: 0);
            var maxLen = max.Match(Some: x => x, None: int.MaxValue);

            return Optional(input)
                .Where(x => x.Length >= minLen && x.Length <= maxLen)
                .ToValidation(E_VALUE_MUST_BE_WITHIN_LENGTH(minLen, maxLen));
        };
}
