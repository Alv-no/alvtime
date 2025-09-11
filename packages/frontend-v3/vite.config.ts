import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
	esbuild: {
		supported: {
		'top-level-await': true //browsers can handle top-level-await features
		},
	},
	plugins: [vue(
		{
			template: {
				compilerOptions: {
					isCustomElement: (tag) => {
						// Allow custom elements to be used in Vue templates
						return tag.startsWith('swiper-');
					}
				}
			}
		}
	)],
	resolve: {
    	alias: {
      		'@': '/src',
		}
	},
	css: {
		preprocessorOptions: {
			scss: {
				additionalData: `@use "sass:color"; @import "@/assets/scss/global.scss";`
			}
		}
	}
});
