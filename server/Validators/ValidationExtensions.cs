namespace advisor.Validators;

using System.Threading.Tasks;
using LanguageExt.Effects.Traits;

public static class ValidationExtensions {
    /// <summary>
    /// Run the async effect wrapped in a Validation and then replaces the value in the Validation with the result of the effect.
    /// </summary>
    public static Task<Validation<Error, A>> Unwrap<RT, A>(this Validation<Error, Aff<RT, A>> wrappedAffect, RT runtime)
        where RT : struct, HasCancel<RT> =>
        wrappedAffect.MatchAsync(
            SuccAsync: async affect =>
                (await affect.Run(runtime))
                .Match(
                    Succ: Success<Error, A>,
                    Fail: Fail<Error, A>
                ),
            Fail: Fail<Error, A>
        );

    /// <summary>
    /// Prepand the field name to the error messages in the validation.
    /// </summary>
    public static Validation<Error, A> ForField<A>(this Validation<Error, A> validation, string field) =>
        validation.Match(
            Succ: Success<Error, A>,
            Fail: err => Fail<Error, A>(err.Map(e => Error.New($"{field}: {e.Message}", e)))
        );
}
