string source_dir   = "NOT PROVIDED";
string build_dir    = Environment.CurrentDirectory + "/build";
string template_dir = "NOT PROVIDED";

//lists to hold data
List<string> source_files = new List<string>();
List<string> parse_files = new List<string>();
Dictionary<string, string> template_files = new Dictionary<string, string>();

//no input is provided
if(args.Count() < 1)
{
	display_helptext();
	Environment.Exit(-1);
}

//parse command line arguments
int i = 0;
while(i < args.Count())
{
	switch(args[i])
	{
		//template directory
		case "-t":
			template_dir = Environment.CurrentDirectory + "/" + args[i+1];
			i += 2;
		break;

		//build directory
		case "-b":
			build_dir = Environment.CurrentDirectory + "/" + args[i+1];
			i += 2;
		break;

		//source directory
		case "-s":
			source_dir = Environment.CurrentDirectory + "/" + args[i+1];
			i += 2;
		break;

		//parse extensions
		case "-parse":
			parse_files.AddRange(args[i+1].Split(","));
			i += 2;
		break;

		//unrecognized argument
		default:
			Console.WriteLine("Argument not recognized: '" + args[i] + "'");
			display_helptext();
			Environment.Exit(-1);
		break;
	}
}

print_info();

void display_helptext()
{
	Console.WriteLine("web-gen:");
	Console.WriteLine("-t [relative path]: path to the directory that holds template files");
	Console.WriteLine("-s [relative path]: select directory that holds source files");
	Console.WriteLine("-b [relative path]: select directory where compiled files will go. (defaults 'build' in current dir)");
	Console.WriteLine("-parse [comma,separated,list,of,file,extensions]: enable parsing files with these extensions");
}

void print_info()
{
	Console.WriteLine("Source directory: " + source_dir);
	Console.WriteLine("Template directory: " + template_dir);
	Console.WriteLine("Build directory: " + build_dir);
	Console.WriteLine("Parse extensions: " + String.Join(", ", parse_files));
}