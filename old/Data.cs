public static class Data
{
	public static string current_dir  = Environment.CurrentDirectory;
	public static string source_dir   = "NOT PROVIDED";
	public static string build_dir    = Environment.CurrentDirectory + "/build";
	public static string template_dir = "NOT PROVIDED";

	//lists to hold data
	public static List<string> parse_files = new List<string>();
	public static List<string> direct_copy = new List<string>();
	public static Dictionary<string, string> source_files = new Dictionary<string, string>();
	public static Dictionary<string, string> template_files = new Dictionary<string, string>();
}