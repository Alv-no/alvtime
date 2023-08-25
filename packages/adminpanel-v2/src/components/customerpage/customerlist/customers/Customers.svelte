<script lang="ts">
	import type { TCustomer } from "$lib/types";
	import { customers } from "../../../../stores/CustomerStore";
  import Customer from "./customer/Customer.svelte";
  export let selectCustomer : Function
  export let searchQuery = '';
  export let filterInactiveCustomers: boolean

  
  // Filter function which is dependent on the searchQuery
  function filterCustomers(customers: TCustomer[], filter: boolean, searchQuery: string) {
    return customers.filter((customer) => filter ?
      (customer.Name.toLowerCase().includes(searchQuery.toLowerCase()) &&
      ($customers.projects.filter((p) => p.Customer == customer.Id && !p.EndDate).length || customer.Id == $customers.active.customer))
      : customer.Name.toLowerCase().includes(searchQuery.toLowerCase())
    );
  }
  
  // reactivity
  $: filteredCustomers = filterCustomers($customers.customers, filterInactiveCustomers, searchQuery)

</script>


{#each filteredCustomers as customer}
    <Customer customerId={customer.Id} {selectCustomer}/>
{/each}
