import { PLUGIN_NAME, getFilePath, getFilter, initializeESLint, lintFiles, shouldIgnoreModule } from "./utils-CY0l8nbx.js";
import { parentPort, workerData } from "node:worker_threads";
import debugWrap from "debug";

//#region src/worker.ts
const debug = debugWrap(`${PLUGIN_NAME}:worker`);
const options = workerData.options;
const filter = getFilter(options);
let eslintInstance;
let formatter;
let outputFixes;
const initPromise = initializeESLint(options).then((result) => {
	eslintInstance = result.eslintInstance;
	formatter = result.formatter;
	outputFixes = result.outputFixes;
	return result;
});
(async () => {
	debug("==== worker start ====");
	debug("Initialize ESLint");
	const { eslintInstance: eslintInstance$1, formatter: formatter$1, outputFixes: outputFixes$1 } = await initPromise;
	if (options.lintOnStart) {
		debug("Lint on start");
		lintFiles({
			files: options.include,
			eslintInstance: eslintInstance$1,
			formatter: formatter$1,
			outputFixes: outputFixes$1,
			options
		});
	}
})();
parentPort?.on("message", async (id) => {
	if (!eslintInstance) await initPromise;
	debug("==== worker message event ====");
	debug(`id: ${id}`);
	const shouldIgnore = await shouldIgnoreModule(id, filter, eslintInstance);
	debug(`should ignore: ${shouldIgnore}`);
	if (shouldIgnore) return;
	const filePath = getFilePath(id);
	debug(`filePath: ${filePath}`);
	lintFiles({
		files: options.lintDirtyOnly ? filePath : options.include,
		eslintInstance,
		formatter,
		outputFixes,
		options
	});
});

//#endregion