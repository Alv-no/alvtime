<script script lang="ts">
	import 'iconify-icon';
	import CustomerDetails from '../../components/customerpage/customerdetails/CustomerDetails.svelte';
	import CustomerList from '../../components/customerpage/customerlist/CustomerList.svelte';
	import { customers } from '../../stores/CustomerStore';
	import { onMount } from 'svelte';

	onMount(() => customers.loadCustomers('5801gj90-jf39-5j30-fjk3-480fj39kl409'));

	function selectCustomer(customerId: number) {
		customers.setCustomer(customerId, '5801gj90-jf39-5j30-fjk3-480fj39kl409');
	}
	function selectProject(projectId: number) {
		customers.setProject(projectId);
	}
</script>

<div class="w-full flex">
	<CustomerList {selectCustomer} />
	<CustomerDetails {selectProject} />
</div>
