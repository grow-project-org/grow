using Grow.Domain.Commons;
using Grow.Domain.Plants;

namespace Grow.Tests.Unit.Domain.Plants;

[TestFixture]
public class PlantTests
{
    private static Plant CreateSamplePlant() => Plant.Create(Guid.NewGuid(), "monstera-01", Guid.NewGuid());

    [Test]
    public void Create_ShouldSetProvidedValues()
    {
        var id = Guid.NewGuid();
        var specieId = Guid.NewGuid();

        var plant = Plant.Create(id, "monstera-01", specieId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(plant.Id, Is.EqualTo(id));
            Assert.That(plant.CustomId, Is.EqualTo("monstera-01"));
            Assert.That(plant.SpecieId, Is.EqualTo(specieId));
            Assert.That(plant.Events, Is.Empty);
            Assert.That(plant.PlantGroups, Is.Empty);
        }
    }

    [Test]
    public void SetCustomId_ShouldUpdateCustomIdAndUpdatedAt()
    {
        var plant = CreateSamplePlant();
        var updatedAtBefore = plant.UpdatedAt;

        plant.SetCustomId("monstera-02");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(plant.CustomId, Is.EqualTo("monstera-02"));
            Assert.That(plant.UpdatedAt, Is.GreaterThanOrEqualTo(updatedAtBefore));
        }
    }

    [Test]
    public void SetSpecieId_ShouldUpdateSpecieIdAndUpdatedAt()
    {
        var plant = CreateSamplePlant();
        var newSpecieId = Guid.NewGuid();
        var updatedAtBefore = plant.UpdatedAt;

        plant.SetSpecieId(newSpecieId);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(plant.SpecieId, Is.EqualTo(newSpecieId));
            Assert.That(plant.UpdatedAt, Is.GreaterThanOrEqualTo(updatedAtBefore));
        }
    }

    [Test]
    public void AddEvent_ShouldAddEventWithGivenDataAndUpdateUpdatedAt()
    {
        var plant = CreateSamplePlant();
        var eventId = Guid.NewGuid();
        var executedAt = DateTime.UtcNow.AddDays(-1);
        var updatedAtBefore = plant.UpdatedAt;

        plant.AddEvent(eventId, PlantActionType.Watering, executedAt);

        Assert.That(plant.Events, Has.Count.EqualTo(1));

        var @event = plant.Events.Single();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(@event.Id, Is.EqualTo(eventId));
            Assert.That(@event.PlantId, Is.EqualTo(plant.Id));
            Assert.That(@event.Type, Is.EqualTo(PlantActionType.Watering));
            Assert.That(@event.ExecutedAt, Is.EqualTo(executedAt));
            Assert.That(plant.UpdatedAt, Is.GreaterThanOrEqualTo(updatedAtBefore));
        }
    }

    [Test]
    public void AddEvent_CalledMultipleTimes_ShouldAccumulateEvents()
    {
        var plant = CreateSamplePlant();

        plant.AddEvent(Guid.NewGuid(), PlantActionType.Watering, DateTime.UtcNow);
        plant.AddEvent(Guid.NewGuid(), PlantActionType.Fertilizing, DateTime.UtcNow);

        Assert.That(plant.Events, Has.Count.EqualTo(2));
    }
}
