using Grow.Cqrs;
using Grow.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        var loggerMock = new Mock<ILogger<Dispatcher>>();

        var command = new SampleCommand();
        var handlerMock = new Mock<ICommandHandler<SampleCommand>>();

        var services = new ServiceCollection();
        _ = services.AddScoped(_ => handlerMock.Object);
        var dispatcher = new Dispatcher(services.BuildServiceProvider(), loggerMock.Object);

        await dispatcher.SendAsync(command, CancellationToken.None);

        handlerMock.Verify(h => h.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task QueryAsync_ShouldResolveMatchingHandlerAndReturnItsResult()
    {
        var loggerMock = new Mock<ILogger<Dispatcher>>();

        var query = new SampleQuery();
        var handlerMock = new Mock<IQueryHandler<SampleQuery, string>>();
        _ = handlerMock.Setup(h => h.HandleAsync(query, It.IsAny<CancellationToken>())).ReturnsAsync("result");

        var services = new ServiceCollection();
        _ = services.AddScoped(_ => handlerMock.Object);
        var dispatcher = new Dispatcher(services.BuildServiceProvider(), loggerMock.Object);

        var result = await dispatcher.QueryAsync<SampleQuery, string>(query, CancellationToken.None);

        Assert.That(result, Is.EqualTo("result"));
    }

    [Test]
    public void SendAsync_WhenCancellationRequested_ShouldThrowWithoutInvokingHandler()
    {
        var loggerMock = new Mock<ILogger<Dispatcher>>();

        var handlerMock = new Mock<ICommandHandler<SampleCommand>>();
        var services = new ServiceCollection();
        _ = services.AddScoped(_ => handlerMock.Object);
        var dispatcher = new Dispatcher(services.BuildServiceProvider(), loggerMock.Object);

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
        var loggerMock = new Mock<ILogger<Dispatcher>>();

        var services = new ServiceCollection();
        var dispatcher = new Dispatcher(services.BuildServiceProvider(), loggerMock.Object);

        _ = Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.SendAsync(new SampleCommand(), CancellationToken.None));
    }
}
