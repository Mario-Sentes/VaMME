using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Tmds.DBus.Protocol;
using VaMME;
using VaMME.ViewModels;
using VaMME.Views;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace VaMME;

public static class ErrMsgs
{
    public const string DIRNOTFOUND = "The specified directory does not exist!";
    public const string NULLDIR = "No directory was specified!";
    public const string NULLFILE = "No file was specified!";
    public const string FILENOTFOUND = "The specified file does not exist!";
    public const string ACCESSDENIED = "Access is denied!";
    public const string INVALIDINPUT = "The input is invalid!";
    public const string NOPARAM = "No parameter was provided!";
    public const string NOPARAMVAL = "No parameter value was provided!";
    public const string INVALIDPARAM = "Invalid parameter was provided!";
    public const string DUPLICATEPARAM = "Parameter already exists in the list!";
    public const string DUPLICATEFILE = "File already exists in the list!";
    public const string DUPLICATEFOLDER = "Folder already exists in the list!";
    public const string MOVEFAILED = "File moving failed!";
}

public static class WrnMsgs
{
    public const string USINGCRUDELINES = "You are using a crude line editing tool! This is not recommended! Please use another approach if possible!";
    public const string REMOVEDPARAM = "Removed parameter pair!";
    public const string REMOVEDFILE = "Removed file!";
    public const string REMOVEDFOLDER = "Removed folder!";
    public const string MAXFOLDERSREACHED = "This field has been disabled as the maximum number of folders has been reached!";
    public const string MAXFILESREACHED = "This field has been disabled as the maximum number of files has been reached!";
}

public partial class Engine : Window
{
    private List<string> folderList;
    private List<string> fileList;
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

    private HashSet<string> batchsource = new HashSet<string>();

    string targetFileExtension = "*.vmt";

    const string paramRegex = @"^\s*""(\$[\w]+)""\s*""([^""]*)""";
    const string duplicateSlashRegex = @"\/{2,}";

    //TODO BUG;
    //THE REGEX DOES NOT RECOGNIZE VALID PARAMETERS THAT ARE NOT ENCAPSULATED IN QUOTATION MARKS

    string[] validvmtparameters = new string[]
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

    HashSet<string> validparams = new HashSet<string>();

    string[] acceptedfileextensions = new string[]
    {
            "*.vmt",
            "*.txt",
            "*.psd",
            "*.png"
    };

    HashSet<string> validext = new HashSet<string>();

    private int taskID = -1;
    private string UID = "";

    private bool ctrlHeld = false;

    public Engine(int taskID)
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

