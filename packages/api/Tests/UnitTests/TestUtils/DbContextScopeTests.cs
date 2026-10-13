using Task = System.Threading.Tasks.Task;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Moq;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Xunit;

namespace Tests.UnitTests.TestUtils;

public class DbContextScopeTests
{
    [Fact]
    public async Task AsAtomic_PublishedMessage_IsPublishedThroughOutboxAndFlushedAfterCommit()
    {
        var context = new AlvTimeDbContextBuilder().CreateDbContext();
        var outbox = new Mock<IDbContextOutbox<AlvTime_dbContext>>();
        var scope = new DbContextScope(context, outbox.Object);
        var message = new object();

        await scope.AsAtomic(async () => await scope.PublishAsync(message));

        outbox.Verify(o => o.PublishAsync(message, It.IsAny<DeliveryOptions>()), Times.Once);
        outbox.Verify(o => o.FlushOutgoingMessagesAsync(), Times.Once);
    }
}
