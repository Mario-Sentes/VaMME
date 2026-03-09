using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VaMME.Views;

namespace VaMME;

public static class ErrMsgs
{
    public const string DIRNOTFOUND = "The specified directory does not exist!";
    public const string FILENOTFOUND = "The specified file does not exist!";
    public const string ACCESSDENIED = "Access is denied!";
    public const string INVALIDINPUT = "The input is invalid!";
}

public static class WrnMsgs
{
    public const string USINGCRUDELINES = "You are using a crude line editing tool! This is not recommended! Please use another approach if possible!";
}

public partial class Engine : Window
{
    private HashSet<string> batchsource = new HashSet<string>();

    string targetFileExtension = "*.vmt";

    const string paramRegex = @"^\s*""(\$[\w]+)""\s*""([^""]*)""";
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

    public Engine(int taskID)
    {
        InitializeComponent();

        for (int i = 0; i < validvmtparameters.Length; i++)
        {
            validparams.Add(validvmtparameters[i]);
        }

        for (int i = 0; i < acceptedfileextensions.Length; i++)
        {
            validext.Add(acceptedfileextensions[i]);
        }

        setTaskID(taskID);

        //ExecuteTask();
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
        Debug.Write("Task with ID {0} has been executed.", taskID.ToString());

        if (taskID == 1)
        {
            amendfilesindirectory();
        }
    }

    private void ReturnToMain()
    {
        var mainWindow = new MainWindow();
        mainWindow.Show();

        this.Close();
    }

    private void AbortTask(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ReturnToMain();
    }

    void amendfilesindirectory()
    {
        bool donebatching = false;

        string source = "";

        List<string> batchsource = new List<string>();

        while (!donebatching)
        {
            Console.WriteLine(@"Please enter your target directory, in format 'C:\DirectoryName1\DirectoryName2\DirectoryName3'. ");
            Console.WriteLine("Press enter to finish batch processing, or type X to return to the menu: ");

            source = @"C:\";

            source = Console.ReadLine();

            if (source == "X" || source == "x")
            {
                Console.Clear();
                Console.WriteLine("Returning to the main menu...");
                return;
            }

            if (source == "")
            {
                donebatching = true;
                break;
            }

            if (dirExists(source))
            {
                batchsource.Add(source);
            }
        }

        List<string> contents = new List<string>();

        Dictionary<string, string> par = new Dictionary<string, string>();

        par = getParams();

        if (par.Count <= 0)
        {
            Console.WriteLine("No parameters provided. Returning to the menu...");
            return;
        }

        Console.Clear();

        foreach (string dir in batchsource)
        {
            Console.WriteLine("Working in {0}...", dir);
            try
            {
                foreach (string file in Directory.EnumerateFiles(dir, "*.vmt"))
                {
                    Console.WriteLine("Found file {0}...", file);

                    contents.AddRange(getParameterisedFile(file, par));

                    foreach (var item in contents)
                    {
                        Console.WriteLine(item);
                    }

                    writeFile(file, ref contents);

                    contents.Clear();
                }
            }
            catch (Exception e)
            {
                issueErrorMessage(ErrMsgs.DIRNOTFOUND, e);
            }

            Console.WriteLine("Work in {0} complete!\n", dir);
        }
    }

    bool dirExists(string path)
    {
        if (path == null || path == "")
        {
            return false;
        }

        if (Directory.Exists(path))
        {
            Console.Clear();
            Console.WriteLine("Directory at path>\n");
            Console.WriteLine(path);
            Console.WriteLine("\n< has been located successfully!\n");
            return true;
        }

        Console.Clear();
        issueErrorMessage_NoExc(ErrMsgs.DIRNOTFOUND);
        return false;

    }

    bool fileExists(string path)
    {
        if (File.Exists(path))
        {
            Console.Clear();
            Console.WriteLine("File at path>\n");
            Console.WriteLine(path);
            Console.WriteLine("\n< has been located successfully!\n");
            return true;
        }

        Console.Clear();
        issueErrorMessage_NoExc(ErrMsgs.FILENOTFOUND);
        return false;

    }

    string validateDir()
    {
        bool validDir = false;

        string s = "";

        while (!validDir)
        {
            Console.WriteLine("Enter your desired directory: ");

            s = Console.ReadLine();

            if (dirExists(s))
            {
                validDir = true;
                break;
            }
        }

        return s;
    }

