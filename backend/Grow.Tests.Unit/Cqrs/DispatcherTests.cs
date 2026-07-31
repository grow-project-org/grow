using Grow.Cqrs;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Grow.Tests.Unit.Cqrs;

[TestFixture]
public class DispatcherTests
{
    public record SampleCommand : ICommand;
    public record SampleQuery : IQuery<string>;

    [Test]
    public async Task SendAsync_ShouldResolveAndInvokeMatchingHandler()
    {
        var command = new SampleCommand();
        var handlerMock = new Mock<ICommandHandler<SampleCommand>>();

        var services = new ServiceCollection();
        _ = services.AddScoped(_ => handlerMock.Object);
        var dispatcher = new Dispatcher(services.BuildServiceProvider());

        await dispatcher.SendAsync(command, CancellationToken.None);

        handlerMock.Verify(h => h.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task QueryAsync_ShouldResolveMatchingHandlerAndReturnItsResult()
    {
        var query = new SampleQuery();
        var handlerMock = new Mock<IQueryHandler<SampleQuery, string>>();
        _ = handlerMock.Setup(h => h.HandleAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync("result");

        var services = new ServiceCollection();
        _ = services.AddScoped(_ => handlerMock.Object);
        var dispatcher = new Dispatcher(services.BuildServiceProvider());

        var result = await dispatcher.QueryAsync<SampleQuery, string>(query, CancellationToken.None);

        Assert.That(result, Is.EqualTo("result"));
    }

    [Test]
    public void SendAsync_WhenCancellationRequested_ShouldThrowWithoutInvokingHandler()
    {
        var handlerMock = new Mock<ICommandHandler<SampleCommand>>();
        var services = new ServiceCollection();
        _ = services.AddScoped(_ => handlerMock.Object);
        var dispatcher = new Dispatcher(services.BuildServiceProvider());

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        using (Assert.EnterMultipleScope())
        {
            _ = Assert.ThrowsAsync<OperationCanceledException>(() => dispatcher.SendAsync(new SampleCommand(), cts.Token));
            handlerMock.Verify(h => h.HandleAsync(It.IsAny<SampleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Test]
    public void SendAsync_WhenNoHandlerRegistered_ShouldThrowInvalidOperationException()
    {
        var services = new ServiceCollection();
        var dispatcher = new Dispatcher(services.BuildServiceProvider());

        _ = Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.SendAsync(new SampleCommand(), CancellationToken.None));
    }
}
