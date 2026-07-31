using Grow.Commons.Extensions;

namespace Grow.Tests.Unit.Commons;

[TestFixture]
public class DictionaryExtensionsTests
{
    [Test]
    public void AddOrUpdate_WhenKeyDoesNotExist_ShouldAddIt()
    {
        var dict = new Dictionary<string, int>();

        dict.AddOrUpdate("a", 1);

        Assert.That(dict["a"], Is.EqualTo(1));
    }

    [Test]
    public void AddOrUpdate_WhenKeyExists_ShouldUpdateValue()
    {
        var dict = new Dictionary<string, int> { ["a"] = 1 };

        dict.AddOrUpdate("a", 2);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dict["a"], Is.EqualTo(2));
            Assert.That(dict, Has.Count.EqualTo(1));
        }
    }
}
