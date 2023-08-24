<script lang="ts">
	import type { TCustomer } from "$lib/types";
	import { customers } from "../../../../stores/CustomerStore";
    import Customer from "./customer/Customer.svelte";
    export let selectCustomer : Function
    export let searchQuery = '';

  
  // Filter function which is dependent on the searchQuery
  function filterCustomers(customers: TCustomer[], searchQuery: string) {
    return customers.filter(customer =>
      customer.Name.toLowerCase().includes(searchQuery.toLowerCase())
    );
  }
  
  // reactivity
  $: filteredCustomers = filterCustomers($customers.customers, searchQuery)

</script>


{#each filteredCustomers as customer}
    <Customer {customer} {selectCustomer}/>
{/each}

<style>

</style>