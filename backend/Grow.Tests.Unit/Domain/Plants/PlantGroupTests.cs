using Grow.Domain.Plants;

namespace Grow.Tests.Unit.Domain.Plants;

[TestFixture]
public class PlantGroupTests
{
    private static Plant CreateSamplePlant() => Plant.Create(Guid.NewGuid(), "monstera-01", Guid.NewGuid());

    [Test]
    public void CreateWorkGroup_ShouldSetNameAndType()
    {
        var id = Guid.NewGuid();

        var group = PlantGroup.CreateWorkGroup(id, "Balcony");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(group.Id, Is.EqualTo(id));
            Assert.That(group.Name, Is.EqualTo("Balcony"));
            Assert.That(group.Type, Is.EqualTo(GroupType.WorkGroup));
            Assert.That(group.PlantIds, Is.Empty);
        }
    }

    [Test]
    public void CreateRegion_ShouldSetType()
        => Assert.That(PlantGroup.CreateRegion(Guid.NewGuid(), "Kitchen").Type, Is.EqualTo(GroupType.Region));

    [Test]
    public void CreateTemporaryGroup_ShouldSetType()
        => Assert.That(PlantGroup.CreateTemporaryGroup(Guid.NewGuid(), "Quarantine").Type, Is.EqualTo(GroupType.TemporaryGroup));

    [Test]
    public void AddPlant_ShouldAddPlantToGroup()
    {
        var group = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Balcony");
        var plant = CreateSamplePlant();

        group.AddPlant(plant.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(group.PlantIds, Has.Count.EqualTo(1));
            Assert.That(group.PlantIds, Does.Contain(plant.Id));
        }
    }

    [Test]
    public void AddPlant_WhenPlantAlreadyInGroup_ShouldNotAddDuplicate()
    {
        var group = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Balcony");
        var plant = CreateSamplePlant();
        group.AddPlant(plant.Id);

        group.AddPlant(plant.Id);

        Assert.That(group.PlantIds, Has.Count.EqualTo(1));
    }

    [Test]
    public void RemovePlant_ShouldRemovePlantFromGroup()
    {
        var group = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Balcony");
        var plant = CreateSamplePlant();
        group.AddPlant(plant.Id);

        group.RemovePlant(plant.Id);

        Assert.That(group.PlantIds, Is.Empty);
    }

    [Test]
    public void RemovePlant_WhenPlantNotInGroup_ShouldNotThrow()
    {
        var group = PlantGroup.CreateWorkGroup(Guid.NewGuid(), "Balcony");

        Assert.DoesNotThrow(() => group.RemovePlant(CreateSamplePlant().Id));
    }
}
