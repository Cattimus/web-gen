const fs = require('node:fs/promises');

var build_dir = "";
var src_dir = "";
var template_dir = "";

//assemble loaded file structure from template dir
async function load_templates(path, templates) {
	let dir = await fs.opendir(path)
	.catch((error) => {
		console.log(`Unable to open directory: ${path}`)
		return null;
	});

	//guard case for if directory doesn't exist
	if (dir == null) {
		return null;
	}

	//read out filenames
	for await(file of dir) {
		if(file.isFile()) {
			let name = file.path + "/" + file.name;
			templates[name] = await fs.readFile(name);
		} else {
			await load_templates(file.path +"/" + file.name, templates);
		}
	}
}

let templates = {};
load_templates("old", templates)
.catch((err) => {
	console.log(`Something went wrong with loading templates: ${err}`);
})
.finally(() => {
	console.log(templates["old/Program.cs"].toString());
})