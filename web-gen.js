const fs = require('node:fs/promises');

const gen_data = {
	build_dir: "",
	src_dir: "",
	template_dir: "",
	filetypes: [],
	debug: false,
	dry_run: false,

	template_files: {},
	source_files: {}
};

//regular expression to capture indentation level and filename
const pattern = /([ \t]*)(?:<WGT>)(.*)(?:<\/WGT>)/;

//load files and their contents into a dictionary
async function load_files(path, dictionary) {
	console.log(`Attempting to load files from: ${path}`);

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

	console.log("-f,-filetypes (list,of,extensions)");
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
async function handle_args() {
	for(let i = 2; i < process.argv.length; i++) {
		switch(process.argv[i]) {
			//template directory
			case "-template":
			case "-t":
				gen_data.template_dir = process.argv[i+1];
				i++;
			break;

			//build directory
			case "-build":
			case "-b":
				gen_data.build_dir = process.argv[i+1];
				i++;
			break;

			//source directory
			case "-source":
			case "-s":
				gen_data.src_dir = process.argv[i+1];
				i++;
			break;

			//parse extensions
			case "-filetypes":
			case "-f":
				gen_data.filetypes = process.argv[i+1].split(",");
				i++;
			break;

			case "--debug":
			case "--d":
				gen_data.debug = true;
			break;

			case "--help":
			case "--h":
			case "-h":
				print_helptext();
				process.exit(0);
			break;

			case "--dry-run":
				gen_data.dry_run = true;
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
async function init() {
	//print helptext if no args are given
	if(process.argv.length < 3) {
		print_helptext();
		process.exit(0);
	}

	await handle_args();

	//print error message if no template dir is given
	if(gen_data.template_dir == "") {
		console.log("You must at least specify a template directory to use web-gen.");
		print_helptext();
		process.exit(0);
	}

	if(gen_data.debug) {
		print_debug();
	}

	if(gen_data.dry_run) {
		console.log("This is a DRY RUN. That means no files will be written to disk or modified.");
	} else {
		//copy entire directory over
		await fs.cp(gen_data.src_dir, gen_data.build_dir, {recursive: true});
	}

	if(gen_data.filetypes.length == 0) {
		console.log("[WARNING] No file types have been passed so web-gen will not check any files.");
		console.log("You can pass filetypes to check with the -f option.");
		console.log();
	}

	//load files into memory
	await load_files(gen_data.template_dir, gen_data.template_files);
	await load_files(gen_data.src_dir, gen_data.source_files);
}

//apply indent to template file
function apply_indent(filename, indent) {
	let lines = "";
	let file = gen_data.template_files[filename];

	//file doesn't exist (shouldn't happen)
	if(file == null) {
		console.log(`Template file has not been loaded into memory: ${filename}`);
		return "";
	}

	//add proper indent to the start of each line
	file.split("\n").forEach((line) => {
		lines += (indent + line + "\n");
	});

	return lines;
}

//parse an individual file and perform replacements
async function parse_file(filename) {
	//check if file is loaded/exists
	if(gen_data.source_files[filename] == null) {
		console.log(`File has not been loaded into memory: ${filename}`);
		return;
	}

	//file that will be written
	var output_file = "";
	var output_filename = gen_data.build_dir + filename.substr(gen_data.src_dir.length);

	//iterate over lines
	let lines = gen_data.source_files[filename].split("\n");
	lines.forEach((line) => {

		//check for WGT tag group
		let result = pattern.exec(line);

		//match was found (we now need to replace it in the output)
		if(result != null) {
			let indent = result[1];
			let template_name = gen_data.template_dir + "/" + result[2];

			//insert template file into output stream
			output_file += apply_indent(template_name, indent);

		//append the original line if no match was found
		} else {
			output_file += line + "\n";
		}
	})

	//write file to output folder (only if it is going to be modified)
	let extension = output_filename.split(".")[1];
	if(!gen_data.dry_run && gen_data.filetypes.includes(extension)) {
		console.log(`Writing file: ${output_filename}`);
		await fs.writeFile(output_filename, output_file)

		.catch((error) => {
			console.log(`Could not write output file for: ${filename}.`);
		});
	}
}

init()

.then(() => {
	for (const [key, value] of Object.entries(gen_data.source_files)) {
		parse_file(key);
	}
});