using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using VaMME.Views;

namespace VaMME;
/// <summary>
/// A class to help display to the user more information about how to use VaMME more effectively.
/// </summary>
public static class HelpInfo
{
    public static string toolInfo = "VaMME is a GUI-based tool with the purpose of easing the workload of the average .vmt file modder by making bulk operations easier. " +
        "The tool is split into a number of operational categories: amending files, copying files, moving files, adding parameters to files and creating files. " +
        "You can find more information about each category by selecting its respective button. " +
        "Alternatively, you can find out more about this project by visiting its GitHub page and accessing the most recent branch. " +
        "To do so, access this link here by pasting it into your browser of choice: https://github.com/Mario-Sentes/VaMME";
    
    public static string amendInfo = "File amending operations relate to changing the actual contents of a file. This could mean changing parameters, fixing a file to match some format, etc. " +
        "This program provides 3 amending operations: linear amending, recursive amending and fixer amending. " +
        "Linear amending will amend all files that are provided by the user in one or more directories, as well as any other files that the user provides separately. " +
        "Recursive amending will amend all files that are provided by the user in one or more directories recursively. Providing a single directory will result in all files in this diretory and its subdirectories to be amended. " +
        "These two forms of amending will also require the user to declare a suite of parameters to amend into the files. Declaring a parameter is simple: simply declare a \"$parameterKey\" + \"parameterValue\" combination, and add as many as you require. " +
        "Note: VaMME currently supports one wildcard, namely @filename; putting @filename into the \"parameterValue\" field will replace the \"@filename\" wildcard with the name of the current file. " +
        "For each parameter that the user declares: each file that contains said parameters will be modified such that only the parameters specified by the user are changed. " +
        "Any parameters present in the file that are not in the user defined list are preserved. Any parameters defined by the user that are not in the file are skipped. " +
        "To add parameters to a file, consider using the \"Add parameters option\" in the main menu. " +
        "Fixer amending will attempt to restore all files provided by the user to a certain format; in this case, files will be searched and corrected for wrong slashes, duplicate slashes and missing file extension declarations. ";
    
    public static string copyInfo = "File copying operations relate to the copying of a file from one place (or multiple) into another, or sometimes even multiple places, depending on what information the user provides to the program. " +
        "There are two types of copying: recursive and EBMPC. " +
        "Recursive copying means that, in the \"Enter your files here...\" field, the user can enter folders to look through recursively and also any supplementary files that they also want to copy which may or may not be a part of the looked up folders. " +
        "Note that duplicate files are not permitted. Attempting to enter a file that has already been \"scanned\" will simply be met with a warning message. " +
        "Finally, once the user has specified what files to copy, the user can then populate the \"Enter your folders here...\" field with the folders in which they want to copy the files. These can be multiple folders, but again, duplicates are not considered. " +
        "This operation does not alter files directly, but it may overwrite existing files with identical names. " +
        "Example: if the user wishes to copy C:\\myFolder1\\myFile.vmt to C:\\myFolder2\\myOtherFile.vmt, then a copy of 'myFile.vmt' will be created next to 'myOtherFile.vmt'; if, however, 'myFolder2' also contains a 'myFile.vmt', then that file will be replaced and overwritten by the one in 'myFolder1', so be careful. " +
        "EBMPC, also known as 'Example-based mass parameterised copy', is more complex; it allows the user to define a suite of parameters according to which the copied files will 'inherit' parameters from the first suitable file. " +
        "Example: one can have a file named 'myFile.vmt' containing a plethora of parameters; if one wishes to EBMPC this file into one or multiple directories, then the user must specify what parameters to inherit from the first available file. " +
        "If, for example, the user defines this parameter as \"$basetexture\", then EBMPC will look through all files in the target folder and attempt to find the first file that also contains a \"$basetexture\" parameter. " +
        "If such a file is found, then the original 'myFile.vmt' will be copied into the folder and it will effectively 'inherit' the \"$basetexture\" parameter found in the first matching file. ";
    
    public static string moveInfo = "File moving operations relate to the simple moving of a file from one place (or multiple) into a single other place. " +
        "This can be done by defining what files to move in the \"Enter your files here...\" field and then defining a single destination folder in the \"Enter your folders here...\" field. " +
        "Attempting to add multiple destination folders is not possible. " +
        "Beware that entering a directory into the \"Enter your files here...\" field will result in ALL files in that directory and its subdirectories to be moved around. ";

    public static string addParametersInfo = "File parameter addition operations relate to the addition of parameters to one or more files. " +
        "This is done, depending on the use case, by defining the files to add parameters to (unless in Singular mode, in which case only one file is permitted) and defining the parameters to add to those files. " +
        "This assumes a reasonable file structure. Adding parameters does not overwrite existing parameters; as such, beware that adding a parameter to a file that already has it will result in duplicate parameters. ";
    
    public static string createPairedBaseInfo = "File pair creation operations relate to the creation of new .vmt files from existing .vtf resources. " +
        "This is most useful when the user already has a number of .vtf files ready to use and wants to create their accompanying .vmt files. " +
        "This is done by selecting the desired option (whether the .vmt files will use the .vtf files as a \"$basetexture\" parameter or as a \"$detail\" parameter) and providing the tool with a suitable skeleton file, as well as the path(s) to the folders containing the .vtf files. ";
    
}
/// <summary>
/// A class that manages the displaying of the help window.
/// </summary>
public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();
    }

    private void MenuItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }

    private void ToolInfoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        infoBlock.Text = HelpInfo.toolInfo;
    }

    private void AmendInfoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        infoBlock.Text = HelpInfo.amendInfo;
    }

    private void CopyInfoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        infoBlock.Text = HelpInfo.copyInfo;
    }

    private void MoveInfoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        infoBlock.Text = HelpInfo.moveInfo;
    }

    private void CreatePairedBaseInfoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        infoBlock.Text = HelpInfo.createPairedBaseInfo;
    }

    private void AddParametersInfoButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        infoBlock.Text = HelpInfo.addParametersInfo;
    }
}