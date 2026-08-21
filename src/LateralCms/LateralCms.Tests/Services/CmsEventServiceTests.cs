using LateralCms.Application.Interfaces.Persistence;
using LateralCms.Application.Interfaces.Queue;
using LateralCms.Application.Services;
using LateralCms.Application.Services.Contracts;
using LateralCms.Domain.Entities;
using LateralCms.Domain.Enumerations;
using MapsterMapper;
using Moq;

namespace LateralCms.Tests.Services;

public sealed class CmsEventServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<CmsEvent>> _eventRepositoryMock;
    private readonly Mock<ICmsEventQueue> _queueMock;
    private readonly Mock<ICmsEntityService> _entityServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CmsEventService _sut;

    public CmsEventServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _eventRepositoryMock = new Mock<IRepository<CmsEvent>>(MockBehavior.Strict);
        _queueMock = new Mock<ICmsEventQueue>(MockBehavior.Strict);
        _entityServiceMock = new Mock<ICmsEntityService>(MockBehavior.Strict);
        _mapperMock = new Mock<IMapper>(MockBehavior.Strict);

        _unitOfWorkMock
            .SetupGet(unitOfWork => unitOfWork.CmsEvents)
            .Returns(_eventRepositoryMock.Object);

        _sut = new CmsEventService(
            _unitOfWorkMock.Object,
            _queueMock.Object,
            _entityServiceMock.Object,
            _mapperMock.Object);
    }

    //[Fact]
    //public async Task ReceiveAsync_WithEvents_PersistsAndQueuesPendingBatch()
    //{
    //    var mapperMock = _mapperMock;
    //    var eventRepositoryMock = _eventRepositoryMock;
    //    var unitOfWorkMock = _unitOfWorkMock;
    //    var queueMock = _queueMock;
    //    var cancellationToken = new CancellationTokenSource().Token;
    //    var input = new[]
    //    {
    //        new CmsEventInput
    //        {
    //            EntityId = "article-1",
    //            Type = "Add",
    //            Payload = "{\"title\":\"First\"}",
    //            Version = 1
    //        },
    //        new CmsEventInput
    //        {
    //            EntityId = "article-2",
    //            Type = "Publish",
    //            Version = 3
    //        }
    //    };
    //    List<CmsEvent>? persistedEvents = null;
    //    var enqueuedEventIds = new List<Guid>();

    //    mapperMock
    //        .Setup(mapper => mapper.Map<CmsEvent>(It.IsAny<object>()))
    //        .Returns((object source) =>
    //        {
    //            var eventInput = Assert.IsType<CmsEventInput>(source);

    //            return new CmsEvent
    //            {
    //                EntityId = eventInput.EntityId,
    //                Type = eventInput.Type,
    //                Payload = eventInput.Payload,
    //                Version = eventInput.Version
    //            };
    //        });

    //    eventRepositoryMock
    //        .Setup(repository => repository.AddRangeAsync(
    //            It.IsAny<IEnumerable<CmsEvent>>(),
    //            cancellationToken))
    //        .Callback<IEnumerable<CmsEvent>, CancellationToken>((events, _) =>
    //        {
    //            persistedEvents = events.ToList();

    //            foreach (var cmsEvent in persistedEvents)
    //            {
    //                cmsEvent.Id = Guid.NewGuid();
    //            }
    //        })
    //        .Returns(Task.CompletedTask);

    //    unitOfWorkMock
    //        .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
    //        .ReturnsAsync(2);

    //    queueMock
    //        .Setup(queue => queue.EnqueueAsync(
    //            It.IsAny<Guid>(),
    //            cancellationToken))
    //        .Callback<Guid, CancellationToken>((eventId, _) =>
    //            enqueuedEventIds.Add(eventId))
    //        .Returns(ValueTask.CompletedTask);

    //    var startedAt = DateTime.UtcNow;

    //    var output = await _sut.ReceiveAsync(input, cancellationToken);

    //    var completedAt = DateTime.UtcNow;
    //    Assert.NotNull(persistedEvents);
    //    Assert.NotEqual(Guid.Empty, output.BatchId);
    //    Assert.Equal(2, persistedEvents.Count);

    //    Assert.All(persistedEvents, cmsEvent =>
    //    {
    //        Assert.NotEqual(Guid.Empty, cmsEvent.Id);
    //        Assert.Equal(output.BatchId, cmsEvent.BatchId);
    //        Assert.Equal(EventStatus.Pending, cmsEvent.Status);
    //        Assert.InRange(cmsEvent.ReceivedAt, startedAt, completedAt);
    //    });

    //    Assert.Collection(
    //        persistedEvents,
    //        cmsEvent =>
    //        {
    //            Assert.Equal("article-1", cmsEvent.EntityId);
    //            Assert.Equal("Add", cmsEvent.Type);
    //            Assert.Equal("{\"title\":\"First\"}", cmsEvent.Payload);
    //            Assert.Equal(1, cmsEvent.Version);
    //        },
    //        cmsEvent =>
    //        {
    //            Assert.Equal("article-2", cmsEvent.EntityId);
    //            Assert.Equal("Publish", cmsEvent.Type);
    //            Assert.Null(cmsEvent.Payload);
    //            Assert.Equal(3, cmsEvent.Version);
    //        });

    //    var persistedIds = persistedEvents.Select(cmsEvent => cmsEvent.Id).ToList();
    //    Assert.Equal(persistedIds, enqueuedEventIds);
    //    Assert.Equal(persistedIds, output.EventsIds);

    //    mapperMock.Verify(
    //        mapper => mapper.Map<CmsEvent>(It.IsAny<object>()),
    //        Times.Exactly(input.Length));
    //    eventRepositoryMock.Verify(
    //        repository => repository.AddRangeAsync(
    //            It.IsAny<IEnumerable<CmsEvent>>(),
    //            cancellationToken),
    //        Times.Once);
    //    unitOfWorkMock.Verify(
    //        unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
    //        Times.Once);
    //    queueMock.Verify(
    //        queue => queue.EnqueueAsync(It.IsAny<Guid>(), cancellationToken),
    //        Times.Exactly(input.Length));
    //}

    //[Theory]
    //[InlineData("ADD", EventType.Add)]
    //[InlineData("update", EventType.Update)]
    //[InlineData("Delete", EventType.Delete)]
    //[InlineData("PUBLISH", EventType.Publish)]
    //[InlineData("Unpublish", EventType.Unpublish)]
    //public async Task ProcessAsync_WithSupportedType_DelegatesToExpectedEntityOperation(string eventTypeInput,
    //    EventType expectedEventType)
    //{
    //    var cancellationToken = new CancellationTokenSource().Token;
    //    var cmsEvent = new CmsEvent
    //    {
    //        Id = Guid.NewGuid(),
    //        EntityId = "article-1",
    //        Type = eventTypeInput,
    //        Payload = "{\"title\":\"Updated\"}",
    //        Version = 4
    //    };

    //    SetupExpectedEntityOperation(
    //        expectedEventType,
    //        cmsEvent,
    //        cancellationToken);

    //    await _sut.ProcessAsync(cmsEvent, cancellationToken);

    //    _mapperMock.VerifyAll();
    //    _entityServiceMock.VerifyAll();
    //    _mapperMock.VerifyNoOtherCalls();
    //    _entityServiceMock.VerifyNoOtherCalls();
    //}

    //[Fact]
    //public async Task ProcessAsync_WithUnsupportedType_ThrowsInvalidOperationException()
    //{
    //    var cmsEvent = new CmsEvent
    //    {
    //        Id = Guid.NewGuid(),
    //        Type = "Archive"
    //    };

    //    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
    //        () => _sut.ProcessAsync(cmsEvent));

    //    Assert.Contains(cmsEvent.Id.ToString(), exception.Message);
    //    Assert.Contains("Archive", exception.Message);
    //    _mapperMock.VerifyNoOtherCalls();
    //    _entityServiceMock.VerifyNoOtherCalls();
    //}

    //[Fact]
    //public async Task ProcessAsync_WithNullEvent_ThrowsArgumentNullException()
    //{
    //    await Assert.ThrowsAsync<ArgumentNullException>(
    //        () => _sut.ProcessAsync(null!));

    //    _mapperMock.VerifyNoOtherCalls();
    //    _entityServiceMock.VerifyNoOtherCalls();
    //}

    //[Fact]
    //public async Task ProcessAsync_WithCancelledToken_DoesNotInvokeEntityService()
    //{
    //    var cancellationToken = new CancellationToken(canceled: true);
    //    var cmsEvent = new CmsEvent { Type = "Add" };

    //    await Assert.ThrowsAnyAsync<OperationCanceledException>(
    //        () => _sut.ProcessAsync(cmsEvent, cancellationToken));

    //    _mapperMock.VerifyNoOtherCalls();
    //    _entityServiceMock.VerifyNoOtherCalls();
    //}

    //private void SetupExpectedEntityOperation(EventType expectedEventType, CmsEvent cmsEvent,
    //    CancellationToken cancellationToken)
    //{
    //    var mapperMock = _mapperMock;
    //    var entityServiceMock = _entityServiceMock;

    //    switch (expectedEventType)
    //    {
    //        case EventType.Add:
    //            var addInput = new CmsEntityInput
    //            {
    //                Id = cmsEvent.EntityId,
    //                Payload = cmsEvent.Payload
    //            };

    //            mapperMock
    //                .Setup(mapper => mapper.Map<CmsEntityInput>(cmsEvent))
    //                .Returns(addInput);
    //            entityServiceMock
    //                .Setup(service => service.AddEntityAsync(
    //                    It.Is<CmsEntityInput>(input =>
    //                        input.Id == cmsEvent.EntityId &&
    //                        input.Payload == cmsEvent.Payload),
    //                    cancellationToken))
    //                .ReturnsAsync(new CmsEntityOutput());
    //            break;

    //        case EventType.Update:
    //            var updateInput = new EntityPayloadUpdateInput
    //            {
    //                EntityId = cmsEvent.EntityId,
    //                Payload = cmsEvent.Payload
    //            };

    //            mapperMock
    //                .Setup(mapper => mapper.Map<EntityPayloadUpdateInput>(cmsEvent))
    //                .Returns(updateInput);
    //            entityServiceMock
    //                .Setup(service => service.UpdateEntityAsync(
    //                    It.Is<EntityPayloadUpdateInput>(input =>
    //                        input.EntityId == cmsEvent.EntityId &&
    //                        input.Payload == cmsEvent.Payload),
    //                    cancellationToken))
    //                .ReturnsAsync(new CmsEntityOutput());
    //            break;

    //        case EventType.Delete:
    //            entityServiceMock
    //                .Setup(service => service.DeleteEntityAsync(
    //                    cmsEvent.EntityId,
    //                    cancellationToken))
    //                .Returns(Task.CompletedTask);
    //            break;

    //        case EventType.Publish:
    //            entityServiceMock
    //                .Setup(service => service.PublishEntityAsync(
    //                    cmsEvent.EntityId,
    //                    cmsEvent.Version,
    //                    cancellationToken))
    //                .Returns(Task.CompletedTask);
    //            break;

    //        case EventType.Unpublish:
    //            entityServiceMock
    //                .Setup(service => service.UnpublishEntityAsync(
    //                    cmsEvent.EntityId,
    //                    cancellationToken))
    //                .Returns(Task.CompletedTask);
    //            break;

    //        default:
    //            throw new ArgumentOutOfRangeException(
    //                nameof(expectedEventType),
    //                expectedEventType,
    //                null);
    //    }
    //}
}