        setTaskID(taskID);

    }

    private void Engine_KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            ctrlHeld = false;
        }
    }

    private void Engine_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
        {
            ctrlHeld = true;
        }

        if (e.Key == Key.Escape)
        {
            AbortTask(sender, e);
        }
    }

    private void setTaskID(int inputTaskID)
    {
        taskID = inputTaskID;
    }

    private int getTaskID()
    {
        return taskID;
    }

    private void ExecuteTask()
    {
        issueNormalMessage($"Task with ID {taskID.ToString()} has been started...");

        if (taskID == (int)EngineOperations.AmendLinear)
        {
            var files = convertFilesFromFolders(folderList, fileList);
            amendfilesindirectory(files, parametersList);
        }
        else if (taskID == (int)EngineOperations.AmendRecursive)
        {
            HashSet<string> uniqueFolders = new HashSet<string>();

            List<string> x = new List<string>();

            foreach (string recursiveFolder in folderList)
            {
                x = validateMultiDirRecursive(recursiveFolder);
                x.Add(recursiveFolder);
                foreach (string terminalFolder in x)
                {
                    uniqueFolders.Add(terminalFolder);
                }
            }

            amendfilesindirectory(uniqueFolders, parametersList);
        }
        else if (taskID == (int)EngineOperations.AmendFixer)
        {
            foreach (string folder in folderList)
            {
                fixFiles(folder);
            }
        }
        else if (taskID == (int)EngineOperations.CopyLinear)
        {
            foreach (var targetFolder in folderList)
            {
                foreach (var sourceFile in fileList)
                {
                    copyFile(sourceFile, targetFolder);
                }
            }
        }
        else if (taskID == (int)EngineOperations.CopyEMBPC)
        {
            foreach (var file in fileList)
            {
                EBMPC(file, folderList, parametersList);
            }
        }
        else if (taskID == (int)EngineOperations.MoveLinear)
        {
            foreach (var dir in folderList)
            {
                moveFile(fileList, dir);
            }
        }
        else if (taskID == (int)EngineOperations.AddParametersSingular)
        {
            if (fileList.Count > 0)
            {
                addParameterToFile(parametersList, fileList[0]);
            }
        }
        else if (taskID == (int)EngineOperations.AddParametersLinear)
        {
            foreach (var file in fileList)
            {
                addParameterToFile(parametersList, file);
            }
        }
        else if (taskID == (int)EngineOperations.AddParametersMultiple)
        {
            fileList.AddRange(convertFilesFromFolders(folderList).ToList());

            foreach (var file in fileList)
            {
                addParameterToFile(parametersList, file);
            }
        }
        else if (taskID == (int)EngineOperations.CreatePairedBase)
        {
            createNewVMTsFromVTFdir();
        }

            issueSuccessMessage($"Task {(EngineOperations)taskID} with ID {taskID.ToString()} has finished!");

        resetAllFields();
    }

    private void ReturnToMain()
    {
        this.Close();
    }

    private void AbortTask(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ReturnToMain();
    }

    private void StartTask(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ExecuteTask();
    }

    private void removeParameter(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (parametersList.Count <= 0)
        {
            issueErrorMessage_NoExc("Attempted to remove parameter pair from empty list! Ignoring...");
        }
        else
        {
            if (ctrlHeld)
            {
                while (parametersList.Count>0)
                {
                    removeLastParameterPair();
                }
            }
            else
            {
                removeLastParameterPair();   
            }
        }

        redrawParametersToUI();
    }
    private void removeFolder(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (folderList.Count <= 0)
        {
            issueErrorMessage_NoExc("Attempted to remove folder from empty list! Ignoring...");
        }
        else
        {
            if (ctrlHeld)
            {
                while (folderList.Count>0)
                {
                    removeLastFolder();
                }
            }
            else
            {
                removeLastFolder();
            }
        }

        redrawFoldersToUI();
    }
    private void removeLastFolder()
    {
        var x = folderList[folderList.Count - 1];
        issueWarningMessage(WrnMsgs.REMOVEDFOLDER + $" Folder: {x}");
        folderList.Remove(x);
    }
    private void removeFile(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (fileList.Count <= 0)
        {
            issueErrorMessage_NoExc("Attempted to remove file from empty list! Ignoring...");
        }
        else
        {
            if (ctrlHeld)
            {
                while (fileList.Count > 0)
                {
                    removeLastFile();
                }
            }
            else 
            {
                removeLastFile();
            }
        }

        redrawFilesToUI();
    }
    private void removeLastFile()
    {
        var x = fileList[fileList.Count - 1];
        issueWarningMessage(WrnMsgs.REMOVEDFILE + $" File: {x}");
        fileList.Remove(x);
    }

    void amendfilesindirectory(HashSet<string> files, OrderedDictionary par)
    {
        List<string> contents = new List<string>();

        if (par.Count <= 0)
        {
            issueErrorMessage_NoExc(ErrMsgs.NOPARAM);
            return;
        }

        foreach (string file in files)
        {
            contents = getParameterisedFile(file, par);
            issueNormalMessage($"Found file {file}...");

            foreach (var item in contents)
            {
                issueNormalMessage(item);
            }

            writeFile(file, ref contents);
        }
    }

    bool dirExists(string path)
    {
        if (path == null || path == "")
        {
            issueErrorMessage_NoExc(ErrMsgs.NULLDIR);
            return false;
        }

        if (Directory.Exists(path))
        {
            issueSuccessMessage($"Directory at path > {path} < has been located successfully!");
            return true;
        }

        issueErrorMessage_NoExc(ErrMsgs.DIRNOTFOUND + $" Dir: {path}");
        return false;

    }

    bool fileExists(string path)
    {
        if (path == null || path == "")
        {
            issueErrorMessage_NoExc(ErrMsgs.NULLFILE);
            return false;
        }

        if (System.IO.File.Exists(path))
        {
            issueSuccessMessage($"File at path > {path} < has been located successfully!");
            return true;
        }

        issueErrorMessage_NoExc(ErrMsgs.FILENOTFOUND + $" File: {path}");
        return false;
    }

    List<string> validateMultiDirRecursive(string path)
    {
        List<string> s = new List<string>();

        var dirs = getAllSubdirs(path);

        if (dirs == null || dirs.Length < 1)
        {
            return s;
        }
        else
        {
            foreach (var dir in dirs)
            {
                s.Add(dir);
                s.AddRange(validateMultiDirRecursive(dir));
            }
        }

        issueSuccessMessage($"Successfully added the subdirectories of '{path}' to the list!");

        return s;
    }

    string[] getAllSubdirFiles(string path)
    {
        try
        {
            return Directory.GetFiles(path, targetFileExtension, SearchOption.AllDirectories);
        }
        catch (Exception e)
        {
            issueErrorMessage(ErrMsgs.ACCESSDENIED, e);
            return [];
        }
    }

    string[] getAllSubdirs(string path)
    {
        return Directory.GetDirectories(path);
    }

    string getFileNameFromPath(string filepath, string filedir)
    {
        string s;

        s = filepath.Remove(0, filedir.Length + 1);

        return s;
    }

    string getFileName(string filepath)
    {
        return Path.GetFileName(filepath);
    }

    string getFileName_NoExtension(string filepath)
    {
        return Path.GetFileNameWithoutExtension(filepath);
    }

    Dictionary<string, string> getParams()
    {
        Dictionary<string, string> parameterPairs = new Dictionary<string, string>();

        bool doneparam = false;

        while (!doneparam)
        {
            Console.Clear();
            Console.WriteLine("When a valid parameter is entered, pressing enter will continue to the parameter's value.");
            Console.WriteLine("If no parameter is provided and enter is pressed, the program will return to the menu.");
            Console.WriteLine("If a parameter was already provided, pressing enter will continue to the files being amended as requested.");

            displayParametersInDictionary(parameterPairs);

            Console.WriteLine("\nPlease state your parameter, in format '$parametername': ");

            string proc = Console.ReadLine();

            if (proc == "")
            {
                doneparam = true;
                break;
            }

            if (validparams.Contains(proc))
            {
                //valid parameter has been found
                Console.Clear();
                Console.WriteLine("{0} is a valid parameter! Now enter its desired value: ", proc);
                parameterPairs.Add(proc, Console.ReadLine());
                Console.WriteLine("Parameter > \"{0}\" < has been successfully added!", proc);
            }
        }

        return parameterPairs;
    }

    Dictionary<string, string> getParamsNoVal()
    {
        Dictionary<string, string> x = new Dictionary<string, string>();

        bool doneparam = false;

        while (!doneparam)
        {

            Console.WriteLine("When a valid parameter is entered, pressing enter will continue to the parameter's value.");
            Console.WriteLine("If no parameter is provided and enter is pressed, the program will return to the menu.");
            Console.WriteLine("If a parameter was already provided, pressing enter will continue to the files being amended as requested.");

            displayParametersInDictionaryNoVal(x);

            Console.WriteLine("Please state your parameter, in format '$parametername': ");

            string proc = Console.ReadLine();

            if (proc == "")
            {
                doneparam = true;
                break;
            }

            if (validparams.Contains(proc))
            {
                //valid parameter has been found
                Console.Clear();
                Console.WriteLine("{0} is a valid parameter!", proc);
                x.Add(proc, "");
                Console.WriteLine("Parameter > \"{0}\" < has been successfully added!", proc);
            }
        }

        return x;
    }

    List<string> getParameterisedFile(string file, OrderedDictionary par)
    {
        List<string> x = new List<string>();

        using (StreamReader sr = System.IO.File.OpenText(file))
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
                        if (match.Groups[1].Value.ToLower() == entry.Key.ToString().ToLower())
                        {
                            s = $"                \"{entry.Key.ToString()}\"     \"{entry.Value.ToString()}\"";
                            issueNormalMessage($">>> Found eligible parameter {entry.Key.ToString()} in file...");
                        }
                    }
                }

                x.Add(s);
            }
        }

        foreach (var item in x)
        {
            issueNormalMessage(item);
        }

        return x;
    }

    List<string> getParameterisedFile_FullLine(string file, OrderedDictionary par)
    {
        List<string> x = new List<string>();

        using (StreamReader sr = System.IO.File.OpenText(file))
        {
            string s;
            string v;
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
                            issueNormalMessage($">>> Found eligible parameter {entry.Key} in file...");
                            issueNormalMessage(s);
                        }
                    }
                }

                x.Add(s);
            }
        }

        return x;
    }
    void copyFile(string file, string destination)
    {
        string fileName = getFileName(file);

        try
        {
            System.IO.File.Copy(file, destination + @"\" + fileName, true);
        }
        catch (Exception e)
        {
            issueErrorMessage($"Paste for '{file}' in '{destination}' failed! Skipping...", e);
        }
        issueSuccessMessage($"Successfully pasted '{file}' in '{destination}'!");
    }
    void copyFile_NewName(string file, string destination, string newName)
    {
        try
        {
            System.IO.File.Copy(file, destination + @"\" + newName, true);
        }
        catch (Exception)
        {
            issueWarningMessage($"Paste for '{newName}' from '{file}' in '{destination}' failed! Skipping...");
        }
        issueSuccessMessage($"Successfully pasted '{newName}' in '{destination}'!");
    }

    void EBMPC(string fileToCopy, List<string> targetDirs, OrderedDictionary parameters)
    {
        string filename = getFileName(fileToCopy);
        string tempname;

        //foreach directory in which the file needs to be copied
        foreach (string dir in targetDirs)
        {
            //  copy the file into the directory
            copyFile(fileToCopy, dir);
            issueSuccessMessage($"File '{filename}' has been successfully copied into '{dir}'!");

            //  find first file instance which matches the specified parameters
            //  ALL PARAMETERS MUST MATCH!!!
            foreach (var file in Directory.EnumerateFiles(dir, targetFileExtension))
            {
                tempname = getFileName(file);
                issueNormalMessage($"Looking at '{tempname}'...");
                if (matchParamsToFile(file, parameters))
                {
                    //a valid example file has been found
                    issueNormalMessage($"File '{tempname}' has been found as a good example parameter file!");
                    break;
                }
            }

            string newFilePath = Path.Combine(dir, filename);

            //  edit the new file with the new parameters
            List<string> newContents = getParameterisedFile_FullLine(newFilePath, parameters);

            //  write the new file
            writeFile(newFilePath, ref newContents);

            //  continue
            issueSuccessMessage($"Successfully finished work on directory '{dir}'!");
        }
    }

    void moveFile(List<string> sourceFiles, string destinationDir)
    {
        foreach (var file in sourceFiles)
        {
            string fn = getFileName(file);
            issueNormalMessage($"Analysing file '{fn}' at path '{file}'...");

            try
            {
                System.IO.File.Move(file, Path.Combine(destinationDir, fn), true);
            }
            catch (Exception e)
            {
                issueErrorMessage(ErrMsgs.MOVEFAILED + $" For {fn} from {file} in {destinationDir}; Skipping...\n", e);
                continue;
            }
            issueSuccessMessage($"Successfully moved '{fn}' in '{destinationDir}'!\n");

        }
    }

    void writeFile(string fileandpath, ref List<string> contents)
    {
        string temporaryFile = fileandpath + ".tmp";

        string filename = getFileName(fileandpath);
        string filename_noext = getFileName_NoExtension(fileandpath);

        using (StreamWriter writetext = new StreamWriter(temporaryFile))
        {
            issueNormalMessage($"New file contents on '{fileandpath}':");

            for (int i = 0; i < contents.Count(); i++)
            {
                if (contents[i].Contains("@filename"))
                {
                    contents[i] = contents[i].Replace("@filename", filename_noext);
                }

                issueNormalMessage(contents[i]);

                writetext.WriteLine(contents[i]);
            }
        }

        System.IO.File.Copy(temporaryFile, fileandpath, true);
        System.IO.File.Delete(temporaryFile);
    }

    bool matchParamsToFile(string file, OrderedDictionary par)
    {
        HashSet<string> matchedParameters = new HashSet<string>();

        using (StreamReader sr = System.IO.File.OpenText(file))
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
                            issueNormalMessage($">>> Found eligible parameter {entry.Key} in file...");
                            issueNormalMessage($">>> Set value of '{entry.Key}' to '{s}'!");
                        }
                    }
                }
            }
        }

        return (par.Count == matchedParameters.Count);
    }

    void addParameterToFile(OrderedDictionary par, string file)
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

        writeFile(file, ref lineContent);
    }

    void addParameterToFiles(string dirPath, OrderedDictionary parameters)
    {
        foreach (string file in Directory.EnumerateFiles(dirPath, targetFileExtension))
        {
            addParameterToFile(parameters, file);
        }
    }

    void addParameterToFilesInDirs(List<string> dirs, OrderedDictionary parameters)
    {
        foreach (string dir in dirs)
        {
            foreach (string file in Directory.EnumerateFiles(dir, targetFileExtension))
            {
                addParameterToFile(parameters, file);
            }
        }
    }

    List<string> getFileContents(string filename)
    {
        List<string> x = new List<string>();

        using (StreamReader sr = System.IO.File.OpenText(filename))
        {
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                x.Add(s);
            }
        }

        return x;
    }

    List<string>? getFileContents_SpecificQuery(string filename, string searchedString) //if the file contains the specified string in any of the given lines, then the function returns the contents of the file, otherwise null
    {
        bool found = false;

        List<string> x = new List<string>();

        using (StreamReader sr = System.IO.File.OpenText(filename))
        {
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                if (s.ToLower().Contains(searchedString.ToLower()))
                {
                    found = true;
                }
                x.Add(s);
            }
        }

        if (!found)
        {
            return null;
        }

        return x;
    }

    private OrderedDictionary getFileParameters()
    {
        OrderedDictionary result = new OrderedDictionary();

        return result;
    }

    void createNewVMTsFromVTFdir(string source_dir, string base_file)
    {
        int indexOfDetailInBaseFile = -1;

        //find detail attribute
        List<string>? base_file_contents = getFileContents_SpecificQuery(base_file, "$detail");

        if (base_file_contents == null) //if a detail attribute is not found, return
        {
            issueWarningMessage("Selected file does not contain the specified parameter! Aborting...");
            return;
        }

        for (int i = 0; i < base_file_contents.Count; i++)
        {
            if (base_file_contents[i].ToLower().Contains("$detail"))
            {
                indexOfDetailInBaseFile = i;
                break;
            }
        }

        IEnumerable<string> filesFound = Directory.EnumerateFiles(source_dir, "*.vtf");

        if (filesFound.Count() <= 0)
        {
            issueWarningMessage("Selected directory does not contain any valid .vtf files for use! Aborting...");
            return;
        }

        issueNormalMessage("Enter the relative workpath of the source of the materials: ");
        string specifiedPath = Console.ReadLine();

        foreach (var item in filesFound)
        {
            base_file_contents[indexOfDetailInBaseFile] = $"                \"$detail\"     \"{specifiedPath + "\\" + getFileName_NoExtension(item)}\"";
            writeFile(base_file, ref base_file_contents);
            copyFile_NewName(base_file, source_dir, getFileName_NoExtension(item) + ".vmt");

        }


        //if a detail attribute is found,
        //make a copy of the base VMT with the name of the vtf for each vtf found in the specified folder,
        //then inject the detail parameter with the file path of the vtf file
    }

    private void fixFiles(string sourceDir)
    {
        IEnumerable<string> files = Directory.EnumerateFiles(sourceDir, "*.vmt");

        int detailIndex = -1;

        List<int> wrongSlashIndices = new List<int>();

        foreach (var file in files)
        {
            List<string> contentsoffile = getFileContents(file);

            foreach (var item in contentsoffile)
            {
                if (item.ToLower().Contains("\"$detail\""))
                {
                    detailIndex = contentsoffile.IndexOf(item);
                }

                if (item.Contains("\\"))
                {
                    issueNormalMessage("Found back slash on line: " + item);
                    wrongSlashIndices.Add(contentsoffile.IndexOf(item));
                }
            }

            if (detailIndex != -1)
            {
                string stringAtDetail = contentsoffile[detailIndex];
                if (!stringAtDetail.Contains(".vtf"))
                {
                    issueNormalMessage("Does not have a valid .vtf signature for detail.");
                    stringAtDetail = stringAtDetail.Remove(stringAtDetail.Length - 1);
                    stringAtDetail = stringAtDetail + ".vtf\"";

                    issueNormalMessage("Fix: " + stringAtDetail);

                    contentsoffile[detailIndex] = stringAtDetail;

                    issueNormalMessage("New line: " + contentsoffile[detailIndex]);
                }

                detailIndex = -1;
            }

            if (wrongSlashIndices.Count > 0)
            {
                foreach (var index in wrongSlashIndices)
                {
                    string stringContainingWrongSlash = contentsoffile[index];
                    issueNormalMessage($"Found back slash line...: {stringContainingWrongSlash}");
                    stringContainingWrongSlash = stringContainingWrongSlash.Replace('\\', '/');
                    contentsoffile[index] = stringContainingWrongSlash;
                    issueNormalMessage("New line: " + contentsoffile[index]);
                }

                wrongSlashIndices.Clear();
            }

            for (int i = 0; i < contentsoffile.Count; i++)
            {
                if (Regex.IsMatch(contentsoffile[i], duplicateSlashRegex))
                {
                    issueNormalMessage("Found duplicate slash on line: " + contentsoffile[i]);

                    contentsoffile[i] = Regex.Replace(contentsoffile[i], duplicateSlashRegex, "/");
                }
            }

            writeFile(file, ref contentsoffile);
        }
    }

    void displayParametersInDictionary(Dictionary<string, string> x)
    {
        issueNormalMessage("Current parameters: ");
        if (x.Count < 1)
        {
            issueWarningMessage("No parameters added yet.");
        }
        foreach (var item in x)
        {
            issueNormalMessage($"\"{item.Key}\"  \"{item.Value}\"");
        }
    }

    void displayParametersInDictionaryNoVal(Dictionary<string, string> x)
    {
        issueNormalMessage("Current parameters: ");
        if (x.Count < 1)
        {
            issueWarningMessage("No parameters added yet.");
        }
        else
        {
            foreach (var item in x)
            {
                issueNormalMessage($"\"{item.Key}\"");
            }
        }
    }

    //is atomic? Y
    void issueWarningMessage(string message)
    {
        alternateLogLineBackgroundColor();

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

    //is atomic? Y
    void issueErrorMessage(string message, Exception e)
    {
        alternateLogLineBackgroundColor();

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

    //is atomic? Y
    void issueErrorMessage_NoExc(string message)
    {
        alternateLogLineBackgroundColor();

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

    void clearLog()
    {
        alternateLogLineColor = false;

        log.Inlines.Clear();

        logScroller.ScrollToEnd();
    }

    void issueNormalMessage(string message)
    {
        alternateLogLineBackgroundColor();

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

    void issueSuccessMessage(string message)
    {
        alternateLogLineBackgroundColor();

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

    void alternateLogLineBackgroundColor() { alternateLogLineColor = !alternateLogLineColor; }
    void alternateGridLineBackgroundColor() { alternateGridLineColor = !alternateGridLineColor; }
    void alternateFileLineBackgroundColor() { alternateFileLineColor = !alternateFileLineColor; }
    void alternateFolderLineBackgroundColor() { alternateFolderLineColor = !alternateFolderLineColor; }

    private void addParameterPairButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string? parameterKey = parameterKeys.Text;
        string? parameterValue = parameterValues.Text;

        //check if parameter key is provided
        if (parameterKey == null || parameterKey.Length < 1)
        {
            issueErrorMessage_NoExc(ErrMsgs.NOPARAM);
            clearParameterFields();
            return;
        }

        parameterKey = parameterKey.ToLower();

        //check if parameter key is valid
        if (!validparams.Contains(parameterKey))
        {
            issueErrorMessage_NoExc(ErrMsgs.INVALIDPARAM + $" Parameter: {parameterKey}");
            clearParameterFields();
            return;
        }

        //check if parameter value is provided
        if (parameterValue == null || parameterValue.Length < 1)
        {
            issueErrorMessage_NoExc(ErrMsgs.NOPARAMVAL + $" Parameter: {parameterKey}");
            clearParameterFields();
            return;
        }

        //check if parameter already exists
        try
        {
            parametersList.Add(parameterKey, parameterValue);
        }
        catch (Exception err)
        {
            issueErrorMessage(ErrMsgs.DUPLICATEPARAM, err);
            clearParameterFields();
            return;
        }

        issueSuccessMessage($"Successfully added parameter '{parameterKey} with value' '{parameterValue}' to the parameter list!");

        addParameterToUIGrid(parameterKey, parameterValue);

        clearParameterFields();
    }

    private void addFolderButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (addFolderButton.IsEnabled)
        {
            string? s = folders.Text;

            if (dirExists(s))
            {
                if (folderList.Contains(s))
                {
                    issueErrorMessage_NoExc(ErrMsgs.DUPLICATEFOLDER + $" Folder: {s}");
                    folders.Text = "";
                    return;
                }

                folderList.Add(s);

                addFolderToUI(s);

                issueSuccessMessage($"Successfully added {s} to the folder list!");

                if (taskID == (int)EngineOperations.MoveLinear)
                {
                    addFolderButton.IsEnabled = false;
                    issueWarningMessage(WrnMsgs.MAXFOLDERSREACHED + " Field: Folders");
                }
            }

            folders.Text = "";
        }
    }

    private void addFileButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (addFileButton.IsEnabled)
        {
            string? s = files.Text;

            if (fileExists(s))
            {
                if (fileList.Contains(s))
                {
                    issueErrorMessage_NoExc(ErrMsgs.DUPLICATEFILE + $" File: {s}");
                    files.Text = "";
                    return;
                }

                fileList.Add(s);

                addFileToUI(s);

                issueSuccessMessage($"Successfully added {s} to the file list!");

                if (taskID == (int)EngineOperations.AddParametersSingular)
                {
                    issueWarningMessage(WrnMsgs.MAXFILESREACHED);
                    addFileButton.IsEnabled = false;
                }
            }
            else
            {
                issueNormalMessage("Looking for directory instead...");
                if (dirExists(s))
                {
                    foreach (var item in getAllSubdirFiles(s))
                    {
                        if (fileList.Contains(item))
                        {
                            issueErrorMessage_NoExc(ErrMsgs.DUPLICATEFILE + $" File: {item}");
                            files.Text = "";
                            return;
                        }

                        fileList.Add(item);

                        addFileToUI(item);

                        issueSuccessMessage($"Successfully added {item} to the file list!");

                        if (taskID == (int)EngineOperations.AddParametersSingular)
                        {
                            issueWarningMessage(WrnMsgs.MAXFILESREACHED);
                            addFileButton.IsEnabled = false;
                            break;
                        }
                    }
                }
            }

            files.Text = "";
        }
    }

    private void clearParameterFields()
    {
        parameterKeys.Clear();
        parameterValues.Clear();
    }

    private void addParameterToUIGrid(string k, string v)
    {
        alternateGridLineBackgroundColor();

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

    private void addFileToUI(string file)
    {
        alternateFileLineBackgroundColor();

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

    private void addFolderToUI(string folder)
    {
        alternateFolderLineBackgroundColor();

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

    private void redrawFilesToUI()
    {
        currentFiles.Text = "";
        currentFiles.Inlines.Clear();

        foreach (var item in fileList)
        {
            addFileToUI(item);
        }
    }

    private void redrawFoldersToUI()
    {
        currentFolders.Text = "";
        currentFolders.Inlines.Clear();

        foreach (var item in folderList)
        {
            addFolderToUI(item);
        }
    }

    private void redrawParametersToUI()
    {
        gridParameterKeys.Text = "";
        gridParameterValues.Text = "";

        gridParameterKeys.Inlines.Clear();
        gridParameterValues.Inlines.Clear();

        foreach (DictionaryEntry item in parametersList)
        {
            addParameterToUIGrid(item.Key.ToString(), item.Value.ToString());
        }
    }

    private void files_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            addFileButton_Click(sender, e);
        }
    }

    private void folders_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            addFolderButton_Click(sender, e);
        }
    }

    private void parameterPair_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            addParameterPairButton_Click(sender, e);
        }
    }

    //TODO: you were trying to prevent addparameters from adding a duplicate parameter; also trying to find a way to deduplicate files

    private HashSet<string> convertFilesFromFolders(List<string> folders)
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

    private HashSet<string> convertFilesFromFolders(List<string> folders, List<string> spareFiles)
    {
        HashSet<string> x = convertFilesFromFolders(folders);
        foreach (var file in spareFiles)
        {
            x.Add(file);
        }
        return x;
    }

    private void removeLastParameterPair()
    {
        issueWarningMessage(WrnMsgs.REMOVEDPARAM);
        parametersList.RemoveAt(parametersList.Count - 1);
    }

    private void resetAllFields()
    {
        enableAllButtons();
        clearAllTextBoxes();
        clearAllStoredValues();
        redrawAllUI();
    }

    private void enableAllButtons()
    {
        addFileButton.IsEnabled = true;
        addFolderButton.IsEnabled = true;
        addParameterPairButton.IsEnabled = true;
    }

    private void disableAllButtons()
    {
        addFileButton.IsEnabled = false;
        addFolderButton.IsEnabled = false;
        addParameterPairButton.IsEnabled = false;
    }

    private void clearAllTextBoxes()
    {
        files.Text = "";
        folders.Text = "";
        parameterKeys.Text = "";
        parameterValues.Text = "";
    }

    private void clearAllStoredValues()
    { 
        fileList = new List<string>();
        folderList = new List<string>();
        parametersList = new OrderedDictionary();
    }

    private void redrawAllUI()
    {
        redrawFilesToUI();
        redrawFoldersToUI();
        redrawParametersToUI();
    }
}