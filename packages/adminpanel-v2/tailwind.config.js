/** @type {import('tailwindcss').Config} */
export default {
	content: ['./src/**/*.{html,js,svelte,ts}'],
	theme: {
		extend: {
			colors: {
				alv: {
					blue: '#1E92D0',
					'dark-blue': '#061838',
					yellow: '#EABB26'
				}
			},
			gridTemplateColumns: {
				'16': 'repeat(16, minmax(0, 1fr))',
			}
		}
	},
	plugins: []
};