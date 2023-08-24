<script lang="ts">
	import { customers } from '../../../../../stores/CustomerStore';
	import EditButton from '../../../../generic/buttons/EditButton.svelte';

	$: customer = $customers.customers.find((c) => c.id == $customers.active.customer);

	let edit: boolean = false;

	let updateFunction = () => {
		edit = !edit;
	};

	let updateCustomerName = () => {
		customers.updateCustomer(customer!);
	};

	let updateCustomerNumber = () => {
		customers.updateCustomer(customer!);
	};
</script>

<div class="w-1/2 flex justify-between items-center">
	{#if customer}
		<p>
			Kunde: <input
				type="text"
				disabled={!edit}
				bind:value={customer.name}
				on:change={(e) => updateCustomerName()}
			/>
		</p>
		<p>
			Kundenummer: <input
				type="text"
				disabled={!edit}
				bind:value={customer.customerNumber}
				on:change={(e) => updateCustomerNumber()}
			/>
		</p>
		<EditButton {updateFunction} />
	{:else}
		<p>Kunde</p>
	{/if}
</div>
