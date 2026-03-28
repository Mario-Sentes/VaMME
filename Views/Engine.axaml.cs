using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using HarfBuzzSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VaMME.Views;

namespace VaMME;
/// <summary>
/// A static class containing various shorthanded error messages to display when an error occurs.
/// </summary>
public static class ErrMsgs
{
    public const string ACCESSDENIED = "Access is denied!";

    public const string DIRNOTFOUND = "The specified directory does not exist!";
    public const string DUPLICATEPARAM = "Parameter already exists in the list!";
    public const string DUPLICATEFILE = "File already exists in the list!";
    public const string DUPLICATEFOLDER = "Folder already exists in the list!";

    public const string FILENOTFOUND = "The specified file does not exist!";

    public const string INVALIDINPUT = "The input is invalid!";
    public const string INVALIDPARAM = "Invalid parameter was provided!";

    public const string MALFORMEDFILE = "This file is malformed and cannot be parsed properly!";
    public const string MOVEFAILED = "File moving failed!";

    public const string NOEXAMPLEFILE = "No suitable example file was found!";
    public const string NOPARAM = "No parameter was provided!";
    public const string NOPARAMFOUND = "No eligible parameter was found!";
    public const string NOPARAMTOREMOVE = "No parameters could be removed from the list as the list is empty!";
    public const string NOPARAMVAL = "No parameter value was provided!";

    public const string NULLDIR = "No directory was specified!";
    public const string NULLFILE = "No file was specified!";
}

/// <summary>
/// A static class containing various shorthanded warning messages to display when a warning must be displayed to the user.
/// </summary>
public static class WrnMsgs
{
    public const string MAXFOLDERSREACHED = "This field has been disabled as the maximum number of folders has been reached!";
    public const string MAXFILESREACHED = "This field has been disabled as the maximum number of files has been reached!";

    public const string NOTASKEFFECT = "Task will not have effect as one or more of the relevant fields are empty!";

    public const string REMOVEDPARAM = "Removed parameter pair!";
    public const string REMOVEDFILE = "Removed file!";
    public const string REMOVEDFOLDER = "Removed folder!";

    public const string USINGCRUDELINES = "You are using a crude line editing tool! This is not recommended! Please use another approach if possible!";
}
/// <summary>
/// A class that handles all familiar operations, from executing the desired changes to validating input and emitting changes to the UI.
/// </summary>
public partial class Engine : Window
{
    /// <summary>
    /// A list containing the folders with the files to be edited.
    /// </summary>
    private List<string> folderList;

    /// <summary>
    /// A list containing the files to be edited.
    /// </summary>
    private List<string> fileList;

    /// <summary>
    /// An OrderedDictionary that contains all relevant parameters, usually in format '"$parameterKey"  "parameterValue"'. 
    /// </summary>
    private OrderedDictionary parametersList;

    public IBrush errorColor = Brushes.OrangeRed;
    public IBrush warningColor = Brushes.Gold;
    public IBrush successColor = Brushes.Lime;
    public IBrush textGenericColor = Brushes.White;
    public IBrush[] textBackgroundColor = { Brushes.Black, Brushes.DarkBlue };

    private bool alternateLogLineColor = false;
    private bool alternateGridLineColor = false;
    private bool alternateFileLineColor = false;
    private bool alternateFolderLineColor = false;

    /// <summary>
    /// The file extension to be targeted. Is set to "*.vmt" by default.
    /// </summary>
    readonly string targetFileExtension = "*.vmt";

    //these strings are just for easier documentation
    readonly string typicalFileFormat = @"C:\myFolder\my other_folder\...\myFile.myFileExtension";
    readonly string typicalFolderFormat = @"C:\myFolder\my other_folder\...\myTerminalFolder\";
    readonly string typicalParameterFormat = "\"$parameterKey\"     \"parameterValue\"";

    const string paramRegex = @"^\s*""(\$[\w]+)""\s*""([^""]*)""";
    const string duplicateSlashRegex = @"\/{2,}";

    /// <summary>
    /// An array of valid .vmt parameter keys.
    /// </summary>
    readonly string[] validvmtparameters = new string[]
    {
            "$basetexture"
            ,"$basetexturetransform"
            ,"$frame"
            ,"$basetexture2"
            ,"$basetexturetransform2"
            ,"$frame2"
            ,"$surfaceprop"
            ,"$decal"
            ,"$decalscale"
            ,"$modelmaterial"
            ,"$decalfadeduration"
            ,"$decalfadetime"
            ,"$decalsecondpass"
            ,"$fogscale"
            ,"$splatter"
            ,"$detail"
            ,"$detailtexturetransform"
            ,"$detailscale"
            ,"$detailblendfactor"
            ,"$detailblendmode"
            ,"$detailtint"
            ,"$detailframe"
            ,"$detail_alpha_mask_base_texture"
            ,"$detail2"
            ,"$detailscale2"
            ,"$detailblendfactor2"
            ,"$detailframe2"
            ,"$detailtint2"
            ,"$model"
            ,"$color"
            ,"$color2"
            ,"$seamless scale"
            ,"$seamless_detail"
            ,"$pointsamplemagfilter"
            ,"$alpha"
            ,"$alphatest"
            ,"$additive"
            ,"$blendmodulatetexture"
            ,"$distancealpha"
            ,"$nocull"
            ,"$translucent"
            ,"$no_draw"
            ,"$vertexalpha"
            ,"$bumpmap"
            ,"$ssbump"
            ,"$selfillum"
            ,"$selfillummask"
            ,"$lightwarptexture"
            ,"$halflambert"
            ,"$ambientocclusion"
            ,"$rimlight"
            ,"$rimlightexponent"
            ,"$rimlightboost"
            ,"$rimmask"
            ,"$receiveflashlight"
            ,"$lightmap"
            ,"$reflectivity"
            ,"$phong"
            ,"$phongboost"
            ,"$phongwarptexture"
            ,"$phongexponenttexture"
            ,"$phongexponent"
            ,"$phongfresnelranges"
            ,"$phongalbedotint"
            ,"$envmap"
            ,"$envmaptint"
            ,"$envmapmask"
            ,"$ignorez"
            ,"$alphatest"
            ,"$softwareskin"
            ,"%keywords"
            ,"%notooltexture"
            ,"%tooltexture"
            ,"$treesway"
            ,"$nofog"
            ,"$emissiveblendenabled"
            ,"$emissiveblendstrength"
            ,"$emissiveblendtexture"
            ,"$emissiveblendbasetexture"
            ,"$emissiveblendflowtexture"
            ,"$emissiveblendscrollvector"
            ,"$normalmapalphaenvmapmask"
            ,"%compileblocklos"
            ,"%compileclip"
            ,"%compiledetail"
            ,"%compileladder"
            ,"%compilenodraw"
            ,"%compilenolight"
            ,"%compilenonsolid"
            ,"%compilenpcclip"
            ,"%compilepassbullets"
            ,"%compileskip"
            ,"%compileslime"
            ,"%compileteam"
            ,"%compiletrigger"
            ,"%compilewater"
            ,"%playerclip"
            ,"$nodecal"
    };

    

    /// <summary>
    /// An array of acceptable file extensions for text input.
    /// </summary>
    readonly string[] acceptedfileextensions = new string[]
    {
            "*.vmt",
            "*.txt"
    };

    /// <summary>
    /// A HashSet of unique file extension strings.
    /// See <see cref="acceptedfileextensions"/> for a list of acceptable file extensions.
    /// </summary>
    readonly HashSet<string> validext = new HashSet<string>();

