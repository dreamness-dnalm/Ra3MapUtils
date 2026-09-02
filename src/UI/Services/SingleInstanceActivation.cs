namespace UI.Services;

/// <summary>
/// Named wait-handle used so a second process can ask the Mutex holder to show the main window.
/// </summary>
public static class SingleInstanceActivation
{
    public const string EventName = @"Local\Ra3MapUtils.ActivateMainWindow";

    public static EventWaitHandle CreateWaiter() =>
        new(false, EventResetMode.AutoReset, EventName);

    public static bool TrySignal()
    {
        try
        {
            using var handle = EventWaitHandle.OpenExisting(EventName);
            return handle.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
