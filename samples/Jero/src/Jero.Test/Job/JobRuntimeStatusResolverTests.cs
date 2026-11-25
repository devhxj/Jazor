using System.Reflection;
using Jero.Core;
using Jero.Core.Service;
using Xunit;

namespace Jero.Test.Job;

public class JobRuntimeStatusResolverTests
{
    [Fact]
    public void Resolve_SchedulerNotLoaded_ReturnsNotLoaded()
    {
        var status = Resolve(false, "Ready");

        Assert.Equal(JobRuntimeStatusResolver.NotLoaded, status.Value);
        Assert.Equal("未加载", status.Text);
    }

    [Fact]
    public void Resolve_RunningTrigger_ReturnsRunning()
    {
        var status = Resolve(true, "Ready", "Running");

        Assert.Equal(JobRuntimeStatusResolver.Running, status.Value);
        Assert.Equal("运行中", status.Text);
    }

    [Fact]
    public void Resolve_AllTriggersPaused_ReturnsPaused()
    {
        var status = Resolve(true, "Pause", "Pause");

        Assert.Equal(JobRuntimeStatusResolver.Paused, status.Value);
        Assert.Equal("已暂停", status.Text);
    }

    [Fact]
    public void Resolve_ReadyTrigger_ReturnsStarted()
    {
        var status = Resolve(true, "Ready");

        Assert.Equal(JobRuntimeStatusResolver.Started, status.Value);
        Assert.Equal("已启动", status.Text);
    }

    [Fact]
    public void Resolve_ErrorToReadyTrigger_ReturnsAbnormal()
    {
        var status = Resolve(true, "ErrorToReady");

        Assert.Equal(JobRuntimeStatusResolver.Abnormal, status.Value);
        Assert.Equal("异常", status.Text);
    }

    [Fact]
    public void Resolve_NoTriggers_ReturnsNoTrigger()
    {
        var status = Resolve(true);

        Assert.Equal(JobRuntimeStatusResolver.NoTrigger, status.Value);
        Assert.Equal("无触发器", status.Text);
    }

    private static JobRuntimeStatus Resolve(bool isLoaded, params string[] statusNames)
    {
        var triggerStatusType = typeof(SysJobTrigger)
            .GetProperty(nameof(SysJobTrigger.Status), BindingFlags.Instance | BindingFlags.Public)!
            .PropertyType;
        var statuses = Array.CreateInstance(triggerStatusType, statusNames.Length);

        for (var i = 0; i < statusNames.Length; i++)
        {
            statuses.SetValue(Enum.Parse(triggerStatusType, statusNames[i]), i);
        }

        return (JobRuntimeStatus)typeof(JobRuntimeStatusResolver)
            .GetMethod(nameof(JobRuntimeStatusResolver.Resolve), BindingFlags.Static | BindingFlags.Public)!
            .Invoke(null, [isLoaded, statuses])!;
    }
}
