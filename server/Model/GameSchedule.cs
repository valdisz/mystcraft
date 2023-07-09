namespace advisor.Model;

/// <summary>
/// Game schedule.
/// </summary>
public record GameSchedule(
    string Cron,
    string TimeZone
) {
    public static Validation<Error, GameSchedule> New(string cron, string timeZone) =>
        (
            NotEmpty(cron)
                .Bind(WithinLength(1, None)),

            NotEmpty(timeZone)
                .Bind(WithinLength(1, None))
        )
            .Apply((c, tz) => new GameSchedule(c, tz));
}
