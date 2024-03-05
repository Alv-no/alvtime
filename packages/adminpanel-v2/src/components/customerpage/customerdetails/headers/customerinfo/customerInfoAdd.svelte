<script lang="ts">
	export let newCustomerName: string|undefined
	export let newCustomerNumber: number|undefined
	export let updateFunction: Function

	const addStyling: string =
		'rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent';
</script>

<p>
	Kunde:
	<input type="text" bind:value={newCustomerName} on:change={() => updateFunction(newCustomerName, newCustomerNumber)} class={addStyling} />
</p>
<p>
	Kundenummer:
	<input type="number" bind:value={newCustomerNumber} on:change={() => updateFunction(newCustomerName, newCustomerNumber)} class={addStyling} />
</p>
