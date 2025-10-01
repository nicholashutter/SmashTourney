const require_utils = require('./utils-UEfUEvtH.cjs');
const node_worker_threads = require_utils.__toESM(require("node:worker_threads"));
const debug = require_utils.__toESM(require("debug"));

//#region src/worker.ts
const debug$1 = (0, debug.default)(`${require_utils.PLUGIN_NAME}:worker`);
const options = node_worker_threads.workerData.options;
const filter = require_utils.getFilter(options);
let eslintInstance;
let formatter;
let outputFixes;
const initPromise = require_utils.initializeESLint(options).then((result) => {
	eslintInstance = result.eslintInstance;
	formatter = result.formatter;
	outputFixes = result.outputFixes;
	return result;
});
(async () => {
	debug$1("==== worker start ====");
	debug$1("Initialize ESLint");
	const { eslintInstance: eslintInstance$1, formatter: formatter$1, outputFixes: outputFixes$1 } = await initPromise;
	if (options.lintOnStart) {
		debug$1("Lint on start");
		require_utils.lintFiles({
			files: options.include,
			eslintInstance: eslintInstance$1,
			formatter: formatter$1,
			outputFixes: outputFixes$1,
			options
		});
	}
})();
node_worker_threads.parentPort?.on("message", async (id) => {
	if (!eslintInstance) await initPromise;
	debug$1("==== worker message event ====");
	debug$1(`id: ${id}`);
	const shouldIgnore = await require_utils.shouldIgnoreModule(id, filter, eslintInstance);
	debug$1(`should ignore: ${shouldIgnore}`);
	if (shouldIgnore) return;
	const filePath = require_utils.getFilePath(id);
	debug$1(`filePath: ${filePath}`);
	require_utils.lintFiles({
		files: options.lintDirtyOnly ? filePath : options.include,
		eslintInstance,
		formatter,
		outputFixes,
		options
	});
});

//#endregion