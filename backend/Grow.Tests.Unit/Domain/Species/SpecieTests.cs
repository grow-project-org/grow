using Grow.Domain.Commons;
using Grow.Domain.Species;

namespace Grow.Tests.Unit.Domain.Species;

[TestFixture]
public class SpecieTests
{
    private static Specie CreateSampleSpecie() => Specie.Create(Guid.NewGuid(), "Monstera Deliciosa", Guid.NewGuid());

    [Test]
    public void Create_ShouldSetIdAndName()
    {
        var id = Guid.NewGuid();

        var specie = Specie.Create(id, "Monstera Deliciosa", Guid.NewGuid());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(specie.Id, Is.EqualTo(id));
            Assert.That(specie.Name, Is.EqualTo("Monstera Deliciosa"));
            Assert.That(specie.Intervals, Is.Empty);
        }
    }

    [Test]
    public void SetWateringInterval_ShouldAddInterval()
    {
        var specie = CreateSampleSpecie();
        var interval = TimeSpan.FromDays(7);

        specie.SetWateringInterval(interval);

        Assert.That(specie.Intervals[PlantActionType.Watering], Is.EqualTo(interval));
    }

    [Test]
    public void SetWateringInterval_CalledTwice_ShouldUpdateExistingInterval()
    {
        var specie = CreateSampleSpecie();
        specie.SetWateringInterval(TimeSpan.FromDays(7));

        specie.SetWateringInterval(TimeSpan.FromDays(3));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(specie.Intervals[PlantActionType.Watering], Is.EqualTo(TimeSpan.FromDays(3)));
            Assert.That(specie.Intervals, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void SetFertilizingInterval_ShouldAddInterval()
    {
        var specie = CreateSampleSpecie();
        var interval = TimeSpan.FromDays(30);

        specie.SetFertilizingInterval(interval);

        Assert.That(specie.Intervals[PlantActionType.Fertilizing], Is.EqualTo(interval));
    }

    [Test]
    public void SetIntervals_ForDifferentActionTypes_ShouldKeepBothEntries()
    {
        var specie = CreateSampleSpecie();

        specie.SetWateringInterval(TimeSpan.FromDays(7));
        specie.SetFertilizingInterval(TimeSpan.FromDays(30));

        Assert.That(specie.Intervals, Has.Count.EqualTo(2));
    }
}
