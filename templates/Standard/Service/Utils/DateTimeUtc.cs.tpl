namespace {{SolutionName}}.Service.Utils;

public static class DateTimeUtc
{
    public static DateTime StartOfDay(DateTime date) =>
        new(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime ExclusiveEndOfDay(DateTime date) => StartOfDay(date).AddDays(1);

    public static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : new DateTime(value.Ticks, DateTimeKind.Utc);
}
