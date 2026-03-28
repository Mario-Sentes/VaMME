using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;
using VaMME.Views;

namespace VaMME.Views
{
    public enum EngineOperations
    {
            AmendLinear = 1,
            AmendRecursive = 2,
            AmendFixer = 3,

            CopyRecursive = 5,
            CopyEMBPC = 6,

            MoveLinear = 7,

            AddParametersSingular = 8,
            AddParametersLinear = 9,
            AddParametersMultiple = 10,

            CreatePairedBase = 11,
            CreatePairedBaseDetail = 12
    }
    public partial class MainWindow : Window
    {
        public object CurrentView { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void helpWindow(object? sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow();

            helpWindow.Closed += (_, _) => this.Show();

            helpWindow.Show();

            this.Hide();
        }

        private void runEngine(EngineOperations taskID)
        {
            var engineWindow = new Engine(taskID);

            engineWindow.Closed += (_, _) => this.Show();

            engineWindow.Show();

            this.Hide();
        }

        private void EngineWindow_Closed(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void exitProgram(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
            Environment.Exit(0);
        }

        private void run_Amend_Linear(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            runEngine(EngineOperations.AmendLinear);
        }

        private void run_Amend_Recursive(object? sender, RoutedEventArgs e)
        {
            runEngine(EngineOperations.AmendRecursive);
        }
        private void run_Amend_Fixer(object? sender, RoutedEventArgs e)
        {
            runEngine(EngineOperations.AmendFixer);
        }

        private void run_Copy_Recursive(object? sender, RoutedEventArgs e)
        {
            runEngine(EngineOperations.CopyRecursive);
        }

        private void run_Copy_EBMPC(object? sender, RoutedEventArgs e)
        {
            runEngine(EngineOperations.CopyEMBPC);
        }

        private void run_Move_Linear(object? sender, RoutedEventArgs e)
        {
            runEngine(EngineOperations.MoveLinear);
        }

        private void run_AddParameters_Singular(object? sender, RoutedEventArgs e)
        {
            runEngine(EngineOperations.AddParametersSingular);
        }

        private void run_AddParameters_Linear(object? sender, RoutedEventArgs e)
        {
            runEngine(EngineOperations.AddParametersLinear);
        }

        private void run_AddParameters_Multiple(object? sender, RoutedEventArgs e)
        {
            runEngine(EngineOperations.AddParametersMultiple);
        }

        private void run_Create_PairedBase(object? sender, RoutedEventArgs e)
        {
            runEngine(EngineOperations.CreatePairedBase);
        }

        private void run_Create_PairedBaseDetail(object? sender, RoutedEventArgs e)
        {
            runEngine(EngineOperations.CreatePairedBaseDetail);
        }
    }
}