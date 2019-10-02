namespace atlantis
{
    using System;
    using Pidgin;
    using static Pidgin.Parser;

    // public sealed class SeparatedMap<TToken, T1, R> : Parser<TToken, R>
    // {
    //     private readonly Func<T1, R> _func;
    //     private readonly Parser<TToken, T1> _p1;

    //     public SeparatedMap(
    //         Func<T1, R> func,
    //         Parser<TToken, T1> parser1
    //     )
    //     {
    //         _func = func;
    //         _p1 = parser1;
    //     }

    //     internal sealed override InternalResult<R> Parse(ref ParseState<TToken> state)
    //     {
    //         var consumedInput = false;


    //         var result1 = _p1.Parse(ref state);
    //         consumedInput = consumedInput || result1.ConsumedInput;
    //         if (!result1.Success)
    //         {
    //             return InternalResult.Failure<R>(consumedInput);
    //         }

    //         return InternalResult.Success<R>(_func(
    //             result1.Value
    //         ), consumedInput);
    //     }

    //     internal override MapParserBase<TToken, U> Map<U>(Func<R, U> func)
    //         => new Map1Parser<TToken, T1, U>(
    //             (x1) => func(_func(x1)),
    //             _p1
    //         );

    //     internal override InternalResult<R> Parse(ref ParseState<TToken> state)
    //     {
    //         throw new NotImplementedException();
    //     }
    // }

    public static class Parsers {
        public static Parser<TToken, R> SeparatedMap<TToken, T1, T2, R>(
            Parser<TToken, Unit> separator,
            Parser<TToken, T1> p1,
            Parser<TToken, T2> p2,
            Func<T1, T2, R> project) {
                return Map(
                    project,
                    p1.Before(separator),
                    p2
                );
        }

        public static Parser<TToken, R> SeparatedMap<TToken, T1, T2, T3, R>(
            Parser<TToken, Unit> separator,
            Parser<TToken, T1> p1,
            Parser<TToken, T2> p2,
            Parser<TToken, T3> p3,
            Func<T1, T2, T3, R> project) {
                return Map(
                    project,
                    p1.Before(separator),
                    p2.Before(separator),
                    p3
                );
        }

        public static Parser<TToken, R> SeparatedMap<TToken, T1, T2, T3, T4, R>(
            Parser<TToken, Unit> separator,
            Parser<TToken, T1> p1,
            Parser<TToken, T2> p2,
            Parser<TToken, T3> p3,
            Parser<TToken, T4> p4,
            Func<T1, T2, T3, T4, R> project) {
                return Map(
                    project,
                    p1.Before(separator),
                    p2.Before(separator),
                    p3.Before(separator),
                    p4
                );
        }

        public static Parser<TToken, R> SeparatedMap<TToken, T1, T2, T3, T4, T5, R>(
            Parser<TToken, Unit> separator,
            Parser<TToken, T1> p1,
            Parser<TToken, T2> p2,
            Parser<TToken, T3> p3,
            Parser<TToken, T4> p4,
            Parser<TToken, T5> p5,
            Func<T1, T2, T3, T4, T5, R> project) {
                return Map(
                    project,
                    p1.Before(separator),
                    p2.Before(separator),
                    p3.Before(separator),
                    p4.Before(separator),
                    p5
                );
        }

        public static Parser<TToken, R> SeparatedMap<TToken, T1, T2, T3, T4, T5, T6, R>(
            Parser<TToken, Unit> separator,
            Parser<TToken, T1> p1,
            Parser<TToken, T2> p2,
            Parser<TToken, T3> p3,
            Parser<TToken, T4> p4,
            Parser<TToken, T5> p5,
            Parser<TToken, T6> p6,
            Func<T1, T2, T3, T4, T5, T6, R> project) {
                return Map(
                    project,
                    p1.Before(separator),
                    p2.Before(separator),
                    p3.Before(separator),
                    p4.Before(separator),
                    p5.Before(separator),
                    p6
                );
        }

        public static Parser<TToken, R> SeparatedMap<TToken, T1, T2, T3, T4, T5, T6, T7, R>(
            Parser<TToken, Unit> separator,
            Parser<TToken, T1> p1,
            Parser<TToken, T2> p2,
            Parser<TToken, T3> p3,
            Parser<TToken, T4> p4,
            Parser<TToken, T5> p5,
            Parser<TToken, T6> p6,
            Parser<TToken, T7> p7,
            Func<T1, T2, T3, T4, T5, T6, T7, R> project) {
                return Map(
                    project,
                    p1.Before(separator),
                    p2.Before(separator),
                    p3.Before(separator),
                    p4.Before(separator),
                    p5.Before(separator),
                    p6.Before(separator),
                    p7
                );
        }

        public static Parser<TToken, R> SeparatedMap<TToken, T1, T2, T3, T4, T5, T6, T7, T8, R>(
            Parser<TToken, Unit> separator,
            Parser<TToken, T1> p1,
            Parser<TToken, T2> p2,
            Parser<TToken, T3> p3,
            Parser<TToken, T4> p4,
            Parser<TToken, T5> p5,
            Parser<TToken, T6> p6,
            Parser<TToken, T7> p7,
            Parser<TToken, T8> p8,
            Func<T1, T2, T3, T4, T5, T6, T7, T8, R> project) {
                return Map(
                    project,
                    p1.Before(separator),
                    p2.Before(separator),
                    p3.Before(separator),
                    p4.Before(separator),
                    p5.Before(separator),
                    p6.Before(separator),
                    p7.Before(separator),
                    p8
                );
        }
    }
}
