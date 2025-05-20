import js from "@eslint/js";
import tseslint from "typescript-eslint";
import globals from "globals";
import vuePlugin from "eslint-plugin-vue";
import vueParser from "vue-eslint-parser";

export default [
	// Ignore files
	{
		ignores: [
			"dist",
			"node_modules",
			"coverage",
			".eslintrc.js",
			"vite.config.*",
			"**/*.d.ts",
			"node_modules/**"
		]
	},

	// JavaScript base configuration
	js.configs.recommended,
  
	// TypeScript base configuration for .ts files
	...tseslint.configs.recommended,
  
	// TypeScript override for ts files only
	{
		files: ["**/*.ts", "**/*.tsx"],
		languageOptions: {
			parser: tseslint.parser,
			parserOptions: {
				ecmaVersion: "latest",
				sourceType: "module",
				project: "./tsconfig.app.json"
			}
		},
		rules: {
			"@typescript-eslint/no-explicit-any": "warn",
			"@typescript-eslint/explicit-function-return-type": "off",
			"@typescript-eslint/no-non-null-assertion": "warn",
			"@typescript-eslint/no-unused-vars": ["error", { 
				argsIgnorePattern: "^_",
				varsIgnorePattern: "^_"
			}]
		}
	},
  
	// Vue 3 configuration
	// Incorporate Vue plugin's configs
	...vuePlugin.configs["flat/recommended"],
  
	// Vue files specific configuration
	{
		files: ["**/*.vue"],
		languageOptions: {
			parser: vueParser,
			parserOptions: {
				ecmaVersion: "latest",
				sourceType: "module",
				extraFileExtensions: [".vue"],
				parser: {
					// Use TS parser for <script lang="ts"> blocks
					ts: tseslint.parser,
					// Use the same for regular scripts
					js: tseslint.parser
				},
				project: "./tsconfig.app.json"
			}
		},
		// Vue specific custom rules
		rules: {
			"vue/html-self-closing": ["error", {
				html: {
					void: "always",
					normal: "always",
					component: "always"
				}
			}],
			"vue/multi-word-component-names": "error",
			"vue/no-v-html": "warn",
			"vue/require-default-prop": "error",
			"vue/attributes-order": "warn",
			"vue/html-indent": ["error", "tab"]
		}
	},
  
	// Apply TypeScript rules to Vue files with <script lang="ts">
	{
		files: ["**/*.vue"],
		rules: {
			"@typescript-eslint/no-explicit-any": "warn",
			"@typescript-eslint/explicit-function-return-type": "off",
			"@typescript-eslint/no-non-null-assertion": "warn",
			"@typescript-eslint/no-unused-vars": ["error", { 
				argsIgnorePattern: "^_",
				varsIgnorePattern: "^_"
			}]
		}
	},

	// General rules for all files
	{
		rules: {
			"no-console": process.env.NODE_ENV === "production" ? "error" : "warn",
			"no-debugger": process.env.NODE_ENV === "production" ? "error" : "warn",
			"eqeqeq": "error",
			"no-var": "error",
			"prefer-const": "error",
			"no-unused-vars": "off", // TypeScript handles this
			"indent": ["error", "tab"],
			"quotes": ["error", "double"]
		}
	},
  
	// Add browser globals
	{
		languageOptions: {
			globals: {
				...globals.browser
			}
		}
	}
]
