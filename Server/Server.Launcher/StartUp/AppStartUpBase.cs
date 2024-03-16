namespace Server.Launcher.StartUp;

public abstract class AppStartUpBase : IAppStartUp
{
    public ServerType ServerType { get; private set; }
    public AppSetting Setting { get; protected set; }

    protected readonly TaskCompletionSource<string> AppExitSource = new TaskCompletionSource<string>();
    public Task<string> AppExitToken => AppExitSource.Task;

    protected virtual void Init()
    {
    }

    public bool Init(ServerType serverType, BaseSetting setting, string[] args = null)
    {
        ServerType = serverType;
        Setting = (AppSetting)setting;
        Init();
        return true;
    }

    public abstract Task EnterAsync();

    public virtual void Stop(string message = "")
    {
        AppExitSource.TrySetResult(message);
    }
}