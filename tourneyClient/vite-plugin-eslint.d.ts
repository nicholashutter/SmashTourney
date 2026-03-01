declare module "vite-plugin-eslint" {
    import type { PluginOption } from "vite";

    type EslintPluginOptions = {
        cache?: boolean;
        include?: string | string[];
        exclude?: string | string[];
        emitWarning?: boolean;
        emitError?: boolean;
        failOnWarning?: boolean;
        failOnError?: boolean;
        lintOnStart?: boolean;
        fix?: boolean;
    };

    export default function eslintPlugin(options?: EslintPluginOptions): PluginOption;
}