string current_dir  = Environment.CurrentDirectory;
string source_dir   = "NOT PROVIDED";
string build_dir    = Environment.CurrentDirectory + "/build";
string template_dir = "NOT PROVIDED";

//lists to hold data
List<string> parse_files = new List<string>();
List<string> direct_copy = new List<string>();
Dictionary<string, string> source_files = new Dictionary<string, string>();
Dictionary<string, string> template_files = new Dictionary<string, string>();

//no input is provided
if(args.Count() < 1)
{
	display_helptext();
	Environment.Exit(-1);
}

//set up environment for program to run
handle_args();
check_dirs();
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

Console.WriteLine("Copying directories to build dir...");
copy_dirs(build_dir, source_dir);

//copy non-parsing files to build
Console.WriteLine("Copying files that will not be parsed...");
foreach(var file in direct_copy)
{
	var output_name = build_dir + file.Remove(0, source_dir.Length);
	File.Copy(file, output_name, true);
	
}

//parse all files that require parsing
Console.WriteLine("Parsing files...");
foreach(var file in source_files)
{
	var text = file.Value;
	string output_file = "";

	//find all copy directives in file
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

			//check for file in templates
			bool match_found = false;
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

					//file has been found, we do not need to keep searching
					match_found = true;
					break;
				}
			}

			//template directive was included but no match was found
			if(!match_found)
			{
				Console.WriteLine();
				Console.WriteLine("WARNING - copy directive was found on line:" + line_count + " of " + file.Key);
				Console.WriteLine("Template: " + filename + " could not be found.");
				Console.WriteLine();
			}
		}

		//no template directive on line
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
Console.WriteLine("Done.");

//parse command line arguments
void handle_args()
{
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
}

//display program usage prompt
void display_helptext()
{
	Console.WriteLine();
	Console.WriteLine("[web-gen]:");

	Console.WriteLine("-s,-source (path)");
	Console.WriteLine("Path to the folder that contains source files");
	Console.WriteLine();

	Console.WriteLine("-p,-parse (list,of,extensions)");
	Console.WriteLine("Tell web-gen which types of files you want to be run through the parser.");
	Console.WriteLine();

	Console.WriteLine("-t,-template (path)");
	Console.WriteLine("Path to the folder that contains templates.");
	Console.WriteLine();

	Console.WriteLine("-b,-build (path)");
	Console.WriteLine("Path to the output(build) directory.");
	Console.WriteLine();

	Console.WriteLine("-A");
	Console.WriteLine("[NOT IMPLEMENTED] - use absolute paths instead of relative paths");
	Console.WriteLine("All paths are relative to the current working directory by default.");
	Console.WriteLine();

	Console.WriteLine("[Directives]:");
	Console.WriteLine("Copy: //!web-copy:[template name]");
	Console.WriteLine("Copy a template into the source code.");
	Console.WriteLine();
}

//print configuration info
void print_info()
{
	Console.WriteLine();
	Console.WriteLine("[WEB-GEN]");
	Console.WriteLine("Source: " + source_dir);
	Console.WriteLine("Template: " + template_dir);
	Console.WriteLine("Build: " + build_dir);
	Console.WriteLine("Parse files: [" + String.Join(", ", parse_files) + "]");
	Console.WriteLine();
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
		else
		{
			//if file is not a template, it should be added to the direct_copy list
			if(!file.Contains(template_dir))
			{
				direct_copy.Add(file);
			}
		}
	}

	//recursively get all files in subdirectories
	foreach(var dir in dirs)
	{
		populate(ref container, dir);
	}
}

//copy directory structure from src to dest
void copy_dirs(string dest, string src)
{
	var dirs = Directory.GetDirectories(src);

	foreach(var dir in dirs)
	{
		//create directory if it doesn't exist
		var stub = dir.Remove(0, src.Length);
		if(!Directory.Exists(dest + stub))
		{
			Directory.CreateDirectory(dest + stub);
		}

		//recursively copy any subdirectories
		copy_dirs(dest + stub, src + stub);
	}
}