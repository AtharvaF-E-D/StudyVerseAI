using Microsoft.EntityFrameworkCore;

namespace StudyVerse.Application.Tests.TestSupport;

public static class TestDbContextFactory
{
    /// <summary>Creates a fresh, isolated InMemory database for a single test.</summary>
    public static TestAppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TestAppDbContext(options);
    }
}