    /// <summary>
    /// A HashSet of unique parameter key/value string pairs.
    /// </summary>
    readonly HashSet<string> validparams = new HashSet<string>();

    /// <summary>
    /// An integer field containing the ID of the task to be executed.
    /// See also the <see cref="EngineOperations"/> enum.
    /// </summary>
    private EngineOperations taskID;

    private bool ctrlHeld = false;

    /// <summary>
    /// A constructor to the Engine class.
    /// Calls InitializeComponent(), 
    /// subscribes to the KeyDown and KeyUp event handlers, 
    /// initialises the relevant <see cref="folderList"/>, <see cref="fileList"/> and <see cref="parametersList"/> lists, 
    /// populates <see cref="validparams"/> and <see cref="validext"/>,
    /// and sets the <see cref="taskID"/> according to the provided parameter.
    /// </summary>
    /// <param name="taskID"> An enum element; should be derived from the <see cref="EngineOperations"/> enum.</param>
    public Engine(EngineOperations taskID)
    {
        InitializeComponent();

        this.KeyDown += Engine_KeyDown;
        this.KeyUp += Engine_KeyUp;

        folderList = new List<string>();
        fileList = new List<string>();
        parametersList = new OrderedDictionary();

        for (int i = 0; i < validvmtparameters.Length; i++)
        {
            validparams.Add(validvmtparameters[i]);
        }

        for (int i = 0; i < acceptedfileextensions.Length; i++)
        {
            validext.Add(acceptedfileextensions[i]);
        }

        SetTaskID(taskID);

        if (taskID == EngineOperations.AmendRecursive)
        {
            files.IsEnabled = false;
            addFileButton.IsEnabled = false;
        }
        else if (taskID == EngineOperations.AmendFixer || taskID == EngineOperations.CopyRecursive || taskID == EngineOperations.MoveLinear)
        {
            parameterKeys.IsEnabled = false;
            parameterValues.IsEnabled = false;

            addParameterPairButton.IsEnabled = false;
        }
        else if (taskID == EngineOperations.AddParametersSingular)
        {
            folders.IsEnabled = false;
            addFolderButton.IsEnabled = false;
        }



    }
    /// <summary>
    /// Handles events relating to the depressing of specific keys.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Engine_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            ctrlHeld = false;
        }
    }
    /// <summary>
    /// Handles events relating to the pressing of specific keys.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Engine_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            ctrlHeld = true;
        }

        if (e.Key == Key.Escape)
        {
            AbortTask(sender, e); //exit the current task
        }
    }
    /// <summary>
    /// Sets the current <see cref="taskID"/> of the engine.
    /// </summary>
    /// <param name="inputTaskID"> The new taskID. </param>
    private void SetTaskID(EngineOperations inputTaskID)
    {
        taskID = inputTaskID;
    }
    /// <summary>
    /// Returns the current <see cref="taskID"/>.
    /// </summary>
    /// <returns></returns>
    private EngineOperations GetTaskID()
    {
        return taskID;
    }
    /// <summary>
    /// Executes the current operation as dictated by <see cref="taskID"/>.
    /// Will issue a message to notify the user of this, then a success message once the task finishes.
    /// Will clear all UI fields once the task is complete, as well as their associated data structures. See <see cref="ResetAllFields"/>.
    /// </summary>
    private void ExecuteTask()
    {
        IssueNormalMessage($"Task with ID {taskID.ToString()} has been started...");

        if (taskID == EngineOperations.AmendLinear)
        {
            if (fileList.Count < 1 || folderList.Count < 1 || parametersList.Count < 1)
            {
                IssueWarningMessage(WrnMsgs.NOTASKEFFECT);
            }
            else
            {
                var files = ConvertFilesFromFolders(folderList, fileList);
                AmendFiles(files, parametersList);
            }
        }
        else if (taskID == EngineOperations.AmendRecursive)
        {
            if (folderList.Count < 1 || parametersList.Count < 1)
            {
                IssueWarningMessage(WrnMsgs.NOTASKEFFECT);
            }
            else
            {
                HashSet<string> uniqueFolders = new HashSet<string>();

                List<string> x = new List<string>();

                foreach (string recursiveFolder in folderList)
                {
                    x = ValidateMultiDirRecursive(recursiveFolder);
                    x.Add(recursiveFolder);
                    foreach (string terminalFolder in x)
                    {
                        uniqueFolders.Add(terminalFolder);
                    }
                }

                AmendFiles(uniqueFolders, parametersList);
            }
        }
        else if (taskID == EngineOperations.AmendFixer)
        {
            if (folderList.Count < 1)
            {
                IssueWarningMessage(WrnMsgs.NOTASKEFFECT);
            }
            else
            {
                foreach (string folder in folderList)
                {
                    FixFiles(folder);
                }
            }
        }
        else if (taskID == EngineOperations.CopyRecursive)
        {
            if (folderList.Count < 1)
            {
                IssueWarningMessage(WrnMsgs.NOTASKEFFECT);
            }
            else
            {
                foreach (var targetFolder in folderList)
                {
                    foreach (var sourceFile in fileList)
                    {
                        CopyFile(sourceFile, targetFolder);
                    }
                }
            }
        }
        else if (taskID == EngineOperations.CopyEMBPC)
        {
            if (fileList.Count < 1 || folderList.Count < 1 || parametersList.Count < 1)
            {
                IssueWarningMessage(WrnMsgs.NOTASKEFFECT);
            }
            else
            {
                foreach (var file in fileList)
                {
                    EBMPC(file, folderList, parametersList);
                }
            }
        }
        else if (taskID == EngineOperations.MoveLinear)
        {
            if (fileList.Count < 1 || folderList.Count < 1)
            {
                IssueWarningMessage(WrnMsgs.NOTASKEFFECT);
            }
            else
            {
                foreach (var dir in folderList)
                {
                    MoveFile(fileList, dir);
                }
            }
        }
        else if (taskID == EngineOperations.AddParametersSingular)
        {
            if (fileList.Count < 1 || parametersList.Count < 1)
            {
                IssueWarningMessage(WrnMsgs.NOTASKEFFECT);
            }
            else 
            { 
                AddParametersToFile(parametersList, fileList[0]);
            }
        }
        else if (taskID == EngineOperations.AddParametersLinear || taskID == EngineOperations.AddParametersMultiple)
        {
            if (parametersList.Count > 0 && (fileList.Count > 0 || folderList.Count > 0))
            {
                var files = ConvertFilesFromFolders(folderList, fileList);

                foreach (var file in files)
                {
                    AddParametersToFile(parametersList, file);
                }
            }
            else
            {
                IssueWarningMessage(WrnMsgs.NOTASKEFFECT);
            }
            
        }
        else if (taskID == EngineOperations.CreatePairedBase)
        {
            if (folderList.Count > 0 && fileList.Count > 0)
            {
                foreach (var folder in folderList)
                {
                    CreateNewVMTsFromVTFdir(folder, fileList[0], false);
                }
            }
            else
            {
                IssueWarningMessage(WrnMsgs.NOTASKEFFECT);
            }
        }
        else if (taskID == EngineOperations.CreatePairedBaseDetail)
        {
            if (folderList.Count > 0 && fileList.Count > 0)
            {
                foreach (var folder in folderList)
                {
                    CreateNewVMTsFromVTFdir(folder, fileList[0], true);
                }
            }
            else
            {
                IssueWarningMessage(WrnMsgs.NOTASKEFFECT);
            }
        }

        IssueSuccessMessage($"Task {(EngineOperations)taskID} has finished!");

        ResetAllFields();
    }
    /// <summary>
    /// Returns to the main menu by calling <see cref="this.Close()"/>.
    /// </summary>
    private void ReturnToMain()
    {
        this.Close();
    }
    /// <summary>
    /// Aborts the current task by calling <see cref="ReturnToMain()"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AbortTask(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ReturnToMain();
    }
    /// <summary>
    /// Starts the current task by calling <see cref="ExecuteTask()"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StartTask(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ExecuteTask();
    }
    /// <summary>
    /// Removes one or all parameters from <see cref="parametersList"/> by calling <see cref="RemoveLastParameterPair()"/>, depending on the value of the <see cref="ctrlHeld"/> <see cref="bool"/>.
    /// If <see cref="true"/>, removes all parameter pairs. If <see cref="false"/>, removes only the last parameter pair.
    /// If no parameter pairs exist, will call <see cref="issueErrorMessage_NoExc()"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RemoveParameter(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (parametersList.Count <= 0)
        {
            IssueErrorMessage_NoExc(ErrMsgs.NOPARAMTOREMOVE);
        }
        else
        {
            if (ctrlHeld)
            {
                while (parametersList.Count>0)
                {
                    RemoveLastParameterPair();
                }
            }
            else
            {
                RemoveLastParameterPair();   
            }
        }

        RedrawParametersToUI();
    }
    /// <summary>
    /// Removes one or all folders from <see cref="folderList"/> by calling <see cref="RemoveLastFolder()"/>, depending on the value of the <see cref="ctrlHeld"/> <see cref="bool"/>.
    /// If <see cref="true"/>, removes all folders. If <see cref="false"/>, removes only the last folder.
    /// If no folder exists, will call <see cref="issueErrorMessage_NoExc()"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RemoveFolder(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (folderList.Count <= 0)
        {
            IssueErrorMessage_NoExc("Attempted to remove folder from empty list! Ignoring...");
        }
        else
        {
            if (ctrlHeld)
            {
                while (folderList.Count>0)
                {
                    RemoveLastFolder();
                }
            }
            else
            {
                RemoveLastFolder();
            }
        }

        RedrawFoldersToUI();
    }
    /// <summary>
    /// Removes the last folder in <see cref="folderList"/> and calls <see cref="IssueWarningMessage(string)"/> to let the user know which folder was last removed. 
    /// CAUTION: Does not affect UI.
    /// </summary>
    private void RemoveLastFolder()
    {
        var x = folderList[folderList.Count - 1];
        IssueWarningMessage(WrnMsgs.REMOVEDFOLDER + $" Folder: {x}");
        folderList.Remove(x);
    }
    /// <summary>
    /// Removes one or all files from <see cref="fileList"/> by calling <see cref="RemoveLastFile()"/>, depending on the value of the <see cref="ctrlHeld"/> <see cref="bool"/>.
    /// If <see cref="true"/>, removes all files. If <see cref="false"/>, removes only the last file.
    /// If no file exists, will call <see cref="issueErrorMessage_NoExc()"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RemoveFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (fileList.Count <= 0)
        {
            IssueErrorMessage_NoExc("Attempted to remove file from empty list! Ignoring...");
        }
        else
        {
            if (ctrlHeld)
            {
                while (fileList.Count > 0)
                {
                    RemoveLastFile();
                }
            }
            else 
            {
                RemoveLastFile();
            }
        }

        RedrawFilesToUI();
    }

    /// <summary>
    /// Removes the last file in <see cref="fileList"/> and calls <see cref="IssueWarningMessage(string)"/> to let the user know which file was last removed. 
    /// CAUTION: Does not affect UI.
    /// </summary>
    private void RemoveLastFile()
    {
        var x = fileList[fileList.Count - 1];
        IssueWarningMessage(WrnMsgs.REMOVEDFILE + $" File: {x}");
        fileList.Remove(x);
    }

    /// <summary>
    /// Changes a <see cref="HashSet{string}"/> of files using a provided <see cref="OrderedDictionary"/>, in format <see cref="typicalParameterFormat"/>, by making calls to <see cref="WriteFile(string, ref List{string})"/>.
    /// See also <see cref="WriteFile(string, ref List{string})"/>.
    /// </summary>
    /// <param name="files">The <see cref="HashSet{T}"/> container with all the unique file paths, in format <see cref="typicalFileFormat"/>.</param>
    /// <param name="par">The <see cref="OrderedDictionary"/> container with all the relevant parameters to edit into the <see cref="files"/>.</param>
    void AmendFiles(HashSet<string> files, OrderedDictionary par)
    {
        List<string> contents = new List<string>();

        if (par.Count <= 0)
        {
            IssueErrorMessage_NoExc(ErrMsgs.NOPARAM);
            return;
        }

        foreach (string file in files)
        {
            contents = GetParameterisedFile(file, par);
            IssueNormalMessage($"Found file {file}...");

            foreach (var item in contents)
            {
                IssueNormalMessage(item);
            }

            WriteFile(file, ref contents);
        }
    }
    /// <summary>
    /// Determines whether a folder exists. Updates the UI accordingly and returns true if the folder exists, false otherwise.
    /// </summary>
    /// <param name="path">The path to the folder, in format <see cref="typicalFolderFormat"/>.</param>
    /// <returns></returns>
    bool FolderExists(string path)
    {
        if (path == null || path == "")
        {
            IssueErrorMessage_NoExc(ErrMsgs.NULLDIR);
            return false;
        }

        if (Directory.Exists(path))
        {
            IssueSuccessMessage($"Directory at path > {path} < has been located successfully!");
            return true;
        }

        IssueErrorMessage_NoExc(ErrMsgs.DIRNOTFOUND + $" Dir: {path}");
        return false;

    }
    /// <summary>
    /// Determines whether a file exists. Updates the UI accordingly and returns true if the file exists, false otherwise.
    /// </summary>
    /// <param name="path">The path to the folder, in format <see cref="typicalFileFormat"/>.</param>
    /// <returns></returns>
    bool FileExists(string? path)
    {
        if (path == null || path == "")
        {
            IssueErrorMessage_NoExc(ErrMsgs.NULLFILE);
            return false;
        }

        if (System.IO.File.Exists(path))
        {
            IssueSuccessMessage($"File at path > {path} < has been located successfully!");
            return true;
        }

        IssueErrorMessage_NoExc(ErrMsgs.FILENOTFOUND + $" File: {path}");
        return false;
    }
    /// <summary>
    /// Returns a list containing all directories nested within the provided folder and issues a success message to the UI. Returns the empty list if the provided folder does not contain any folders.
    /// </summary>
    /// <param name="path">The complete path to the folder to look into, in format <see cref="typicalFolderFormat"/>.</param>
    /// <returns></returns>
    List<string> ValidateMultiDirRecursive(string path)
    {
        List<string> s = new List<string>();

        var dirs = GetAllSubdirs(path);

        if (dirs == null || dirs.Length < 1)
        {
            return s;
        }
        else
        {
            foreach (var dir in dirs)
            {
                s.Add(dir);
                s.AddRange(ValidateMultiDirRecursive(dir));
            }
        }

        IssueSuccessMessage($"Successfully added the subdirectories of '{path}' to the list!");

        return s;
    }
    /// <summary>
    /// Returns a <see cref="string"/> array containing all files nested within the provided folder that match <see cref="targetFileExtension"/>. Returns the empty array if the provided folder does not contain any files, or if an error occurs during the operation.
    /// </summary>
    /// <param name="path">The path of the folder to look for files into, in format <see cref="typicalFolderFormat"/>.</param>
    /// <returns></returns>
    string[] GetAllSubdirFiles(string path)
    {
        try
        {
            return Directory.GetFiles(path, targetFileExtension, SearchOption.AllDirectories);
        }
        catch (Exception e)
        {
            IssueErrorMessage(ErrMsgs.ACCESSDENIED, e);
            return [];
        }
    }
    /// <summary>
    /// Returns a <see cref="string"/> array containing all directories nested within the provided folder. Returns the empty array if the provided folder does not contain any folders, or if an error occurs during the operation.
    /// </summary>
    /// <param name="path">The path of the folder to look for folders into, in format <see cref="typicalFolderFormat"/>.</param>
    /// <returns></returns>
    string[] GetAllSubdirs(string path)
    {
        try
        {
            return Directory.GetDirectories(path);
        }
        catch (Exception e)
        {
            IssueErrorMessage(ErrMsgs.ACCESSDENIED, e);
            return [];
        }
    }
    /// <summary>
    /// Returns the file name and extension of the specified file, as according to the provided filepath. 
    /// If the provided path ends in '/' or its operating system equivalent, returns <see cref="string.Empty"/>.
    /// If the provided path is null, it returns null.
    /// </summary>
    /// <param name="filepath">The path of the file, in format <see cref="typicalFileFormat"/>.</param>
    /// <returns>Just the name of the file + its extension. Example: myFile.vmt</returns>
    static string GetFileName(string filepath)
    {
        return Path.GetFileName(filepath);
    }
    /// <summary>
    /// Returns the file name of the specified file, as according to the provided filepath, excluding its extension. 
    /// If the provided path ends in '/' or its operating system equivalent, returns <see cref="string.Empty"/>.
    /// If the provided path is null, it returns null.
    /// </summary>
    /// <param name="filepath">The path of the file, in format <see cref="typicalFileFormat"/>.</param>
    /// <returns>Just the name of the file. Example: myFile</returns>
    static string GetFileName_NoExtension(string filepath)
    {
        return Path.GetFileNameWithoutExtension(filepath);
    }
    /// <summary>
    /// Returns a <see cref="string"/> <see cref="List{T}"/> containing the new contents of the file after applying parameters, according to the provided <see cref="OrderedDictionary"/>, and updates the UI throughout.
    /// For each match line in the file, if a matching parameter key is found in the <see cref="OrderedDictionary"/>, the element in the list is mutated according to the matching parameter key/value pair in the <see cref="OrderedDictionary"/>.
    /// The base file is not altered.
    /// </summary>
    /// <param name="fileandpath">The complete filepath to the file, in format <see cref="typicalFileFormat"/>.</param>
    /// <param name="par">The container holding the parameter key/value pairs, in format <see cref="typicalParameterFormat"/>.</param>
    /// <returns>A <see cref="string"/> <see cref="List{T}"/> containing the altered file contents, normalized to lowercase. The base file is not altered.</returns>
    List<string> GetParameterisedFile(string fileandpath, OrderedDictionary par)
    {
        List<string> x = new List<string>();

        using (StreamReader sr = System.IO.File.OpenText(fileandpath))
        {
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                s = s.ToLower();

                var match = Regex.Match(s, paramRegex);

                if (match.Success)
                {
                    foreach (DictionaryEntry entry in par)
                    {
                        //if the parameter has been found               
                        if (match.Groups[1].Value.ToLower().Equals(entry.Key.ToString().ToLower()))
                        {
                            s = $"                \"{entry.Key.ToString()}\"     \"{entry.Value.ToString()}\"";
                            IssueNormalMessage($">>> Found eligible parameter {entry.Key.ToString()} in file...");
                        }
                    }
                }

                x.Add(s);
            }
        }

        foreach (var item in x)
        {
            IssueNormalMessage(item);
        }

        return x;
    }

    /// <summary>
    /// Returns a <see cref="string"/> <see cref="List{T}"/> containing the new contents of the file after applying parameters' values only, according to the provided <see cref="OrderedDictionary"/>, and updates the UI throughout.
    /// For each match line in the file, if a matching parameter key is found in the <see cref="OrderedDictionary"/>, the element in the list is mutated according to the matching parameter's value only in the <see cref="OrderedDictionary"/>.
    /// The base file is not altered.
    /// </summary>
    /// <param name="fileandpath">The complete filepath to the file, in format <see cref="typicalFileFormat"/>.</param>
    /// <param name="par">The container holding the parameter key/value pairs, in format <see cref="typicalParameterFormat"/>.</param>
    /// <returns>A <see cref="string"/> <see cref="List{T}"/> containing the altered file contents. The base file is not altered.</returns>
    List<string> GetParameterisedFile_FullLine(string fileandpath, OrderedDictionary par)
    {
        List<string> x = new List<string>();

        using (StreamReader sr = System.IO.File.OpenText(fileandpath))
        {
            string? s;
            string? v;
            string res;
            while ((s = sr.ReadLine()) != null)
            {
                var match = Regex.Match(s, paramRegex);
                res = match.Groups[1].Value.ToLower();

                if (match.Success)
                {
                    foreach (DictionaryEntry entry in par)
                    {
                        v = entry.Key.ToString();
                        //if the parameter has been found               
                        if ((res == v.ToLower()) && (v != null) && (v != "") && (res != null) && (res != ""))
                        {
                            s = entry.Value.ToString();
                            IssueNormalMessage($">>> Found eligible parameter {entry.Key} in file...");
                            IssueNormalMessage(s);
                        }
                    }
                }

                x.Add(s);
            }
        }

        return x;
    }
    /// <summary>
    /// Copies a file from its starting full filepath (in format <see cref="typicalFileFormat"/>) to some destination folder (in format <see cref="typicalFolderFormat"/>).
    /// If the operation fails due to an <see cref="Exception"/>, the file is skipped.
    /// Updates the UI throughout.
    /// </summary>
    /// <param name="fileandpath">The full filepath of the file, in format <see cref="typicalFileFormat"/>.</param>
    /// <param name="destination">The full filepath of the destination folder, in format <see cref="typicalFolderFormat"/>.</param>
    void CopyFile(string fileandpath, string destination)
    {
        string fileName = GetFileName(fileandpath);

        try
        {
            System.IO.File.Copy(fileandpath, destination + @"\" + fileName, true);
        }
        catch (Exception e)
        {
            IssueErrorMessage($"Paste for '{fileandpath}' in '{destination}' failed! Skipping...", e);
        }
        IssueSuccessMessage($"Successfully pasted '{fileandpath}' in '{destination}'!");
    }
    /// <summary>
    /// Example-Based Mass Parameterised Copy - will copy one file from a complete filepath into multiple target directories, and within each directory, get the first matching example and inject it with the specified parameters in the <see cref="OrderedDictionary"/> container, in format <see cref="typicalParameterFormat"/>.
    /// ALL parameters must match between the file to be copied and the file(s) found in the provided folders.
    /// </summary>
    /// <param name="fileToCopy">The complete file path of the file to be copied, in format <see cref="typicalFileFormat"/>.</param>
    /// <param name="targetDirs">The <see cref="string"/> <see cref="List{T}"/> containing all folders to copy into, in format <see cref="typicalFolderFormat"/>.</param>
    /// <param name="parameters">The <see cref="OrderedDictionary"/> container holding all the parameters, in format <see cref="typicalParameterFormat"/>.</param>
    void EBMPC(string fileToCopy, List<string> targetDirs, OrderedDictionary parameters)
    {
        string filename = GetFileName(fileToCopy);  

        //foreach directory in which the file needs to be copied
        foreach (string dir in targetDirs)
        {
            string? exampleFile = null;

            //find first file instance which matches the specified parameters
            //ALL PARAMETERS MUST MATCH!!!
            foreach (var file in Directory.EnumerateFiles(dir, targetFileExtension))
            {
                string tempname = GetFileName(file);
                IssueNormalMessage($"Looking at '{tempname}'...");

                if (MatchParamsToFile(file, parameters))
                {
                    //a valid example file has been found
                    exampleFile = file;
                    IssueNormalMessage($"File '{tempname}' has been found as a good example parameter file!");
                    break;
                }
            }
            
            string newFilePath = Path.Combine(dir, filename);

            if (exampleFile == null)
            {
                IssueErrorMessage_NoExc(ErrMsgs.NOEXAMPLEFILE + $" Folder: {dir}");
                continue;
            }

            //copy the file into the directory
            CopyFile(fileToCopy, dir);
            IssueSuccessMessage($"File '{filename}' has been successfully copied into '{dir}'!");

            //edit the new file with the new parameters
            List<string> newContents = GetParameterisedFile_FullLine(exampleFile, parameters);

            //write the new file
            WriteFile(newFilePath, ref newContents);

            //continue
            IssueSuccessMessage($"Successfully finished work on directory '{dir}'!");
        }
    }

    /// <summary>
    /// Moves a list of files to a single destination directory. Any file that cannot be moved is skipped and an error message is issued.
    /// </summary>
    /// <param name="sourceFiles">The <see cref="string"/> <see cref="List{T}"/> containing all files to move, in format <see cref="typicalFileFormat"/>.</param>
    /// <param name="destinationDir">The destination folder to move files to, in format <see cref="typicalFolderFormat"/>.</param>
    void MoveFile(List<string> sourceFiles, string destinationDir)
    {
        foreach (var file in sourceFiles)
        {
            string fn = GetFileName(file);
            IssueNormalMessage($"Analysing file '{fn}' at path '{file}'...");

            try
            {
                System.IO.File.Move(file, Path.Combine(destinationDir, fn), true);
            }
            catch (Exception e)
            {
                IssueErrorMessage(ErrMsgs.MOVEFAILED + $" For {fn} from {file} in {destinationDir}; Skipping...\n", e);
                continue;
            }
            IssueSuccessMessage($"Successfully moved '{fn}' in '{destinationDir}'!\n");

        }
    }
    /// <summary>
    /// Writes a referenced <see cref="List"/> of type <see cref="string"/> to a specific file + path combination <see cref="string"/>.
    /// First creates a temporary file by appending ".tmp" and writes to it.
    /// CAUTION, automatic mutation: occurrences of "@filename" in the file's text are replaced with the name of the file.
    /// Only once the operation successfully finishes, copies the contents to the desired file.
    /// </summary>
    /// <param name="fileandpath">The complete path of the file, in format <see cref="typicalFileFormat"/>.</param>
    /// <param name="contents">The <see cref="string"/> <see cref="List"/> of new contents to write to the file.</param>
    void WriteFile(string fileandpath, ref List<string> contents)
    {
        string temporaryFile = fileandpath + ".tmp";

        string filename_noext = GetFileName_NoExtension(fileandpath);

        using (StreamWriter writetext = new StreamWriter(temporaryFile))
        {
            IssueNormalMessage($"New file contents on '{fileandpath}':");

            for (int i = 0; i < contents.Count(); i++)
            {
                if (contents[i].Contains("@filename"))
                {
                    contents[i] = contents[i].Replace("@filename", filename_noext);
                }

                IssueNormalMessage(contents[i]);

                writetext.WriteLine(contents[i]);
            }
        }

        System.IO.File.Copy(temporaryFile, fileandpath, true);
        System.IO.File.Delete(temporaryFile);
    }
    /// <summary>
    /// Checks whether the provided file <see cref="string"/> contains the parameters in the provided <see cref="OrderedDictionary"/>. 
    /// Returns true if all parameters provided are present in the file. Validates files that contain duplicated parameters (could be commented out, etc.).
    /// </summary>
    /// <param name="fileandpath">The complete file path, in format <see cref="typicalFileFormat"/>.</param> 
    /// <param name="par">The list of parameters to verify against, in format <see cref="typicalParameterFormat"/>. See <see cref="OrderedDictionary"/>.</param>
    /// <returns></returns>
    bool MatchParamsToFile(string fileandpath, OrderedDictionary par)
    {
        HashSet<string> matchedParameters = new HashSet<string>();

        using (StreamReader sr = System.IO.File.OpenText(fileandpath))
        {
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                var match = Regex.Match(s, paramRegex);

                if (match.Success)
                {
                    foreach (DictionaryEntry entry in par)
                    {
                        //if a valid parameter has been found               
                        if (match.Groups[1].Value.ToLower() == entry.Key.ToString().ToLower())
                        {
                            matchedParameters.Add(entry.Key.ToString().ToLower());
                            IssueNormalMessage($">>> Found eligible parameter {entry.Key} in file...");
                        }
                    }
                }
            }
        }

        return (par.Count == matchedParameters.Count);
    }
    /// <summary>
    /// Adds parameter(s) to the specified file <see cref="string"/> using the provided <see cref="OrderedDictionary"/>, regardless of whether the parameters in the <see cref="OrderedDictionary"/> already exist.
    /// </summary>
    /// <param name="par">The <see cref="OrderedDictionary"/> collection of parameters, in format <see cref="typicalParameterFormat"/>.</param>
    /// <param name="file">The complete file <see cref="string"/> path, in format <see cref="typicalFileFormat"/>.</param>
    void AddParametersToFile(OrderedDictionary par, string file)
    {
        bool doneAdding = false;

        string newLine = "";

        List<string> lineContent = new List<string>();

        using (StreamReader sr = System.IO.File.OpenText(file))
        {
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                lineContent.Add(s);
                if (s.Contains('{') && !s.Contains('}') && !doneAdding)
                {
                    foreach (DictionaryEntry item in par)
                    {
                        newLine = $"                \"{item.Key.ToString()}\"     \"{item.Value.ToString()}\"";
                        lineContent.Add(newLine);
                    }
                    lineContent.Add("   ");

                    doneAdding = true;
                }
            }
        }

        WriteFile(file, ref lineContent);
    }
    /// <summary>
    /// Returns the contents of a file at the specified filepath.
    /// </summary>
    /// <param name="fileandpath">The complete filepath of the file to open, in format <see cref="typicalFileFormat"/>.</param>
    /// <returns>A <see cref="string"/> <see cref="List{T}"/> containing the contents of the file on a line-by-line basis.</returns>
    static List<string> GetFileContents(string fileandpath)
    {
        List<string> x = new List<string>();

        using (StreamReader sr = System.IO.File.OpenText(fileandpath))
        {
            string? s;
            while ((s = sr.ReadLine()) != null)
            {
                x.Add(s);
            }
        }

        return x;
    }
    /// <summary>
    /// Creates a .vmt file for each .vtf file found in a provided folder, where each .vmt file will be based on a provided skeleton file that will contain the path to the .vtf either as a $basetexture or as a $detail parameter.
    /// The base file is not altered.
    /// </summary>
    /// <param name="folderContainingVTFs">The folder containing the desired .vtf files, in format <see cref="typicalParameterFormat"/>.</param>
    /// <param name="skeletonFile">The skeleton file to copy and edit into the directory.</param>
    /// <param name="useAsDetail">Whether to write the path to the .vtf as the $basetexture. If false, it will write it as the $detail instead.</param>
    void CreateNewVMTsFromVTFdir(string folderContainingVTFs, string skeletonFile, bool useAsDetail)
    {
        if (!FolderExists(folderContainingVTFs))
        {
            IssueErrorMessage_NoExc(ErrMsgs.DIRNOTFOUND + $" Path: {folderContainingVTFs}");
            return;
        }

        if (!File.Exists(skeletonFile))
        {
            IssueErrorMessage_NoExc(ErrMsgs.FILENOTFOUND + $" File: {skeletonFile}");
            return;
        }

        string toFind = "$basetexture";

        if (useAsDetail)
        {
            toFind = "$detail";
        }

        string derivedPath = Path.GetFileName(folderContainingVTFs);

        int indexOfDesiredAttributeInFile = -1;

        //find desired attribute
        List<string> base_file_contents = GetFileContents(skeletonFile);
        bool foundDesiredAtt = false;

        for (int i = 0; i < base_file_contents.Count(); i++)
        {
            var match = Regex.Match(base_file_contents[i], paramRegex);
            if (match.Success)
            {
                string paramName = match.Groups[1].Value.ToLower();

                if (paramName == toFind)
                {
                    foundDesiredAtt = true;
                    indexOfDesiredAttributeInFile = i;
                    break;
                }
            }
        }

        if (!foundDesiredAtt)
        {
            IssueErrorMessage_NoExc(ErrMsgs.NOPARAMFOUND);
            IssueNormalMessage($"Attempting to create a copy in memory that contains the desired {toFind} parameter...");

            for (int i = base_file_contents.Count() - 1; i >= 0; i--)
            {
                if (base_file_contents[i].Contains("}"))
                {
                    indexOfDesiredAttributeInFile = i;

                    base_file_contents.Insert(i, $"                \"{toFind}\"     \"PLACEHOLDER\"");

                    break;
                }
            }
        }

        if (indexOfDesiredAttributeInFile == -1)
        {
            IssueErrorMessage_NoExc(ErrMsgs.MALFORMEDFILE + $" File: {skeletonFile}");
            return;
        }

        IEnumerable<string> filesFound = Directory.EnumerateFiles(folderContainingVTFs, "*.vtf");

        if (filesFound.Count() <= 0)
        {
            IssueWarningMessage("Selected directory does not contain any valid .vtf files for use! Aborting...");
            return;
        }

        List<string> originalContents = new List<string>(base_file_contents);
        List<string> tempContents;

        foreach (var item in filesFound)
        {
            tempContents = new List<string>(originalContents);
            tempContents[indexOfDesiredAttributeInFile] = $"                \"{toFind}\"     \"{derivedPath + "\\" + GetFileName_NoExtension(item)}\"";

            string tempFilePath = Path.Combine(folderContainingVTFs, GetFileName_NoExtension(item) + ".vmt");

            WriteFile(tempFilePath, ref tempContents);
        }

        //if a detail attribute is found,
        //make a copy of the base VMT with the name of the vtf for each vtf found in the specified folder,
        //then inject the detail parameter with the file path of the vtf file
    }
    /// <summary>
    /// Iterates through all .vmt files in a directory and performs basic cleanup. 
    /// Attempts to ensure the "$detail" parameter references a ".vtf" file, replaces backslashes '\' with forward slashes '/' and collapses duplicate slashes into a single '/'.
    /// </summary>
    /// <param name="sourceFolder">The folder containing the files to fix, in format <see cref="typicalFolderFormat"/>.</param>
    private void FixFiles(string sourceFolder)
    {
        IEnumerable<string> files = Directory.EnumerateFiles(sourceFolder, "*.vmt");

        int detailIndex = -1;

        List<int> wrongSlashIndices = new List<int>();

        foreach (var file in files)
        {
            List<string> contentsOfFile = GetFileContents(file);

            for (int i = 0; i < contentsOfFile.Count; i++)
            {
                var item = contentsOfFile[i];
                var match = Regex.Match(item, paramRegex);

                if (match.Success && string.Equals(match.Groups[1].Value, "$detail", StringComparison.OrdinalIgnoreCase))
                {
                    detailIndex = i;
                }

                if (item.Contains("\\"))
                {
                    IssueNormalMessage("Found back slash on line: " + item);
                    wrongSlashIndices.Add(i);
                }
            }

            if (detailIndex != -1)
            {
                string stringAtDetail = contentsOfFile[detailIndex];
                if (!stringAtDetail.Contains(".vtf"))
                {
                    IssueNormalMessage("Does not have a valid .vtf signature for detail.");
                    stringAtDetail = stringAtDetail.Remove(stringAtDetail.Length - 1);
                    stringAtDetail = stringAtDetail + ".vtf\"";

                    IssueNormalMessage("Fix: " + stringAtDetail);

                    contentsOfFile[detailIndex] = stringAtDetail;

                    IssueNormalMessage("New line: " + contentsOfFile[detailIndex]);
                }

                detailIndex = -1;
            }

            if (wrongSlashIndices.Count > 0)
            {
                foreach (var index in wrongSlashIndices)
                {
                    string stringContainingWrongSlash = contentsOfFile[index];
                    IssueNormalMessage($"Found back slash line...: {stringContainingWrongSlash}");
                    stringContainingWrongSlash = stringContainingWrongSlash.Replace('\\', '/');
                    contentsOfFile[index] = stringContainingWrongSlash;
                    IssueNormalMessage("New line: " + contentsOfFile[index]);
                }

                wrongSlashIndices.Clear();
            }

            for (int i = 0; i < contentsOfFile.Count; i++)
            {
                if (Regex.IsMatch(contentsOfFile[i], duplicateSlashRegex))
                {
                    IssueNormalMessage("Found duplicate slash on line: " + contentsOfFile[i]);

                    contentsOfFile[i] = Regex.Replace(contentsOfFile[i], duplicateSlashRegex, "/");
                }
            }

            WriteFile(file, ref contentsOfFile);
        }
    }
    /// <summary>
    /// Issues a warning message to the UI, alternating the background colour with each call and scrolling to the end of the <see cref="logScroller"/>.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void IssueWarningMessage(string message)
    {
        AlternateLogLineBackgroundColor();

        var c = textBackgroundColor[Convert.ToInt32(alternateLogLineColor)];

        log.Inlines.Add(new Run
        {
            Text = "WARNING: " + message,
            Foreground = warningColor,
            Background = c
        });

        log.Inlines.Add(new LineBreak());

        logScroller.ScrollToEnd();
    }
    /// <summary>
    /// Issues an error message to the UI and its associated <see cref="Exception"/>, alternating the background colour with each call and scrolling to the end of the <see cref="logScroller"/>.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="e">The <see cref="Exception"/> to display.</param>
    void IssueErrorMessage(string message, Exception e)
    {
        AlternateLogLineBackgroundColor();

        var c = textBackgroundColor[Convert.ToInt32(alternateLogLineColor)];

        log.Inlines.Add(new Run
        {
            Text = "ERROR: " + message,
            Foreground = errorColor,
            Background = c
        });

        log.Inlines.Add(new LineBreak());

        log.Inlines.Add(new Run
        {
            Text = e.ToString(),
            Foreground = errorColor,
            Background = c
        });

        log.Inlines.Add(new LineBreak());

        logScroller.ScrollToEnd();
    }

    /// <summary>
    /// Issues an error message to the UI, alternating the background colour with each call and scrolling to the end of the <see cref="logScroller"/>.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void IssueErrorMessage_NoExc(string message)
    {
        AlternateLogLineBackgroundColor();

        var c = textBackgroundColor[Convert.ToInt32(alternateLogLineColor)];

        log.Inlines.Add(new Run
        {
            Text = "ERROR: " + message,
            Foreground = errorColor,
            Background = c
        });

        log.Inlines.Add(new LineBreak());

        logScroller.ScrollToEnd();
    }
    /// <summary>
    /// Issues a basic message to the UI, alternating the background colour with each call and scrolling to the end of the <see cref="logScroller"/>.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void IssueNormalMessage(string message)
    {
        AlternateLogLineBackgroundColor();

        var c = textBackgroundColor[Convert.ToInt32(alternateLogLineColor)];

        log.Inlines.Add(new Run
        {
            Text = message,
            Foreground = textGenericColor,
            Background = c
        });

        log.Inlines.Add(new LineBreak());

        logScroller.ScrollToEnd();
    }
    /// <summary>
    /// Issues a success message to the UI, alternating the background colour with each call and scrolling to the end of the <see cref="logScroller"/>.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void IssueSuccessMessage(string message)
    {
        AlternateLogLineBackgroundColor();

        var c = textBackgroundColor[Convert.ToInt32(alternateLogLineColor)];

        log.Inlines.Add(new Run
        {
            Text = message,
            Foreground = successColor,
            Background = c
        });

        log.Inlines.Add(new LineBreak());

        logScroller.ScrollToEnd();
    }

    /// <summary>
    /// Alternates which colour to use in the UI.
    /// </summary>
    void AlternateLogLineBackgroundColor() { alternateLogLineColor = !alternateLogLineColor; }

    /// <summary>
    /// Alternates which colour to use in the UI.
    /// </summary>
    void AlternateGridLineBackgroundColor() { alternateGridLineColor = !alternateGridLineColor; }

    /// <summary>
    /// Alternates which colour to use in the UI.
    /// </summary>
    void AlternateFileLineBackgroundColor() { alternateFileLineColor = !alternateFileLineColor; }

    /// <summary>
    /// Alternates which colour to use in the UI.
    /// </summary>
    void AlternateFolderLineBackgroundColor() { alternateFolderLineColor = !alternateFolderLineColor; }

    /// <summary>
    /// Adds a parameter to <see cref="parametersList"/> depending on what is present in the relevant UI fields, according to relevant validation checks, then updates the relevant UI.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddParameterPairButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string? parameterKey = parameterKeys.Text;
        string? parameterValue = parameterValues.Text;

        //check if parameter key is provided
        if (parameterKey == null || parameterKey.Length < 1)
        {
            IssueErrorMessage_NoExc(ErrMsgs.NOPARAM);
            ClearParameterFields();
            return;
        }

        parameterKey = parameterKey.ToLower();

        //check if parameter key is valid
        if (!validparams.Contains(parameterKey))
        {
            IssueErrorMessage_NoExc(ErrMsgs.INVALIDPARAM + $" Parameter: {parameterKey}");
            ClearParameterFields();
            return;
        }

        //check if parameter value is provided
        if (parameterValue == null || parameterValue.Length < 1)
        {
            IssueErrorMessage_NoExc(ErrMsgs.NOPARAMVAL + $" Parameter: {parameterKey}");
            ClearParameterFields();
            return;
        }

        //check if parameter already exists
        try
        {
            parametersList.Add(parameterKey, parameterValue);
        }
        catch (Exception err)
        {
            IssueErrorMessage(ErrMsgs.DUPLICATEPARAM, err);
            ClearParameterFields();
            return;
        }

        IssueSuccessMessage($"Successfully added parameter '{parameterKey} with value' '{parameterValue}' to the parameter list!");

        AddParameterToUIGrid(parameterKey, parameterValue);

        ClearParameterFields();
    }
    /// <summary>
    /// Adds a folder to <see cref="folderList"/> depending on what is present in the relevant UI fields, according to relevant validation checks, then updates the relevant UI.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddFolderButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (addFolderButton.IsEnabled)
        {
            string? s = folders.Text;

            if (FolderExists(s))
            {
                if (folderList.Contains(s))
                {
                    IssueErrorMessage_NoExc(ErrMsgs.DUPLICATEFOLDER + $" Folder: {s}");
                    folders.Text = "";
                    return;
                }

                folderList.Add(s);

                AddFolderToUI(s);

                IssueSuccessMessage($"Successfully added {s} to the folder list!");

                if (taskID == EngineOperations.MoveLinear)
                {
                    addFolderButton.IsEnabled = false;
                    IssueWarningMessage(WrnMsgs.MAXFOLDERSREACHED + " Field: Folders");
                }
            }

            folders.Text = "";
        }
    }
    /// <summary>
    /// Adds a file to <see cref="fileList"/> depending on what is present in the relevant UI fields, according to relevant validation checks, then updates the relevant UI.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddFileButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (addFileButton.IsEnabled)
        {
            string? s = files.Text;

            if (FileExists(s))
            {
                if (fileList.Contains(s))
                {
                    IssueErrorMessage_NoExc(ErrMsgs.DUPLICATEFILE + $" File: {s}");
                    files.Text = "";
                    return;
                }

                fileList.Add(s);

                AddFileToUI(s);

                IssueSuccessMessage($"Successfully added {s} to the file list!");

                if (taskID == EngineOperations.AddParametersSingular || taskID == EngineOperations.CreatePairedBase || taskID == EngineOperations.CreatePairedBaseDetail)
                {
                    IssueWarningMessage(WrnMsgs.MAXFILESREACHED);
                    addFileButton.IsEnabled = false;
                }
            }
            else
            {
                IssueNormalMessage("Looking for directory instead...");
                if (FolderExists(s))
                {
                    foreach (var item in GetAllSubdirFiles(s))
                    {
                        if (fileList.Contains(item))
                        {
                            IssueErrorMessage_NoExc(ErrMsgs.DUPLICATEFILE + $" File: {item}");
                            files.Text = "";
                            return;
                        }

                        fileList.Add(item);

                        AddFileToUI(item);

                        IssueSuccessMessage($"Successfully added {item} to the file list!");

                        if (taskID == EngineOperations.AddParametersSingular || taskID == EngineOperations.CreatePairedBase || taskID == EngineOperations.CreatePairedBaseDetail)
                        {
                            IssueWarningMessage(WrnMsgs.MAXFILESREACHED);
                            addFileButton.IsEnabled = false;
                            break;
                        }
                    }
                }
            }

            files.Text = "";
        }
    }
    /// <summary>
    /// Clears all parameter UI fields.
    /// </summary>
    private void ClearParameterFields()
    {
        parameterKeys.Clear();
        parameterValues.Clear();
    }
    /// <summary>
    ///Adds a parameter to the UI grid, alternating the background colour with each call and scrolling to the end of the <see cref="parameterScroller"/>.
    /// </summary>
    /// <param name="k">The key of the parameter; see format <see cref="typicalParameterFormat"/>.</param>
    /// <param name="v">The value of the parameter; see format <see cref="typicalParameterFormat"/>.</param>
    private void AddParameterToUIGrid(string k, string v)
    {
        AlternateGridLineBackgroundColor();

        var c = textBackgroundColor[Convert.ToInt32(alternateGridLineColor)];

        gridParameterKeys.Inlines.Add(new Run
        {
            Text = k + "    ",
            Foreground = textGenericColor,
            Background = c
        });

        gridParameterValues.Inlines.Add(new Run
        {
            Text = v,
            Foreground = textGenericColor,
            Background = c
        });

        gridParameterKeys.Inlines.Add(new LineBreak());
        gridParameterValues.Inlines.Add(new LineBreak());

        parameterScroller.ScrollToEnd();
    }
    /// <summary>
    ///Adds a file to the UI, alternating the background colour with each call and scrolling to the end of the <see cref="fileScroller"/>.
    /// </summary>
    /// <param name="file">The file to add.</param>
    private void AddFileToUI(string file)
    {
        AlternateFileLineBackgroundColor();

        var c = textBackgroundColor[Convert.ToInt32(alternateFileLineColor)];

        currentFiles.Inlines.Add(new Run
        {
            Text = file,
            Foreground = textGenericColor,
            Background = c
        });

        currentFiles.Inlines.Add(new LineBreak());

        fileScroller.ScrollToEnd();
    }
    /// <summary>
    ///Adds a folder to the UI, alternating the background colour with each call and scrolling to the end of the <see cref="folderScroller"/>.
    /// </summary>
    /// <param name="folder">The folder to add.</param>
    private void AddFolderToUI(string folder)
    {
        AlternateFolderLineBackgroundColor();

        var c = textBackgroundColor[Convert.ToInt32(alternateFolderLineColor)];

        currentFolders.Inlines.Add(new Run
        {
            Text = folder,
            Foreground = textGenericColor,
            Background = c
        });

        currentFolders.Inlines.Add(new LineBreak());

        folderScroller.ScrollToEnd();
    }
    /// <summary>
    /// Redraws the files to the UI.
    /// </summary>
    private void RedrawFilesToUI()
    {
        currentFiles.Text = "";

        currentFiles.Inlines.Clear();

        foreach (var item in fileList)
        {
            AddFileToUI(item);
        }
    }
    /// <summary>
    /// Redraws the folders to the UI.
    /// </summary>
    private void RedrawFoldersToUI()
    {
        currentFolders.Text = "";
        currentFolders.Inlines.Clear();

        foreach (var item in folderList)
        {
            AddFolderToUI(item);
        }
    }
    /// <summary>
    /// Redraws the parameters to the UI.
    /// </summary>
    private void RedrawParametersToUI()
    {
        gridParameterKeys.Text = "";
        gridParameterValues.Text = "";

        gridParameterKeys.Inlines.Clear();
        gridParameterValues.Inlines.Clear();

        foreach (DictionaryEntry item in parametersList)
        {
            AddParameterToUIGrid(item.Key.ToString(), item.Value.ToString());
        }
    }
    /// <summary>
    /// Handles actions relating to pressing a key in the file UI field. Pressing <see cref="Key.Enter"/> will call <see cref="AddFileButton_Click(object?, Avalonia.Interactivity.RoutedEventArgs)"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Files_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            AddFileButton_Click(sender, e);
        }
    }
    /// <summary>
    /// Handles actions relating to pressing a key in the folder UI field. Pressing <see cref="Key.Enter"/> will call <see cref="AddFolderButton_Click(object?, Avalonia.Interactivity.RoutedEventArgs)"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Folders_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            AddFolderButton_Click(sender, e);
        }
    }
    /// <summary>
    /// Handles actions relating to pressing a key in the parameter UI field. Pressing <see cref="Key.Enter"/> will call <see cref="AddParameterPairButton_Click(object?, Avalonia.Interactivity.RoutedEventArgs)"/>.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ParameterPair_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            AddParameterPairButton_Click(sender, e);
        }
    }
    /// <summary>
    /// Converts a <see cref="string"/> <see cref="List{T}"/> to a unique <see cref="string"/> <see cref="HashSet{T}"/>. Duplicate files are not permitted.
    /// </summary>
    /// <param name="folders">The <see cref="string"/> <see cref="List{T}"/> of folders to look into.</param>
    /// <returns>A <see cref="string"/> <see cref="HashSet{T}"/> containing all files found in all folders uniquely. Duplicate files are not permitted.</returns>
    private HashSet<string> ConvertFilesFromFolders(List<string> folders)
    { 
        HashSet<string> result = new HashSet<string>();

        foreach (string folder in folders)
        {
            foreach (string file in Directory.EnumerateFiles(folder, targetFileExtension))
            {
                result.Add(file);
            }
        }

        return result;
    }
    /// <summary>
    /// Converts a <see cref="string"/> <see cref="List{T}"/> to a unique <see cref="string"/> <see cref="HashSet{T}"/> and adds any remaining spare files to the set, if they weren't already present. Duplicate files are not permitted.
    /// </summary>
    /// <param name="folders">The <see cref="string"/> <see cref="List{T}"/> of folders to look into.</param>
    /// <param name="spareFiles">The <see cref="string"/> <see cref="List{T}"/> of spare files to add.</param>
    /// <returns>A <see cref="string"/> <see cref="HashSet{T}"/> containing all files found in all folders uniquely + any unique spare files. Duplicate files are not permitted.</returns>
    private HashSet<string> ConvertFilesFromFolders(List<string> folders, List<string> spareFiles)
    {
        HashSet<string> x = ConvertFilesFromFolders(folders);
        foreach (var file in spareFiles)
        {
            x.Add(file);
        }
        return x;
    }

    /// <summary>
    /// Removes the last parameter pair in <see cref="parametersList"/> and calls <see cref="IssueWarningMessage(string)"/> to let the user know which parameter pair was last removed. 
    /// CAUTION: Does not affect UI.
    /// </summary>
    private void RemoveLastParameterPair()
    {
        if (parametersList.Count > 0)
        {
            string? x = (string?)parametersList[parametersList.Count - 1];
            IssueWarningMessage(WrnMsgs.REMOVEDPARAM + $" Parameter value: {x}");
            parametersList.RemoveAt(parametersList.Count - 1);
        }
        else
        {
            IssueErrorMessage_NoExc(ErrMsgs.NOPARAMTOREMOVE);
        }
    }
    /// <summary>
    /// Resets all relevant UI fields by calling <see cref="EnableAllButtons"/>, <see cref="ClearAllTextBoxes"/>, <see cref="ClearAllStoredValues"/> and <see cref="RedrawAllUI"/>.
    /// </summary>
    private void ResetAllFields()
    {
        EnableAllButtons();
        ClearAllTextBoxes();
        ClearAllStoredValues();
        RedrawAllUI();
    }
    /// <summary>
    /// Enables all relevant UI buttons.
    /// </summary>
    private void EnableAllButtons()
    {
        addFileButton.IsEnabled = true;
        addFolderButton.IsEnabled = true;
        addParameterPairButton.IsEnabled = true;
    }
    /// <summary>
    /// Clears all relevant TextBoxes.
    /// </summary>
    private void ClearAllTextBoxes()
    {
        files.Text = "";
        folders.Text = "";
        parameterKeys.Text = "";
        parameterValues.Text = "";
    }
    /// <summary>
    /// Clears all values stored in <see cref="fileList"/>, <see cref="folderList"/> and <see cref="parametersList"/>.
    /// </summary>
    private void ClearAllStoredValues()
    { 
        fileList = new List<string>();
        folderList = new List<string>();
        parametersList = new OrderedDictionary();
    }
    /// <summary>
    /// Redraws all relevant UI elements by calling <see cref="RedrawFilesToUI"/>, <see cref="RedrawFoldersToUI"/> and <see cref="RedrawParametersToUI"/>.
    /// </summary>
    private void RedrawAllUI()
    {
        RedrawFilesToUI();
        RedrawFoldersToUI();
        RedrawParametersToUI();
    }
}