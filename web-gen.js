const fs = require('node:fs/promises');

var build_dir = "";
var src_dir = "";
var template_dir = "";

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

	//read out filenames
	for await(file of dir) {
		if(file.isFile()) {
			let name = file.path + "/" + file.name;
			dictionary[name] = await fs.readFile(name);
		} else {
			await load_files(file.path +"/" + file.name, dictionary);
		}
	}
}

let templates = {};
load_files("old", templates)
.catch((err) => {
	console.log(`Something went wrong with loading templates: ${err}`);
})
.finally(() => {
	console.log(templates["old/Program.cs"].toString());
})