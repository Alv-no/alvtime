<script lang="ts">
	import { customers } from '../../../../../stores/CustomerStore.ts';
	export let customerId: number;
	export let selectCustomer: Function;

	$: customer = $customers.customers.find((c) => c.id == customerId);
	$: customerStyling =
		customer?.id == $customers.active.customer
			? 'border-2 border-blue-500 bg-sky-50 w-full flex'
			: inactive
			? 'border-r border-b border-gray-300 bg-gray-100 w-full flex'
			: 'border-r border-b border-gray-300 bg-white w-full flex';
	$: inactive = $customers.projects.filter((p) => p.customer == customer?.id && !p.endDate).length
		? false
		: true;
</script>

{#if customer}
	<button on:click={() => selectCustomer(customerId)} class={customerStyling}>
		<div class="p-4">
			<span class="text-sm text-gray-800">{customer.name}</span>
		</div>
	</button>
{/if}
