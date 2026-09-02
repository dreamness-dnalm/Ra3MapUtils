using System.Windows;

namespace UI.Services;

/// <summary>Localized MessageBox helper for application-owned dialogs.</summary>
public static class LocalizedMessageBox
{
    public static MessageBoxResult Show(
        ILocalizationService loc,
        string messageKey,
        string titleKey,
        MessageBoxButton buttons = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        string? messageFallback = null,
        string? titleFallback = null,
        params object[] messageArgs)
    {
        var message = loc.GetString(messageKey, messageFallback ?? messageKey);
        if (messageArgs.Length > 0)
        {
            message = string.Format(message, messageArgs);
        }

        var title = loc.GetString(titleKey, titleFallback ?? titleKey);
        return MessageBox.Show(message, title, buttons, icon);
    }

    public static MessageBoxResult ShowRaw(
        string message,
        string title,
        MessageBoxButton buttons = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None) =>
        MessageBox.Show(message, title, buttons, icon);
}
