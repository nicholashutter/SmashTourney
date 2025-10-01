import { createFilter, normalizePath } from "@rollup/pluginutils";

//#region rolldown:runtime
var __create = Object.create;
var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __getProtoOf = Object.getPrototypeOf;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __commonJS = (cb, mod) => function() {
	return mod || (0, cb[__getOwnPropNames(cb)[0]])((mod = { exports: {} }).exports, mod), mod.exports;
};
var __copyProps = (to, from, except, desc) => {
	if (from && typeof from === "object" || typeof from === "function") for (var keys = __getOwnPropNames(from), i = 0, n = keys.length, key; i < n; i++) {
		key = keys[i];
		if (!__hasOwnProp.call(to, key) && key !== except) __defProp(to, key, {
			get: ((k) => from[k]).bind(null, key),
			enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable
		});
	}
	return to;
};
var __toESM = (mod, isNodeMode, target) => (target = mod != null ? __create(__getProtoOf(mod)) : {}, __copyProps(isNodeMode || !mod || !mod.__esModule ? __defProp(target, "default", {
	value: mod,
	enumerable: true
}) : target, mod));

//#endregion
//#region src/constants.ts
const ESLINT_SEVERITY = {
	ERROR: 2,
	WARNING: 1
};
const PLUGIN_NAME = "vite:eslint2";
const COLOR_MAPPING = {
	error: "red",
	warning: "yellow",
	plugin: "magenta"
};

//#endregion
//#region ../../node_modules/.pnpm/picocolors@1.1.1/node_modules/picocolors/picocolors.js
var require_picocolors = __commonJS({ "../../node_modules/.pnpm/picocolors@1.1.1/node_modules/picocolors/picocolors.js"(exports, module) {
	let p = process || {}, argv = p.argv || [], env = p.env || {};
	let isColorSupported = !(!!env.NO_COLOR || argv.includes("--no-color")) && (!!env.FORCE_COLOR || argv.includes("--color") || p.platform === "win32" || (p.stdout || {}).isTTY && env.TERM !== "dumb" || !!env.CI);
	let formatter = (open, close, replace = open) => (input) => {
		let string = "" + input, index = string.indexOf(close, open.length);
		return ~index ? open + replaceClose(string, close, replace, index) + close : open + string + close;
	};
	let replaceClose = (string, close, replace, index) => {
		let result = "", cursor = 0;
		do {
			result += string.substring(cursor, index) + replace;
			cursor = index + close.length;
			index = string.indexOf(close, cursor);
		} while (~index);
		return result + string.substring(cursor);
	};
	let createColors = (enabled = isColorSupported) => {
		let f = enabled ? formatter : () => String;
		return {
			isColorSupported: enabled,
			reset: f("\x1B[0m", "\x1B[0m"),
			bold: f("\x1B[1m", "\x1B[22m", "\x1B[22m\x1B[1m"),
			dim: f("\x1B[2m", "\x1B[22m", "\x1B[22m\x1B[2m"),
			italic: f("\x1B[3m", "\x1B[23m"),
			underline: f("\x1B[4m", "\x1B[24m"),
			inverse: f("\x1B[7m", "\x1B[27m"),
			hidden: f("\x1B[8m", "\x1B[28m"),
			strikethrough: f("\x1B[9m", "\x1B[29m"),
			black: f("\x1B[30m", "\x1B[39m"),
			red: f("\x1B[31m", "\x1B[39m"),
			green: f("\x1B[32m", "\x1B[39m"),
			yellow: f("\x1B[33m", "\x1B[39m"),
			blue: f("\x1B[34m", "\x1B[39m"),
			magenta: f("\x1B[35m", "\x1B[39m"),
			cyan: f("\x1B[36m", "\x1B[39m"),
			white: f("\x1B[37m", "\x1B[39m"),
			gray: f("\x1B[90m", "\x1B[39m"),
			bgBlack: f("\x1B[40m", "\x1B[49m"),
			bgRed: f("\x1B[41m", "\x1B[49m"),
			bgGreen: f("\x1B[42m", "\x1B[49m"),
			bgYellow: f("\x1B[43m", "\x1B[49m"),
			bgBlue: f("\x1B[44m", "\x1B[49m"),
			bgMagenta: f("\x1B[45m", "\x1B[49m"),
			bgCyan: f("\x1B[46m", "\x1B[49m"),
			bgWhite: f("\x1B[47m", "\x1B[49m"),
			blackBright: f("\x1B[90m", "\x1B[39m"),
			redBright: f("\x1B[91m", "\x1B[39m"),
			greenBright: f("\x1B[92m", "\x1B[39m"),
			yellowBright: f("\x1B[93m", "\x1B[39m"),
			blueBright: f("\x1B[94m", "\x1B[39m"),
			magentaBright: f("\x1B[95m", "\x1B[39m"),
			cyanBright: f("\x1B[96m", "\x1B[39m"),
			whiteBright: f("\x1B[97m", "\x1B[39m"),
			bgBlackBright: f("\x1B[100m", "\x1B[49m"),
			bgRedBright: f("\x1B[101m", "\x1B[49m"),
			bgGreenBright: f("\x1B[102m", "\x1B[49m"),
			bgYellowBright: f("\x1B[103m", "\x1B[49m"),
			bgBlueBright: f("\x1B[104m", "\x1B[49m"),
			bgMagentaBright: f("\x1B[105m", "\x1B[49m"),
			bgCyanBright: f("\x1B[106m", "\x1B[49m"),
			bgWhiteBright: f("\x1B[107m", "\x1B[49m")
		};
	};
	module.exports = createColors();
	module.exports.createColors = createColors;
} });

