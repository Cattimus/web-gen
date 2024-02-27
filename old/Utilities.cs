public static class Utilities
{
	//parse command line arguments
	public static void handle_args(string[] args)
	{
		int i = 0;
		while(i < args.Count())
		{
			switch(args[i])
			{
				//template directory
				case "-template":
				case "-t":
					Data.template_dir = Data.current_dir + "/" + args[i+1];
					i += 2;
				break;

				//build directory
				case "-build":
				case "-b":
					Data.build_dir = Data.current_dir + "/" + args[i+1];
					i += 2;
				break;

				//source directory
				case "-source":
				case "-s":
					Data.source_dir = Data.current_dir + "/" + args[i+1];
					i += 2;
				break;

				//parse extensions
				case "-parse":
				case "-p":
					Data.parse_files.AddRange(args[i+1].Split(","));
					i += 2;
				break;

				//unrecognized argument
				default:
					Console.WriteLine("Argument not recognized: '" + args[i] + "'");
					Utilities.display_helptext();
					Environment.Exit(-1);
				break;
			}
		}
	}

	//print configuration info
	public static void print_info()
	{
		Console.WriteLine();
		Console.WriteLine("[WEB-GEN]");
		Console.WriteLine("Source: " + Data.source_dir);
		Console.WriteLine("Template: " + Data.template_dir);
		Console.WriteLine("Build: " + Data.build_dir);
		Console.WriteLine("Parse files: [" + String.Join(", ", Data.parse_files) + "]");
		Console.WriteLine();
}


	//display program usage prompt
	public static void display_helptext()
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

	//check that all directories exist
	public static void check_dirs()
	{
		bool error = false;

		//check to see if source directory exists
		if(Data.source_dir != "NOT PROVIDED")
		{
			if(!Directory.Exists(Data.source_dir))
			{
				Console.WriteLine("Error - provided source directory does not exist.\nSource dir: " + Data.source_dir);
				error = true;
			}
		}
		else
		{
			Console.WriteLine("Error - source directory must be provided.");
			error = true;
		}

		//check to see if build directory exists
		if(!Directory.Exists(Data.build_dir))
		{
			Directory.CreateDirectory(Data.build_dir);
			if(!Directory.Exists(Data.build_dir))
			{
				Console.WriteLine("Error - build directory does not exist and cannot be created.\nBuild dir: " + Data.build_dir);
				error = true;
			}
		}

		//check to see if template directory exists
		if(Data.template_dir != "NOT PROVIDED")
		{
			if(!Directory.Exists(Data.template_dir))
			{
				Console.WriteLine("Error - provided template directory does not exist.\nTemplate dir: " + Data.template_dir);
				error = true;
			}
		}

		//exit on error
		if(error)
		{
			Environment.Exit(-1);
		}
	}

	//copy directory structure from src to dest
	public static void copy_dirs(string dest, string src)
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

	//populate files into memory recursively
	public static void populate(ref Dictionary<string,string> container, string path)
	{
		var files = Directory.GetFiles(path);
		var dirs = Directory.GetDirectories(path);

		//check all files in directory
		foreach(var file in files)
		{
			//check if the file is of the correct type
			bool parse = false;
			foreach(var filetype in Data.parse_files)
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
				if(!file.Contains(Data.template_dir))
				{
					Data.direct_copy.Add(file);
				}
			}
		}

		//recursively get all files in subdirectories
		foreach(var dir in dirs)
		{
			populate(ref container, dir);
		}
	}
}