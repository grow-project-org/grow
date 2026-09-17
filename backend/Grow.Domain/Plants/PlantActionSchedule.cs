namespace Grow.Domain.Plants;

public class PlantActionSchedule
{
    public static DateOnly CalculateNextDate(DateTime executedAt, TimeSpan interval)
    {
        var nextDate = executedAt + interval;
        return DateOnly.FromDateTime(nextDate);
    }
}