//#endregion
//#region src/utils.ts
var import_picocolors = __toESM(require_picocolors(), 1);
function interopDefault(m) {
	return m.default || m;
}
const getOptions = ({ test, dev, build, cache, include, exclude, eslintPath, formatter: formatter$1, lintInWorker, lintOnStart, lintDirtyOnly, emitError, emitErrorAsWarning, emitWarning, emitWarningAsError,...eslintConstructorOptions }) => ({
	test: test ?? false,
	dev: dev ?? true,
	build: build ?? false,
	cache: cache ?? true,
	include: include ?? ["src/**/*.{js,jsx,ts,tsx,vue,svelte}"],
	exclude: exclude ?? ["node_modules", "virtual:"],
	eslintPath: eslintPath ?? "eslint",
	formatter: formatter$1 ?? "stylish",
	lintInWorker: lintInWorker ?? false,
	lintOnStart: lintOnStart ?? false,
	lintDirtyOnly: lintDirtyOnly ?? true,
	emitError: emitError ?? true,
	emitErrorAsWarning: emitErrorAsWarning ?? false,
	emitWarning: emitWarning ?? true,
	emitWarningAsError: emitWarningAsError ?? false,
	...eslintConstructorOptions
});
const getFilter = (options) => createFilter(options.include, options.exclude);
const getESLintConstructorOptions = (options) => ({
	...Object.fromEntries(Object.entries(options).filter(([key]) => ![
		"test",
		"dev",
		"build",
		"include",
		"exclude",
		"eslintPath",
		"formatter",
		"lintInWorker",
		"lintOnStart",
		"lintDirtyOnly",
		"emitError",
		"emitErrorAsWarning",
		"emitWarning",
		"emitWarningAsError"
	].includes(key))),
	errorOnUnmatchedPattern: false
});
const initializeESLint = async (options) => {
	const { eslintPath, formatter: formatter$1 } = options;
	try {
		const _module = await import(eslintPath);
		const module$1 = interopDefault(_module);
		const ESLintClass = module$1.loadESLint ? await module$1.loadESLint() : module$1.ESLint || module$1.FlatESLint || module$1.LegacyESLint;
		const eslintInstance = new ESLintClass(getESLintConstructorOptions(options));
		const loadedFormatter = await eslintInstance.loadFormatter(formatter$1);
		const outputFixes = ESLintClass.outputFixes.bind(ESLintClass);
		return {
			eslintInstance,
			formatter: loadedFormatter,
			outputFixes
		};
	} catch (error) {
		throw new Error(`Failed to initialize ESLint. Have you installed and configured correctly? ${error}`);
	}
};
const isVirtualModule = (id) => id.startsWith("virtual:") || id[0] === "\0" || !id.includes("/");
const getFilePath = (id) => normalizePath(id).split("?")[0];
const shouldIgnoreModule = async (id, filter, eslintInstance) => {
	if (isVirtualModule(id)) return true;
	if (!filter(id)) return true;
	const filePath = getFilePath(id);
	if ([".vue", ".svelte"].some((extname) => filePath.endsWith(extname)) && id.includes("?") && id.includes("type=style")) return true;
	if (eslintInstance) return await eslintInstance.isPathIgnored(filePath);
	return false;
};
const removeESLintErrorResults = (results) => results.map((result) => {
	const filteredMessages = result.messages.filter((message) => message.severity !== ESLINT_SEVERITY.ERROR);
	const filteredSuppressedMessages = result.suppressedMessages.filter((message) => message.severity !== ESLINT_SEVERITY.ERROR);
	return {
		...result,
		messages: filteredMessages,
		suppressedMessages: filteredSuppressedMessages,
		errorCount: 0,
		fatalErrorCount: 0,
		fixableErrorCount: 0
	};
});
const removeESLintWarningResults = (results) => results.map((result) => {
	const filteredMessages = result.messages.filter((message) => message.severity !== ESLINT_SEVERITY.WARNING);
	const filteredSuppressedMessages = result.suppressedMessages.filter((message) => message.severity !== ESLINT_SEVERITY.WARNING);
	return {
		...result,
		messages: filteredMessages,
		suppressedMessages: filteredSuppressedMessages,
		warningCount: 0,
		fixableWarningCount: 0
	};
});
const filterESLintLintResults = (results) => results.filter((result) => result.errorCount > 0 || result.warningCount > 0);
const colorize = (text, textType) => import_picocolors.default[COLOR_MAPPING[textType]](text);
const log = (text, textType, context) => {
	console.log("");
	if (context) {
		if (textType === "error") context.error(text);
		else if (textType === "warning") context.warn(text);
	} else console.log(`${text}  Plugin: ${colorize(PLUGIN_NAME, "plugin")}\r\n`);
};
const lintFiles = async ({ files, eslintInstance, formatter: formatter$1, outputFixes, options }, context) => await eslintInstance.lintFiles(files).then(async (lintResults) => {
	if (!lintResults || lintResults.length === 0) return;
	if (options.fix) outputFixes(lintResults);
	let results = [...lintResults];
	if (!options.emitError) results = removeESLintErrorResults(results);
	if (!options.emitWarning) results = removeESLintWarningResults(results);
	results = filterESLintLintResults(results);
	if (results.length === 0) return;
	const formattedText = await formatter$1.format(results);
	let formattedTextType;
	if (results.some((result) => result.errorCount > 0)) formattedTextType = options.emitErrorAsWarning ? "warning" : "error";
	else formattedTextType = options.emitWarningAsError ? "error" : "warning";
	return log(formattedText, formattedTextType, context);
});

//#endregion
export { PLUGIN_NAME, getFilePath, getFilter, getOptions, initializeESLint, lintFiles, shouldIgnoreModule };