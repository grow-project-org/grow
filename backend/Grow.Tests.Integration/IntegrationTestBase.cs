namespace Grow.Tests.Integration;

public abstract class IntegrationTestBase
{
    protected HttpClient client = null!;
    protected GrowApiFactory factory = null!;

    [SetUp]
    public void BaseSetUp()
    {
        this.factory = new GrowApiFactory();
        this.client = this.factory.CreateClient();
    }

    [TearDown]
    public void BaseTearDown()
    {
        this.client.Dispose();
        this.factory.Dispose();
    }
}
