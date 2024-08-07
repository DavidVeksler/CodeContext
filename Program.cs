using CodeContext;
using DotNet.Globbing;

var context = new MyAppsContext();

string path = args.Length > 0 ? args[0] : MyAppsContext.GetUserInput("Enter the path to index: ");
string ignorePath = MyAppsContext.GetUserInput("Enter path to .ignore file (press Enter for current directory): ");
        
if (string.IsNullOrWhiteSpace(ignorePath))
    ignorePath = Path.Combine(Directory.GetCurrentDirectory(), ".ignore");

MyAppsContext.LoadIgnorePatterns(ignorePath);

string additionalIgnores = MyAppsContext.GetUserInput("Enter additional files/directories to ignore (comma-separated, press Enter to skip): ");
if (!string.IsNullOrWhiteSpace(additionalIgnores))
{
    foreach (var pattern in additionalIgnores.Split(',', StringSplitOptions.RemoveEmptyEntries))
    {
        MyAppsContext.ignorePatterns.Add(Glob.Parse(pattern.Trim()));
    }
}

Console.WriteLine("Project Structure:");
Console.WriteLine(MyAppsContext.GetProjectStructure(path));

Console.WriteLine("File Contents:");
Console.WriteLine(MyAppsContext.GetFileContents(path));