namespace advisor.Model;

using System;

public record GamePeriod(Option<DateTimeOffset> StartAt, Option<DateTimeOffset> FinishAt) {
    public static Validation<Error, GamePeriod> New(Option<DateTimeOffset> startAt, Option<DateTimeOffset> finishAt) =>
        startAt.Match(
            Some: s => finishAt.Match(
                Some: f => s < f
                    ? Success<Error, GamePeriod>(new GamePeriod(startAt, finishAt))
                    : Fail<Error, GamePeriod>(Error.New("Start date must be before finish date.")),
                None: () => Success<Error, GamePeriod>(new GamePeriod(startAt, None))
            ),
            None: () => finishAt.Match(
                Some: f => Success<Error, GamePeriod>(new GamePeriod(None, finishAt)),
                None: () => Success<Error, GamePeriod>(new GamePeriod(None, None))
            )
        );
}
