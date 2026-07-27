using CatSit.Core.Detection;

namespace CatSit.Core.Tests;

public class DetectionGateTests
{
    [Theory]
    [InlineData(0L)]
    [InlineData(1000L)]
    [InlineData(86_400_000L)]
    [InlineData(4_000_000_000L)]
    public void FreshGate_IsOpenImmediately(long now)
    {
        Assert.True(new DetectionGate().IsOpen(now));
    }

    [Fact]
    public void StartGrace_ClosesGateForTheGraceWindow()
    {
        var gate = new DetectionGate();
        gate.StartGrace(10_000, 3000);
        Assert.False(gate.IsOpen(10_000));
        Assert.False(gate.IsOpen(12_999));
    }

    [Fact]
    public void GateReopens_AfterGraceElapses()
    {
        var gate = new DetectionGate();
        gate.StartGrace(10_000, 3000);
        Assert.True(gate.IsOpen(13_000));
        Assert.True(gate.IsOpen(50_000));
    }

    [Fact]
    public void StartGrace_AtVeryLargeUptime_DoesNotOverflow()
    {
        var gate = new DetectionGate();
        gate.StartGrace(long.MaxValue - 1000, 3000);
        Assert.False(gate.IsOpen(long.MaxValue - 1000));
    }

    [Fact]
    public void Open_EndsGraceEarly()
    {
        var gate = new DetectionGate();
        gate.StartGrace(10_000, 3000);
        gate.Open();
        Assert.True(gate.IsOpen(10_000));
    }

    [Fact]
    public void RepeatedGrace_ExtendsFromLatestCall()
    {
        var gate = new DetectionGate();
        gate.StartGrace(10_000, 3000);
        gate.StartGrace(12_000, 3000);
        Assert.False(gate.IsOpen(14_000));
        Assert.True(gate.IsOpen(15_000));
    }
}
