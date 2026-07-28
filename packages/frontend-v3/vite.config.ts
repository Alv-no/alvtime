import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
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
		target: 'es2022', //browsers can handle top-level-await features
		rollupOptions: {
			output: {
				manualChunks(id) {
					if (!id.includes('/node_modules/')) return;
					if (/\/node_modules\/(vue|vue-router|pinia|@vue)\//.test(id)) return 'vue';
					if (id.includes('/node_modules/swiper/')) return 'swiper';
					if (/\/node_modules\/(axios|date-easter|fuse\.js|sortablejs-vue3|feather-icons|@hugeicons)\//.test(id)) return 'utils';
				}
			}
		}
	}
});
