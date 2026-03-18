using Company.Function;
using Xunit;

namespace tests;

public class CounterLogicTests
{
    [Fact]
    public void IncrementCounter_UpdatesExistingCounter()
    {
        var counter = new Counter
        {
            Id = "1",
            Count = 2
        };

        var updatedCounter = GetResumeCounter.IncrementCounter(counter);

        Assert.Equal("1", updatedCounter.Id);
        Assert.Equal(3, updatedCounter.Count);
    }

    [Fact]
    public void IncrementCounter_CreatesCounterWhenMissing()
    {
        var updatedCounter = GetResumeCounter.IncrementCounter(null);

        Assert.Equal("1", updatedCounter.Id);
        Assert.Equal(1, updatedCounter.Count);
    }
}
