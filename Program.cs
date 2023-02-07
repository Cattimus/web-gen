﻿string source_dir   = "NOT PROVIDED";
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

check_dirs();
print_info();

//display program usage prompt
void display_helptext()
{
	Console.WriteLine("web-gen usage:");
	Console.WriteLine("-t [relative path]: path to the directory that holds template files");
	Console.WriteLine("-s [relative path]: select directory that holds source files");
	Console.WriteLine("-b [relative path]: select directory where compiled files will go. (defaults 'build' in current dir)");
	Console.WriteLine("-parse [comma,separated,list,of,file,extensions]: enable parsing files with these extensions");
}

//print configuration info
void print_info()
{
	Console.WriteLine("Source directory: " + source_dir);
	Console.WriteLine("Template directory: " + template_dir);
	Console.WriteLine("Build directory: " + build_dir);
	Console.WriteLine("Parse extensions: " + String.Join(", ", parse_files));
}

//check that all directories exist
void check_dirs()
{
	bool error = false;

	//check to see if source directory exists
	if(source_dir != "NOT PROVIDED")
	{
		if(!Directory.Exists(source_dir))
		{
			Console.WriteLine("Error - provided source directory does not exist.\nSource dir: " + source_dir);
			error = true;
		}
	}
	else
	{
		Console.WriteLine("Error - source directory must be provided.");
		error = true;
	}

	//check to see if build directory exists
	if(!Directory.Exists(build_dir))
	{
		Directory.CreateDirectory(build_dir);
		if(!Directory.Exists(build_dir))
		{
			Console.WriteLine("Error - build directory does not exist and cannot be created.\nBuild dir: " + build_dir);
			error = true;
		}
	}

	//check to see if template directory exists
	if(template_dir != "NOT PROVIDED")
	{
		if(!Directory.Exists(template_dir))
		{
			Console.WriteLine("Error - provided template directory does not exist.\nTemplate dir: " + template_dir);
			error = true;
		}
	}

	//exit on error
	if(error)
	{
		Environment.Exit(-1);
	}
}