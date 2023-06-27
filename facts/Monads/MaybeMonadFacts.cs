#nullable enable

namespace advisor.facts;

using static advisor.Prelude;

public class MaybeMonadFacts {
    // Monad laws

    // Q: What is a monad identity law?
    // A: The identity law states that if you take a monad and you bind it to a function that returns the same monad,
    //    you get the same monad back.

    [Fact]
    public void IdentityLaw() {
        var value = Some(42);

        Assert.Equal(value.Bind(x => Some(x)), value);
    }

    // Q: What is a monad associativity law?
    // A: The associativity law states that if you take a monad and you bind it to a function that returns another monad,
    //    and you bind that to a function that returns another monad, you get the same result as if you had taken the original
    //    monad and bound it to a function that returns another monad, and then bound the result to the second function.

    [Fact]
    public void AssociativityLaw() {
        var value = Some(42);

        var left = value
            .Bind(x => Some(x + 1))
            .Bind(x => Some(x + 1));

        var right = value
            .Bind(x => Some(x + 1)
                .Bind(x => Some(x + 1))
            );

        Assert.Equal(left, right);
    }

    [Fact]
    public void NullIsEqualToNone() {
        None<string>().Should().BeEquivalentTo((string?) null);
        None<int>().Should().BeEquivalentTo((int?) null);
        None<object>().Should().BeEquivalentTo((object?) null);
    }
}
