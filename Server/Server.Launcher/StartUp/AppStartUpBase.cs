using Server.Utility;

namespace Server.Launcher.StartUp;

public abstract class AppStartUpBase : IAppStartUp
{
    public ServerType ServerType { get; private set; }
    public AppSetting Setting { get; private set; }

    protected readonly TaskCompletionSource<string> AppExitSource = new TaskCompletionSource<string>();
    public Task<string> AppExitToken => AppExitSource.Task;

    public bool Init(ServerType serverType, BaseSetting setting, string[] args = null)
    {
        ServerType = serverType;
        Guard.NotNull(setting, nameof(setting));
        Setting = (AppSetting)setting;
        return true;
    }

    public abstract Task EnterAsync();

    public virtual void Stop(string message = "")
    {
        AppExitSource.TrySetResult(message);
    }
}