    string validateFile()
    {
        bool validFile = false;

        string s = "";

        while (!validFile)
        {
            Console.WriteLine("Enter your desired file: ");

            s = Console.ReadLine();

            if (fileExists(s))
            {
                validFile = true;
                break;
            }
        }

        return s;
    }

    List<string> validateMultiDir()
    {
        bool exit = false;

        List<string> s = new List<string>();

        string input = "NULL";

        while (!exit)
        {
            Console.WriteLine("Enter one of your desired directories: ");

            input = Console.ReadLine();

            if (input == "")
            {
                Console.WriteLine("Done adding...");
                break;
            }
            if (dirExists(input))
            {
                s.Add(input);
                Console.WriteLine("Successfully added directory '{0}' to list!\n", input);
            }
        }

        return s;
    }

    List<string> validateMultiDirRecursive()
    {
        bool exit = false;

        List<string> s = new List<string>();

        string input = "NULL";

        while (!exit)
        {
            Console.WriteLine("Enter your desired recursive directory: ");

            input = Console.ReadLine();

            if (dirExists(input))
            {
                s.AddRange(getAllSubdirs(input));
                Console.WriteLine("Successfully added the subdirectories of '{0}' to the list!\n", input);
                break;
            }
        }

        return s;
    }

    string[] getAllSubdirFiles(string path)
    {
        return Directory.GetFiles(path, targetFileExtension, SearchOption.AllDirectories);
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
        string s = "";

        char[] x = filepath.ToCharArray();

        Stack<char> xs = new Stack<char>();

        for (int i = x.Length - 1; i >= 0; i--)
        {
            if (x[i] == '\\')
            {
                break;
            }
            xs.Push(x[i]);
        }

        while (xs.Count != 0)
        {
            s += xs.Pop().ToString();
        }

        return s;
    }

    string getFileName_NoExtension(string filepath)
    {
        bool foundExtensionPoint = false;

        string s = "";

        char[] x = filepath.ToCharArray();

        Stack<char> xs = new Stack<char>();

        for (int i = x.Length - 1; i >= 0; i--)
        {
            if (x[i] == '\\')
            {
                break;
            }
            if ((x[i] == '.') && foundExtensionPoint == false)
            {
                foundExtensionPoint = true;
                continue;
            }
            if (foundExtensionPoint)
            {
                xs.Push(x[i]);
            }
        }

        while (xs.Count != 0)
        {
            s += xs.Pop().ToString();
        }

        return s;
    }

    Dictionary<string, string> getParams()
    {
        Dictionary<string, string> x = new Dictionary<string, string>();

        bool doneparam = false;

        while (!doneparam)
        {
            Console.Clear();
            Console.WriteLine("When a valid parameter is entered, pressing enter will continue to the parameter's value.");
            Console.WriteLine("If no parameter is provided and enter is pressed, the program will return to the menu.");
            Console.WriteLine("If a parameter was already provided, pressing enter will continue to the files being amended as requested.");

            displayParametersInDictionary(x);

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
                x.Add(proc, Console.ReadLine());
                Console.WriteLine("\n Parameter >\n \"{0}\" \n< has been successfully added!\n", proc);
            }
        }

