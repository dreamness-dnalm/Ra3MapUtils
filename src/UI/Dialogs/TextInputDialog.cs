using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TextBox = System.Windows.Controls.TextBox;
using Button = System.Windows.Controls.Button;
using Orientation = System.Windows.Controls.Orientation;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace UI.Dialogs;

public sealed class TextInputDialog : Window
{
    private readonly TextBox _textBox;

    public TextInputDialog(string title, string prompt, string initialText)
    {
        Title = title;
        Width = 420;
        Height = 180;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;

        var root = new Grid { Margin = new Thickness(16) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var promptBlock = new TextBlock
        {
            Text = prompt,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8),
        };
        Grid.SetRow(promptBlock, 0);
        root.Children.Add(promptBlock);

        _textBox = new TextBox
        {
            Text = initialText,
            Padding = new Thickness(8, 6, 8, 6),
            Margin = new Thickness(0, 0, 0, 16),
        };
        Grid.SetRow(_textBox, 1);
        root.Children.Add(_textBox);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        var ok = new Button { Content = "确定", Width = 88, Margin = new Thickness(0, 0, 8, 0), IsDefault = true };
        var cancel = new Button { Content = "取消", Width = 88, IsCancel = true };
        ok.Click += (_, _) =>
        {
            DialogResult = true;
            Close();
        };
        cancel.Click += (_, _) =>
        {
            DialogResult = false;
            Close();
        };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        Grid.SetRow(buttons, 2);
        root.Children.Add(buttons);

        Content = root;
        Loaded += (_, _) =>
        {
            _textBox.Focus();
            _textBox.SelectAll();
        };
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        };
    }

    public string InputText => _textBox.Text;

    public static bool TryPrompt(Window? owner, string title, string prompt, string initialText, out string value)
    {
        var dialog = new TextInputDialog(title, prompt, initialText);
        if (owner is not null)
        {
            dialog.Owner = owner;
        }

        var result = dialog.ShowDialog() == true;
        value = dialog.InputText;
        return result;
    }
}
