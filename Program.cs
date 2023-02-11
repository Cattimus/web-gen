//TODO - add a true "template" that will accept a json string as an argument
//this template will automatically fill in based on the rules set by web-gen



//no input is provided
if(args.Count() < 1)
{
	Utilities.display_helptext();
	Environment.Exit(-1);
}

//set up environment for program to run
Utilities.handle_args(args);
Utilities.check_dirs();
Utilities.print_info();

//populate files into memory
if(Data.template_dir != "NOT PROVIDED")
{
	Console.WriteLine("Loading templates into memory...");
	Utilities.populate(ref Data.template_files, Data.template_dir);
}
if(Data.source_dir != "NOT_PROVIDED")
{
	Console.WriteLine("Loading source files into memory...");
	Utilities.populate(ref Data.source_files, Data.source_dir);
}

Console.WriteLine("Copying directories to build dir...");
Utilities.copy_dirs(Data.build_dir, Data.source_dir);

//copy non-parsing files to build
Console.WriteLine("Copying files that will not be parsed...");
foreach(var file in Data.direct_copy)
{
	var output_name = Data.build_dir + file.Remove(0, Data.source_dir.Length);
	File.Copy(file, output_name, true);
	
}

//parse all files that require parsing
Console.WriteLine("Parsing files...");
foreach(var file in Data.source_files)
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
			foreach(var template in Data.template_files)
			{
				//filenames are a match
				if(template.Key.Remove(0, Data.template_dir.Length + 1).Equals(filename) || template.Key.Equals(filename))
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
	var output_filename = Data.build_dir + file.Key.Remove(0, Data.source_dir.Length);
	File.WriteAllText(output_filename, output_file);

}
Console.WriteLine("Done.");
