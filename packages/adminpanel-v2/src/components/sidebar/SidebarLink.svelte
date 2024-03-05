<script lang="ts">
	import 'iconify-icon';

	export let text: string;
	export let url: string;
	export let icon: string;
</script>

<a href={url} class="flex items-center gap-2 p-1 group">
	<iconify-icon {icon} class="text-alv-dark-blue rounded-md group-hover:bg-gray-200" width="40px" />
	<span class="text-alv-dark-blue font-bold">
		{text}
	</span>
</a>
