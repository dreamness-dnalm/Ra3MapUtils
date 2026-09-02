using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UI.Services;

namespace UI.ViewModels.Tools;

public partial class DeveloperHostingWindowViewModel : ObservableObject
{
    private readonly ILocalizationService _localization;

    public DeveloperHostingWindowViewModel(ILocalizationService localization)
    {
        _localization = localization;
        RefreshLocalized();
        _localization.LanguageChanged += (_, _) => RefreshLocalized();
    }

    [ObservableProperty]
    private bool _isTopmost;

    [ObservableProperty]
    private string _windowTitle = "";

    [ObservableProperty]
    private string _singleInstanceNote = "";

    [ObservableProperty]
    private string _mcpEndpointLabel = "";

    [ObservableProperty]
    private string _mcpTransportLabel = "";

    [ObservableProperty]
    private string _swaggerUrlLabel = "";

    public string McpEndpointUrl { get; } = "http://127.0.0.1:30033/mcp";

    public string McpTransport { get; } = "streamable http";

    public string SwaggerUrl { get; } = "http://127.0.0.1:30033/swagger";

    public string DocsUrl { get; } = "https://www.yuque.com/muzeqaq/ra3mapwiki/gvdnzevp57yfqu6k";

    [RelayCommand]
    private void OpenSwagger()
    {
        try
        {
            Process.Start(new ProcessStartInfo(SwaggerUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            LocalizedMessageBox.ShowRaw(
                string.Format(_localization.GetString("DevHost_SwaggerFailed"), ex.Message),
                _localization.GetString("DevHost_Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void OpenDocs()
    {
        try
        {
            Process.Start(new ProcessStartInfo(DocsUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            LocalizedMessageBox.ShowRaw(
                string.Format(_localization.GetString("DevHost_DocsFailed"), ex.Message),
                _localization.GetString("DevHost_Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void CopyMcpEndpoint()
    {
        try
        {
            Clipboard.SetText(McpEndpointUrl);
        }
        catch
        {
            // ignore clipboard failures
        }
    }

    private void RefreshLocalized()
    {
        WindowTitle = _localization.GetString("DevHost_Title");
        SingleInstanceNote = _localization.GetString("DevHost_SingleInstance");
        McpEndpointLabel = string.Format(_localization.GetString("DevHost_McpEndpoint"), McpEndpointUrl);
        McpTransportLabel = string.Format(_localization.GetString("DevHost_McpTransport"), McpTransport);
        SwaggerUrlLabel = string.Format(_localization.GetString("DevHost_SwaggerUrl"), SwaggerUrl);
    }
}
