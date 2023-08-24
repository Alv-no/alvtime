<script lang="ts">
	import type { TCustomer } from '$lib/types';
	import { customers } from '../../../../../stores/CustomerStore';

	let newCustomer: TCustomer;

	const customer: TCustomer = {
		id: Date.now(),
		name: '',
		invoiceAddress: 'ExampleInvoiceAddress',
		contactPerson: 'ExampleContactPerson',
		contactEmail: 'ExampleContactEmail',
		contactPhone: 'ExampleContactPhone',
		customerNumber: 0,
		project: []
	};

	customers.addNewCustomer(customer);
	customers.setCustomer(customer.id, 'test');
	$: reactiveCustomer = customer;

	const addStyling: string =
		'rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent';
</script>

<p>
	Kunde:
	<input type="text" bind:value={reactiveCustomer.name} class={addStyling} />
</p>
<p>
	Kundenummer:
	<input type="text" bind:value={reactiveCustomer.customerNumber} class={addStyling} />
</p>
