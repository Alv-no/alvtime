<script lang="ts">
	import { clickOutside } from '$lib/functions/clickOutside';
	import type { TCustomer } from '$lib/types';
	import { customers } from '../../../../../stores/CustomerStore';
	import AddButton from '../../../../generic/buttons/AddButton.svelte';
	import EditButton from '../../../../generic/buttons/EditButton.svelte';
	import CustomerInfoAdd from './customerInfoAdd.svelte';
	import CustomerInfoEdit from './customerInfoEdit.svelte';

	$: customer = $customers.customers.find((c: TCustomer) => c.id == $customers.active.customer);

	let edit: boolean = false;
	let add: boolean = false;

	let updateFunction = () => {
		if (edit && customer === undefined) {
			throw new Error('Not valid state for program...');
		}
		if (edit && customer) {
			customers.updateCustomer(customer);
		}
		edit = !edit;
	};

	let addFunction = () => {
		if (add && customer === undefined) {
			throw new Error('Not valid state for program...');
		}
		if (add && customer) {
			customers.updateCustomer(customer);
		}
		add = !add;
	};

	const handleClickOutside = () => {
		edit = false
	}

</script>

<div class="w-2/5 flex justify-between items-center"
	use:clickOutside
	on:message={handleClickOutside}
	>
	{#if add}
		<CustomerInfoAdd />
	{:else if customer}
		<CustomerInfoEdit {edit} />
	{:else}
		<p>Kunde</p>
	{/if}

	{#if customer !== undefined}
		<EditButton {updateFunction} isActive={edit} isDisabled={add} />
	{/if}
	<AddButton {addFunction} isDisabled={edit} />
</div>
