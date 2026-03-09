using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VaMME.Views;

namespace VaMME;

public partial class testwindow : Window
{
    public testwindow()
    {
        InitializeComponent();
    }

    private void MenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();

        this.Hide();
    }
}