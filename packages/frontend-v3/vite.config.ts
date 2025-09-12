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
				additionalData: `@use "sass:color"; @use "@/assets/scss/global.scss"; @use "@/assets/scss/variables" as *;`
			}
		}
	},
	build: {
		rollupOptions: {
			output: {
				manualChunks: {
					vue: ['vue', 'vue-router', 'pinia'],
					swiper: ['swiper'],
					utils: ['axios', 'date-easter', 'fuse.js', 'sortablejs-vue3', 'feather-icons', '@hugeicons/core-free-icons', '@hugeicons/vue'],
					msal: ['@azure/msal-browser']
				}
			}
		}	
	}
});
