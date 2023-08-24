<script lang="ts">
	import type { TCustomer } from '$lib/types';
	import { customers } from '../../../../stores/CustomerStore';
	import Customer from './customer/Customer.svelte';
	export let selectCustomer: Function;
	export let searchQuery = '';
	export let filterInactiveCustomers: boolean;

	// Filter function which is dependent on the searchQuery
	function filterCustomers(customers: TCustomer[], filter: boolean, searchQuery: string) {
		return customers.filter((customer) =>
			filter
				? customer.name.toLowerCase().includes(searchQuery.toLowerCase()) &&
				  ($customers.projects.filter((p) => p.customer == customer.id && !p.endDate).length ||
						customer.id == $customers.active.customer)
				: customer.name.toLowerCase().includes(searchQuery.toLowerCase())
		);
	}

	// reactivity
	$: filteredCustomers = filterCustomers(
		$customers.customers,
		filterInactiveCustomers,
		searchQuery
	);
</script>

{#each filteredCustomers as customer}
	<Customer customerId={customer.id} {selectCustomer} />
{/each}
