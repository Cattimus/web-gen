const fs = require('node:fs/promises');

var build_dir = "";
var src_dir = "";
var template_dir = "";
var filetypes = [];
var debug = false;

//load files and their contents into a dictionary
async function load_files(path, dictionary) {
	let dir = await fs.opendir(path)
	.catch((error) => {
		console.log(`Unable to open directory: ${path}`)
		return null;
	});

	//guard case for if directory doesn't exist
	if (dir == null) {
		return;
	}

	//read all files recursively
	for await(file of dir) {
		if(file.isFile()) {
			let name = file.path + "/" + file.name;
			dictionary[name] = (await fs.readFile(name)).toString("utf-8");
		} else {
			await load_files(file.path +"/" + file.name, dictionary);
		}
	}
}

function print_helptext() {
	console.log();
	console.log("[web-gen]:");

	console.log("-s,-source (path)");
	console.log("Path to the folder that contains source files");
	console.log();

	console.log("-p,-parse (list,of,extensions)");
	console.log("Tell web-gen which types of files you want to be run through the parser.");
	console.log();

	console.log("-t,-template (path)");
	console.log("Path to the folder that contains templates.");
	console.log();

	console.log("-b,-build (path)");
	console.log("Path to the output(build) directory.");
	console.log();

	console.log("[Directives]:");
	console.log("Copy: <WGT>path_to_file.html</WGT>");
	console.log("Copy a template into the source code.");
	console.log();
}

function print_debug() {
	console.log(`build dir: ${build_dir}`);
	console.log(`source dir: ${src_dir}`);
	console.log(`template dir: ${template_dir}`);
	console.log(`filetypes: ${filetypes}`)
}

//extract data from command line arguments
function handle_args() {
	for(let i = 2; i < process.argv.length; i++) {
		switch(process.argv[i]) {
			//template directory
			case "-template":
			case "-t":
				template_dir = process.argv[i+1];
				i++;
			break;

			//build directory
			case "-build":
			case "-b":
				build_dir = process.argv[i+1];
				i++;
			break;

			//source directory
			case "-source":
			case "-s":
				src_dir = process.argv[i+1];
				i++;
			break;

			//parse extensions
			case "-parse":
			case "-p":
				filetypes = process.argv[i+1].split(",");
				i++;
			break;

			case "--debug":
			case "--d":
				debug = true;
			break;

			case "--help":
			case "-h":
				print_helptext();
				process.exit(0);
			break;

			//unrecognized argument
			default:
				console.log("Argument not recognized: '" + process.argv[i] + "'");
				print_helptext();
				process.exit(0);
			break;
		}
	}
}

//print helptext if no args are given
if(process.argv.length < 3) {
	print_helptext();
	process.exit(0);
}

handle_args();

//print error message if no template dir is given
if(template_dir == "") {
	console.log("You must at least specify a template directory to use web-gen.");
	print_helptext();
	process.exit(0);
}

if(debug) {
	print_debug();
}