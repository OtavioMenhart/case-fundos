using Bogus;
using CaseItau.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CaseItau.Infra.Repositories.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="UnitOfWork"/> class.
    /// </summary>
    public class UnitOfWorkTests
    {
        private readonly Faker _faker;

        public UnitOfWorkTests()
        {
            _faker = new Faker();
        }

        /// <summary>
        /// Tests that the constructor accepts a null context at runtime despite the non-nullable parameter annotation.
        /// Input: null context.
        /// Expected: UnitOfWork instance is created without throwing exceptions (nullable reference types are compile-time only).
        /// </summary>
        [Fact]
        public void Constructor_WithNullContext_CreatesInstanceWithoutException()
        {
            // Arrange
            AppDbContext? context = null;

            // Act
            UnitOfWork unitOfWork = new UnitOfWork(context!);

            // Assert
            Assert.NotNull(unitOfWork);
        }

        /// <summary>
        /// Tests that CommitAsync completes successfully when SaveChangesAsync succeeds.
        /// Input: SaveChangesAsync returns successfully with entity count.
        /// Expected: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public async Task CommitAsync_WhenSaveChangesSucceeds_CompletesWithoutException()
        {
            // Arrange
            Mock<AppDbContext> contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            UnitOfWork unitOfWork = new UnitOfWork(contextMock.Object);
            CancellationToken token = CancellationToken.None;
            int savedEntitiesCount = _faker.Random.Int(1, 50);
            contextMock
                .Setup(c => c.SaveChangesAsync(token))
                .ReturnsAsync(savedEntitiesCount);

            // Act
            await unitOfWork.CommitAsync(token);

            // Assert
            contextMock.Verify(c => c.SaveChangesAsync(token), Times.Once);
        }

        /// <summary>
        /// Tests that CommitAsync handles zero changes from SaveChangesAsync.
        /// Input: SaveChangesAsync returns 0 (no changes to save).
        /// Expected: Method completes successfully without exceptions.
        /// </summary>
        [Fact]
        public async Task CommitAsync_WhenNoChangesToSave_CompletesSuccessfully()
        {
            // Arrange
            Mock<AppDbContext> contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            UnitOfWork unitOfWork = new UnitOfWork(contextMock.Object);
            CancellationToken token = CancellationToken.None;
            contextMock
                .Setup(c => c.SaveChangesAsync(token))
                .ReturnsAsync(0);

            // Act
            await unitOfWork.CommitAsync(token);

            // Assert
            contextMock.Verify(c => c.SaveChangesAsync(token), Times.Once);
        }

    }
}