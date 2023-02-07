string source_dir   = "NOT PROVIDED";
string build_dir    = "NOT PROVIDED";
string template_dir = "NOT PROVIDED";

//lists to hold data
List<string> source_files = new List<string>();
List<string> allowed_extensions = new List<string>();
Dictionary<string, string> template_files = new Dictionary<string, string>();

void display_helptest()
{
	Console.WriteLine("web-gen:");
	Console.WriteLine("-t [relative path]: path to the directory that holds template files");
	Console.WriteLine("-s [relative path]: select directory that holds source files");
	Console.WriteLine("-b [relative path]: select directory where compiled files will go. (defaults 'build' in current dir)");
	Console.WriteLine("-parse [comma,separated,list,of,file,extensions]: enable parsing files with these extensions");
}