        return x;
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
                Console.WriteLine("{0} is a valid parameter!", proc);
                x.Add(proc, "");
                Console.WriteLine("\n Parameter >\n \"{0}\" \n< has been successfully added!\n", proc);
            }
        }

        return x;
    }

    List<string> getParameterisedFile(string file, Dictionary<string, string> par)
    {
        List<string> x = new List<string>();

        using (StreamReader sr = File.OpenText(file))
        {
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                s = s.ToLower();

                var match = Regex.Match(s, paramRegex);

                if (match.Success)
                {
                    foreach (KeyValuePair<string, string> entry in par)
                    {
                        //if the parameter has been found               
                        if (match.Groups[1].Value == entry.Key)
                        {
                            s = $"                \"{entry.Key}\"     \"{entry.Value}\"";
                            Console.WriteLine(">>> Found eligible parameter {0} in file...", entry.Key);
                        }
                    }
                }

                x.Add(s);
            }
        }

        foreach (var item in x)
        {
            Console.WriteLine(item);
        }

        return x;
    }

    List<string> getParameterisedFile_FullLine(string file, Dictionary<string, string> par)
    {
        List<string> x = new List<string>();

        using (StreamReader sr = File.OpenText(file))
        {
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                var match = Regex.Match(s, paramRegex);

                if (match.Success)
                {
                    foreach (KeyValuePair<string, string> entry in par)
                    {
                        //if the parameter has been found               
                        if (match.Groups[1].Value.ToLower() == entry.Key.ToLower())
                        {
                            s = entry.Value;
                            Console.WriteLine(">>> Found eligible parameter {0} in file...", entry.Key);
                            Console.WriteLine(s);
                        }
                    }
                }

                x.Add(s);
            }
        }

        return x;
    }

    void copyFile(string file, string destination, string fn)
    {
        try
        {
            File.Copy(file, destination + @"\" + fn, true);
        }
        catch (Exception)
        {
            Console.WriteLine("Paste for '{0}' from '{1}' in '{2}' failed! Skipping...\n", fn, file, destination);
        }
        Console.WriteLine("Successfully pasted '{0}' in '{1}'!\n", fn, destination);
    }

    void writeFile(string fileandpath, ref List<string> contents)
    {
        string filename = getFileName(fileandpath);
        string filename_noext = getFileName_NoExtension(fileandpath);

        using (StreamWriter writetext = new StreamWriter(fileandpath))
        {
            Console.WriteLine("New file contents on '{0}':\n", fileandpath);

            for (int i = 0; i < contents.Count(); i++)
            {
                if (contents[i].Contains("@filename"))
                {
                    contents[i] = contents[i].Replace("@filename", filename_noext);
                }
                Console.WriteLine(contents[i]);
                writetext.WriteLine(contents[i]);
            }
        }
    }

    bool matchParamsToFile(string file, Dictionary<string, string> par)
    {
        int matchCounter = 0;
        int idealMatches = par.Count;

        using (StreamReader sr = File.OpenText(file))
        {
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                var match = Regex.Match(s, paramRegex);

                if (match.Success)
                {
                    foreach (KeyValuePair<string, string> entry in par)
                    {
                        //if a valid parameter has been found               
                        if (match.Groups[1].Value.ToLower() == entry.Key.ToLower())
                        {
                            matchCounter++;
                            par[entry.Key] = s;
                            Console.WriteLine(">>> Found eligible parameter {0} in file...", entry.Key);
                            Console.WriteLine(">>> Set value of '{0}' to '{1}'!", entry.Key, par[entry.Key]);
                        }
                    }
                }
            }
        }

        return (idealMatches == matchCounter);

    }

    void addParameterToFile()
    {
        bool doneAdding = false;

        string file = validateFile();

        string newLine = "";

        List<string> lineContent = new List<string>();

        Dictionary<string, string> par = new Dictionary<string, string>();

        par = getParams();

        using (StreamReader sr = File.OpenText(file))
        {
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                lineContent.Add(s);
                if (s.Contains('{') && !s.Contains('}') && !doneAdding)
                {
                    foreach (KeyValuePair<string, string> item in par)
                    {
                        newLine = $"                \"{item.Key}\"     \"{item.Value}\"";
                        lineContent.Add(newLine);
                    }
                    lineContent.Add("   ");

                    doneAdding = true;
                }
            }
        }

        writeFile(file, ref lineContent);
    }

    void addParameterToFile_auto(Dictionary<string, string> par, string file)
    {
        bool doneAdding = false;

        string newLine = "";

        List<string> lineContent = new List<string>();

        using (StreamReader sr = File.OpenText(file))
        {
            string s;
            while ((s = sr.ReadLine()) != null)
            {
                lineContent.Add(s);
                if (s.Contains('{') && !s.Contains('}') && !doneAdding)
                {
                    foreach (KeyValuePair<string, string> item in par)
                    {
                        newLine = $"                \"{item.Key}\"     \"{item.Value}\"";
                        lineContent.Add(newLine);
                    }
                    lineContent.Add("   ");

                    doneAdding = true;
                }
            }
        }

        writeFile(file, ref lineContent);
    }

    void addParameterToFiles()
    {
        string dir = validateDir();

        Dictionary<string, string> parameters = getParams();

        foreach (string file in Directory.EnumerateFiles(dir))
        {
            addParameterToFile_auto(parameters, file);
        }
    }

    void addParameterToFilesInDirs()
    {
        List<string> dirs = validateMultiDir();

        Dictionary<string, string> parameters = getParams();

        foreach (string dir in dirs)
        {
            foreach (string file in Directory.EnumerateFiles(dir))
            {
                addParameterToFile_auto(parameters, file);
            }
        }
    }

    List<string> getFileContents(string filename)
    {
        List<string> x = new List<string>();

        using (StreamReader sr = File.OpenText(filename))
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

        using (StreamReader sr = File.OpenText(filename))
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

    void createNewVMTsFromVTFdir()
    {
        //find desired VMT
        string base_file = validateFile();

        int indexOfDetailInBaseFile = -1;

        //find detail attribute
        List<string>? base_file_contents = getFileContents_SpecificQuery(base_file, "$detail");

        if (base_file_contents == null) //if a detail attribute is not found, return
        {
            Console.WriteLine("Selected file does not contain the specified parameter! Aborting...");
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

        string source_dir = validateDir();

        IEnumerable<string> filesFound = Directory.EnumerateFiles(source_dir, "*.vtf");

        if (filesFound.Count() <= 0)
        {
            Console.WriteLine("Selected directory does not contain any valid .vtf files for use! Aborting...");
            return;
        }

        Console.WriteLine("Enter the relative workpath of the source of the materials: ");
        string specifiedPath = Console.ReadLine();

        foreach (var item in filesFound)
        {
            base_file_contents[indexOfDetailInBaseFile] = $"                \"$detail\"     \"{specifiedPath + "\\" + getFileName_NoExtension(item)}\"";
            writeFile(base_file, ref base_file_contents);
            copyFile(base_file, source_dir, getFileName_NoExtension(item) + ".vmt");

        }


        //if a detail attribute is found,
        //make a copy of the base VMT with the name of the vtf for each vtf found in the specified folder,
        //then inject the detail parameter with the file path of the vtf file
    }

    void fixFiles()
    {
        string sourceDir = validateDir();

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
                    Console.WriteLine("Found back slash on line: " + item);
                    wrongSlashIndices.Add(contentsoffile.IndexOf(item));
                }
            }

            if (detailIndex != -1)
            {

                string stringAtDetail = contentsoffile[detailIndex];
                if (!stringAtDetail.Contains(".vtf"))
                {
                    Console.WriteLine("Does not have a valid .vtf signature for detail.");
                    stringAtDetail = stringAtDetail.Remove(stringAtDetail.Length - 1);
                    stringAtDetail = stringAtDetail + ".vtf\"";

                    Console.WriteLine("Fix: " + stringAtDetail);

                    contentsoffile[detailIndex] = stringAtDetail;

                    Console.WriteLine("New line: " + contentsoffile[detailIndex]);
                }

                detailIndex = -1;
            }

            if (wrongSlashIndices.Count > 0)
            {
                foreach (var index in wrongSlashIndices)
                {
                    string stringContainingWrongSlash = contentsoffile[index];
                    Console.WriteLine("Found back slash line...: {0}", stringContainingWrongSlash);
                    stringContainingWrongSlash = stringContainingWrongSlash.Replace('\\', '/');
                    contentsoffile[index] = stringContainingWrongSlash;
                    Console.WriteLine("New line: " + contentsoffile[index]);
                }

                wrongSlashIndices.Clear();
            }

            writeFile(file, ref contentsoffile);
        }
    }

    void displayParametersInDictionary(Dictionary<string, string> x)
    {
        Console.WriteLine("\nCurrent parameters: \n");
        Console.ForegroundColor = ConsoleColor.Yellow;
        if (x.Count < 1)
        {
            Console.WriteLine("No parameters added yet.");
        }
        foreach (var item in x)
        {
            Console.WriteLine("\"{0}\"  \"{1}\"", item.Key, item.Value);
        }
        Console.ForegroundColor = ConsoleColor.White;
    }

    void displayParametersInDictionaryNoVal(Dictionary<string, string> x)
    {
        Console.WriteLine("\nCurrent parameters: \n");
        Console.ForegroundColor = ConsoleColor.Yellow;
        if (x.Count < 1)
        {
            Console.WriteLine("No parameters added yet.");
        }
        else
        {
            foreach (var item in x)
            {
                Console.WriteLine("\"{0}\"", item.Key);
            }
        }
        Console.ForegroundColor = ConsoleColor.White;
    }

    void issueWarningMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("WARNING: {0} \n", message);
        Console.ForegroundColor = ConsoleColor.White;
    }

    void issueErrorMessage(string message, Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ERROR: {0} \n", message);
        Console.WriteLine(e.ToString() + "\n");
        Console.ForegroundColor = ConsoleColor.White;
    }

    void issueErrorMessage_NoExc(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ERROR: {0} \n", message);
        Console.ForegroundColor = ConsoleColor.White;
    }
}