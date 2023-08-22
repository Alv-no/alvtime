<script>
	import { getCustomers } from '$lib/service/customers';
	import { onMount } from 'svelte';

	let customers = [];

	onMount(async () => {
		customers = getCustomers({ token: '5801gj90-jf39-5j30-fjk3-480fj39kl409' });
	}).catch((error) => {
		console.log(error);
		return [];
	});
</script>

<div>
	{JSON.stringify(customers)}
</div>
