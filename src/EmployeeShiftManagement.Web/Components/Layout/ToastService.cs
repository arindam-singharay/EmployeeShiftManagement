namespace EmployeeShiftManagement.Web.Components.Layout;

public enum ToastLevel
{
    Success,
    Error,
    Info,
    Warning
}

public sealed record ToastMessage(Guid Id, string Text, ToastLevel Level, int DurationMs);

public sealed class ToastService
{
    public event Action<ToastMessage>? OnShow;

    public void Success(string text, int durationMs = 3500) => Show(text, ToastLevel.Success, durationMs);
    public void Error(string text, int durationMs = 4500) => Show(text, ToastLevel.Error, durationMs);
    public void Info(string text, int durationMs = 3500) => Show(text, ToastLevel.Info, durationMs);
    public void Warning(string text, int durationMs = 4000) => Show(text, ToastLevel.Warning, durationMs);

    private void Show(string text, ToastLevel level, int durationMs)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        OnShow?.Invoke(new ToastMessage(Guid.NewGuid(), text.Trim(), level, durationMs));
    }
}
