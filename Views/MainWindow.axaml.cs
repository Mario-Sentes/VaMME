using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;

namespace VaMME.Views
{
    public partial class MainWindow : Window
    {
        public object CurrentView { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            main();
        }

        public void main()
        {
            Debug.WriteLine("Entered main");
        }

        private void testWindow(object? sender, RoutedEventArgs e)
        {
            var testWindow = new testwindow();
            testWindow.Show();

            this.Hide();
        }

        private void runEngine(int taskID)
        {
            var engineWindow = new Engine(taskID);
            engineWindow.Show();

            this.Hide();
        }

        private void run_AmendLinear(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            runEngine(1);
        }

        private void exitProgram(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
            Environment.Exit(0);
        }
    }
}