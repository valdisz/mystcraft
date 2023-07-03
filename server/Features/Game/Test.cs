using System;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;
using LanguageExt.Common;

namespace LanguageExtQuestions;

internal class Program2
{
    // This is an example of some language ext features and functional design.

    //
    // Step 1: Use the compiler to make it impossible to construct an invalid request
    //

    /// This is a validated domain object. The only way to create one is the New method
    /// which forces the user to handle the validation case.
    public record Request
    {
        public readonly string Name;

        Request(string name) =>
            Name = name;

        public static Validation<string, Request> New(string? name) =>
            // the string must not be empty or null
            from nonNullName in
                string.IsNullOrWhiteSpace(name)
                    ? Fail<string, string>("Name must not be null or empty")
                    : Success<string, string>(name)
            // now we know its not, we can validate these together and collect multiple errors
            from correctLength in (
                (nonNullName.Length < 2
                    ? Fail<string, string>("Name must be greater than 2 characters")
                    : Success<string, string>(nonNullName))
                | (nonNullName.StartsWith("TEST")
                    ? Fail<string, string>("Name must NOT start with TEST")
                    : Success<string, string>(nonNullName))
                )
            select new Request(correctLength);
    }

    //
    // Step 2: Select a container type with the correct features to solve the problem
    //

    // looks like we need async + error. Aff<T> fits this, and is also lazy which is a bonus
    // we will be using it to compose our application

    public static Aff<Guid> CreateEnvironment(Request request) => (
            // linq can be used to compose Aff values together
            from envId in GetNewEnvironment(request)
            // they can have fallback values on failure
            // here it will release if the call pipeline fails
            from _1 in CallPipeline(envId, request) | ReleaseEnvironmentDueToError(envId)
            select envId)
        // they can be retried on failure with strategies for scheduling
        // this will retry 6 times with a exponential backoff starting at 100 ms with a random jitter
        .Retry(Schedule.exponential(100 * ms) | Schedule.recurs(6) | Schedule.jitter());

    static Aff<Guid> GetNewEnvironment(Request request) =>
        // this is a constructor function for an Aff
        // it takes a func returning a ValueTask<Fin<T>>
        // I find it helps to put in the type of the generic return value,
        // this allows for the implicit conversions on Fin to kick in.
        // Fin is an alternative value container type, that either succeeds with a T or fails with an Error
        AffMaybe<Guid>(
            async () =>
            {
                // we can return error at any point
                if (request.Name == "FAIL")
                    // the error type gives us a code and message and can wrap exceptions
                    return Error.New(123, "Some failure reason");

                await Task.Delay(2 * seconds);

                // otherwise we just return the success value, which will convert to a Fin.Success<T>
                // in our case Guid
                return Guid.NewGuid();
            });

    // assume this does something, with the id and request
    static Aff<Unit> CallPipeline(Guid id, Request request) =>
        // we can run multiple steps in parallel
        from _1 in Seq(Step1(), Step2(), Step3()).SequenceParallel()
        // we can lift/mix in synchronous effects into the mix
        from _2 in Step4()
        select unit;

    // now we combine the validation and aff container types, this is done
    // by lifting the validation into an aff

    static async Task Main(string[] args)
    {
        var effectToRun =
            from request in
                Request.New(args.FirstOrDefault())
                    .ToAff(errors => Error.New(errors.Aggregate((allErrors, error) => allErrors + "\n" + error)))
            from id in CreateEnvironment(request)
            select id;
        // we need to run an aff, they do nothing/are lazy until they are run like IEnumerable does nothing until it is iterated
        var result = await effectToRun.Run();
        // the result is either a success or failure, so log it how you like
        // or just throw it with a result.ThrowIfFail()
        result.Match(
            id =>
            {
                // log here
                Console.WriteLine($"Environment created against {id}");
            },
            error =>
            {
                // log here
                Console.WriteLine($"Failed to create environment {error.Exception}");
            });
    }

    static Aff<Unit> Step1() =>
        unitAff;

    static Aff<Unit> Step2() =>
        unitAff;

    static Aff<Unit> Step3() =>
        unitAff;

    static Eff<Unit> Step4() =>
        unitEff;

    static Aff<Unit> ReleaseEnvironmentDueToError(Guid envId) =>
        unitAff;
}
