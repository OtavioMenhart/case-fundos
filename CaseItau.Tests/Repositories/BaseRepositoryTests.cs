using Bogus;
using CaseItau.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace CaseItau.Infra.Repositories.UnitTests
{
    public class BaseRepositoryTests
    {
        /// <summary>
        /// Simple test entity class used for testing the generic BaseRepository.
        /// </summary>
        private class TestEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        /// <summary>
        /// Testable derived class that exposes protected members of BaseRepository for verification.
        /// </summary>
        private class TestableBaseRepository : BaseRepository<TestEntity>
        {
            public TestableBaseRepository(AppDbContext context) : base(context) { }

            public AppDbContext ExposedContext => _context;
            public DbSet<TestEntity> ExposedDbSet => _dbSet;
        }

        /// <summary>
        /// Tests that the constructor throws NullReferenceException when provided with a null context.
        /// Input: Null context parameter.
        /// Expected: NullReferenceException is thrown when attempting to call Set&lt;TEntity&gt;() on null context.
        /// </summary>
        [Fact]
        public void Constructor_WithNullContext_ThrowsNullReferenceException()
        {
            // Arrange & Act & Assert
            Assert.Throws<NullReferenceException>(() => new BaseRepository<TestEntity>(null!));
        }

    }
}