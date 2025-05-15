using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.MainWindowPages;

namespace Ra3MapUtils.Views.MainWindowPages;

public partial class KnowledgeBasePage : Page
{
    public KnowledgeBasePageViewModel _knowledgeBasePageViewModel { get => (KnowledgeBasePageViewModel)DataContext; }
    
    public KnowledgeBasePage()
    {
        DataContext = App.Current.Services.GetRequiredService<KnowledgeBasePageViewModel>();
        InitializeComponent();
    }
}