<script lang="ts">
	export let updateFunction: Function;
	export let isDisabled: boolean;
	export let isActive: boolean = false;

	const BUTTON_CLASS_DEFAULT: string =
		'w-8 h-8 flex items-center justify-center content-center rounded';

	function onClick() {
		isActive = !isActive;
		updateFunction();
	}
</script>

{#if isDisabled}
	<button disabled class="{BUTTON_CLASS_DEFAULT} border border-gray-400" on:click={() => onClick()}>
		<iconify-icon icon={'fluent:edit-20-regular'} class="text-gray-400" width="22px" />
	</button>
{:else}
	<button
		class="{BUTTON_CLASS_DEFAULT} {isActive
			? 'border border-blue-500'
			: 'hover:border hover:border-black'}"
		on:click={() => onClick()}
	>
		<iconify-icon
			icon={'fluent:edit-20-regular'}
			class={isActive ? 'text-blue-500' : 'text-alv-dark-blue'}
			width="22px"
		/>
	</button>
{/if}
