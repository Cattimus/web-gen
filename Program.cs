string current_dir  = Environment.CurrentDirectory;
string source_dir   = "NOT PROVIDED";
string build_dir    = Environment.CurrentDirectory + "/build";
string template_dir = "NOT PROVIDED";

//lists to hold data
List<string> parse_files = new List<string>();
Dictionary<string, string> source_files = new Dictionary<string, string>();
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
			template_dir = current_dir + "/" + args[i+1];
			i += 2;
		break;

		//build directory
		case "-b":
			build_dir = current_dir + "/" + args[i+1];
			i += 2;
		break;

		//source directory
		case "-s":
			source_dir = current_dir + "/" + args[i+1];
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

//check that directories exist
check_dirs();

//print info to the user
print_info();

//populate files into memory
if(template_dir != "NOT PROVIDED")
{
	Console.WriteLine("Loading templates into memory...");
	populate(ref template_files, template_dir);
}
if(source_dir != "NOT_PROVIDED")
{
	Console.WriteLine("Loading source files into memory...");
	populate(ref source_files, source_dir);
}

//TODO - this could use some optimization (and refactoring. it is ugly)
//go through each source file
foreach(var file in source_files)
{
	var text = file.Value;
	string output_file = "";
	Console.WriteLine("file: " + file.Key);

	//execute all copy directives in file
	int line_count = 1;
	foreach(var line in text.Split("\n"))
	{
		//line contains a copy directive
		if(line.Contains("//!web-copy:"))
		{
			string whitespace = line.Split("/")[0];

			//invalid copy directive
			if(!string.IsNullOrWhiteSpace(whitespace))
			{
				Console.WriteLine("Warning - Copy directive reached but is invalid.");
				Console.WriteLine("File: " + file.Key + " at line: " + line_count);
				output_file += line + "\n";
				line_count++;
				continue;
			}

			//remove working directory from filename (for comparison)
			string filename = line.Split(":")[1];
			foreach(var template in template_files)
			{
				//filenames are a match
				if(template.Key.Remove(0, template_dir.Length + 1).Equals(filename) || template.Key.Equals(filename))
				{
					//copy template to file (at proper indentation level)
					foreach(var template_line in template.Value.Split("\n"))
					{
						output_file += whitespace + template_line + "\n";
					}

					//we don't need to keep searching for files
					break;
				}
			}
		}

		//copy line normally
		else
		{
			output_file += line + "\n";
		}

		line_count++;
	}

	//write constructed file to output
	var output_filename = build_dir + file.Key.Remove(0, source_dir.Length);
	File.WriteAllText(output_filename, output_file);

}

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
	Console.WriteLine("[WEB-GEN]");
	Console.WriteLine("Source: " + source_dir);
	Console.WriteLine("Template: " + template_dir);
	Console.WriteLine("Build: " + build_dir);
	Console.WriteLine("Parse files: [" + String.Join(", ", parse_files) + "]");
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

//populate files into memory recursively
void populate(ref Dictionary<string,string> container, string path)
{
	var files = Directory.GetFiles(path);
	var dirs = Directory.GetDirectories(path);

	//check all files in directory
	foreach(var file in files)
	{
		//check if the file is of the correct type
		bool parse = false;
		foreach(var filetype in parse_files)
		{
			if(file.Contains(filetype))
			{
				parse = true;
				break;
			}
		}

		//read file into memory
		if(parse)
		{
			container[file] = File.ReadAllText(file);
		}
	}

	//recursively get all files in subdirectories
	foreach(var dir in dirs)
	{
		populate(ref container, dir);
	}
}
