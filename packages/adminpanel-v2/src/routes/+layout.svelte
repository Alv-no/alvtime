<script>
	import '../app.css';
	import 'iconify-icon';
	import Sidebar from '../components/sidebar/Sidebar.svelte';
</script>

<div class="flex h-full">
	<Sidebar />
	<slot />
</div>
