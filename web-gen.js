const fs = require('node:fs/promises');

const gen_data = {
	build_dir: "",
	src_dir: "",
	template_dir: "",
	filetypes: [],
	debug: false,

	template_files: {},
	source_files: {}
};

//regular expression to capture indentation level and filename
const pattern = /([ \t]*)(?:<WGT>)(.*)(?:<\/WGT>)/;

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
async function handle_args(data) {
	for(let i = 2; i < process.argv.length; i++) {
		switch(process.argv[i]) {
			//template directory
			case "-template":
			case "-t":
				data.template_dir = process.argv[i+1];
				i++;
			break;

			//build directory
			case "-build":
			case "-b":
				data.build_dir = process.argv[i+1];
				i++;
			break;

			//source directory
			case "-source":
			case "-s":
				data.src_dir = process.argv[i+1];
				i++;
			break;

			//parse extensions
			case "-parse":
			case "-p":
				data.filetypes = process.argv[i+1].split(",");
				i++;
			break;

			case "--debug":
			case "--d":
				data.debug = true;
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

//set up all arguments and file lists
async function init(data) {
	//print helptext if no args are given
	if(process.argv.length < 3) {
		print_helptext();
		process.exit(0);
	}

	await handle_args(data);

	//print error message if no template dir is given
	if(data.template_dir == "") {
		console.log("You must at least specify a template directory to use web-gen.");
		print_helptext();
		process.exit(0);
	}

	if(data.debug) {
		print_debug();
	}

	//load files into memory
	await load_files(data.template_dir, data.template_files);
	await load_files(data.src_file, data.source_files);
}

//parse an individual file and perform replacements
async function parse_file(filename, data) {
	//check if file is loaded/exists
	if(data.source_files[filename] == null) {
		console.log(`File has not been loaded into memory: ${filename}`);
		return;
	}

	output_file = ""

	//iterate over lines
	let lines = data.source_files[filename].split("\n");
	lines.forEach((line) => {

		//check for WGT tag group
		let result = pattern.exec(line);

		//match was found (we now need to replace it in the output)
		if(result != null) {
			let indentation = result[1];
			let template_name = result[2];

		//append the original line if no match was found
		} else {
			output_file += line;
		}
	})
}