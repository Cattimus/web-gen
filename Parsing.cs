public static class Parsing
{
	//parse and execute copy directive
	public static void parse_copy(ref string output_file, string line, string filename, ref int line_count)
	{
		string whitespace = line.Split("/")[0];

		//invalid copy directive
		if(!string.IsNullOrWhiteSpace(whitespace))
		{
			Console.WriteLine("Warning - Copy directive reached but is invalid.");
			Console.WriteLine("File: " + filename + " at line: " + line_count);
			output_file += line + "\n";
			line_count++;
		}

		//check for file in templates
		bool match_found = false;
		string match_name = line.Split(":")[1];
		foreach(var template in Data.template_files)
		{
			//filenames are a match
			if(template.Key.Remove(0, Data.template_dir.Length + 1).Equals(match_name) || template.Key.Equals(match_name))
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
			Console.WriteLine("WARNING - copy directive was found on line:" + line_count + " of " + filename);
			Console.WriteLine("Template: " + match_name + " could not be found.");
			Console.WriteLine();
		}
	}
}