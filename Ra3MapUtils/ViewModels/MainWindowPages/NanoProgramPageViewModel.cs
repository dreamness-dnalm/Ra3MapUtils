using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Services.Interface;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public class NanoProgramPageViewModel: ObservableObject
{
    INanoProgramService _nanoProgramService = App.Current.Services.GetRequiredService<INanoProgramService>();
    
    public NanoProgramPageViewModel()
    {
        
    }
}