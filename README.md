VaMME - Valve Mass Material Editor
A GUI-based mass file editor for Source engine games using .vmt files.

Features:
 - amend multiple files linearly/recursively parametrically
 - fix multiple files to a correct format
 - copy multiple files, as well as via EBMPC^
 - move multiple files
 - add parameters to one or multiple files
 - creating paired .vmt bases from a source of .vtf files
 
^EBMPC:
Example-based mass parameterised copy - 
Will copy one file into multiple target directories and within each directory, 
get the first matching example and inject it with the found parameters as specified by the user.



Installation:
1. Create a folder on your device somewhere accessible; for example, create a folder on your desktop called 'VaMME'
2. Go to the most recent branch of the repository (which can be found at https://github.com/Mario-Sentes/VaMME)
Now depending on whether you prefer to download the files directly from the GitHub website,
or via a command terminal (note that this requires that GitHub is *already installed on your machine*):



**Windows**:
For website downloads:
3. Click on Code > Local > Download as ZIP
4. Move the downloaded .zip archive where you have created your 'VaMME' folder (using the previous example, could be somewhere like 'C:\Users\YOURUSERNAME\Desktop\VaMME')
5. Right click the .zip and click "Extract all..."
6. Open the extracted folder and you should be able to see all relevant files, including the .sln Solution File.
7. Done! You can now delete the downloaded .zip as it is no longer required.

For terminal downloads:
3. Open Command Prompt
4. Enter this command > cd YOURVaMMEFOLDERLOCATION
Example: 
cd C:\Users\myUsername\Desktop\VaMME
5. Enter this command > git clone -b MOSTRECENTBRANCHNAME https://github.com/Mario-Sentes/VaMME.git
Example: git clone -b rev3 https://github.com/Mario-Sentes/VaMME.git
6. You should now be able to see a populated 'VaMME' folder. If that is the case, you can now close the terminal, and you're done!



**Linux and MacOS**:
For website Downloads:
3. Click on Code > Local > Download as ZIP (This step is the same across both platforms).
4. Move the downloaded .zip archive to the 'VaMME' folder you created earlier (for example, on your Desktop or in your home directory).
5. Extract the contents of the .zip file:
 - On macOS, right-click the .zip file and choose "Open With > Archive Utility."
 - On Linux, right-click and choose "Extract Here," or open the terminal and run:
   unzip NAMEOFDOWNLOADEDARCHIVE.zip -d ~/VaMME
   
6. Open the extracted folder and you should be able to see all relevant files (e.g., source code, project files).
7. Done! You can now delete the .zip file, as it's no longer required.

For terminal Downloads:
3. Open the terminal on your Linux or macOS system.
4. Navigate to the folder you created (for example, on your Desktop):
 - On Linux:
   > cd ~/Desktop/VaMME
   
 - On macOS:
   > cd ~/Desktop/VaMME

   If you haven't already installed Git, you can install it with the following commands:

      On Linux (Debian/Ubuntu-based distros):

      > sudo apt install git

      On macOS:

      > xcode-select --install  # This will install Command Line Tools, including Git.

5. Clone the repository by running the following command in the terminal:

> git clone -b MOSTRECENTBRANCHNAME https://github.com/Mario-Sentes/VaMME.git

Example:

> git clone -b rev3 https://github.com/RELEVANTREPO.git

6. Once the clone is complete, the RELEVANTREPO folder should now be populated with the project files.
7. Done! You can now close the terminal.

**Building**:
The provided repository should already include the built portable binaries, which can be found under "./bin/Release/net8.0/VaMME.exe".

If, for any reason, the application must be rebuilt: this can be done easily so long as you have Visual Studio 2022 installed on your machine.
Simply open the .sln file, and once everything is loaded properly (i.e. you can see the solution explorer and all relevant files properly), 
under "Solution 'VaMMe' (1 of 1 project)", right click on "VaMME", select "Publish".
Then, click on "Show all settings" and configure them to suit your system. By default:
 - "Configuration" should be set to "Release | Any CPU"
 - "Target framework" should be set to "net8.0"
 - "Deployment mode" should be set to "Framework-dependent"
 - "Target runtime" should be set to "Portable"
 - "Target location" should be set to "bin\Release\net8.0\publish\"