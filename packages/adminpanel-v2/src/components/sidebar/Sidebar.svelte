<script>
	import SidebarLink from './SidebarLink.svelte';
</script>

<div class="relative w-12">
	<nav class="h-full w-12 overflow-hidden hover:w-max bg-white hover:pr-4 absolute">
		<SidebarLink text={'Home'} url={'/'} icon={'majesticons:home'} />
		<SidebarLink text={'Kunder'} url={'/customers'} icon={'ic:round-corporate-fare'} />
		<SidebarLink text={'Ansatte'} url={'/employees'} icon={'fluent:person-20-filled'} />
	</nav>
</div>
