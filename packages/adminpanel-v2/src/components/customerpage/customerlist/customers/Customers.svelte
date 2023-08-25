<script lang="ts">
	import { sortOptions, type TCustomer } from "$lib/types";
	import { customers } from "../../../../stores/CustomerStore";
    import Customer from "./customer/Customer.svelte";
    export let selectCustomer : Function
    export let searchQuery = '';
    export let filterInactiveCustomers: boolean

    export let selected :  sortOptions
  
  // Filter function which is dependent on the searchQuery
  function filterCustomers(customers: TCustomer[], filter: boolean, searchQuery: string) {
    return customers.filter((customer) => filter ?
      (customer.Name.toLowerCase().includes(searchQuery.toLowerCase()) &&
      ($customers.projects.filter((p) => p.Customer == customer.Id && !p.EndDate).length || customer.Id == $customers.active.customer))
      : customer.Name.toLowerCase().includes(searchQuery.toLowerCase())
    );
  }

  function sortCustomers(customers: TCustomer[], selected: sortOptions) {
    if (selected == sortOptions.Alfabetisk)
    {
      return customers.sort((customer1, customer2) =>
        (customer1.Name.toLowerCase() > customer2.Name.toLowerCase() ? -1 : 1)
      );
    }
    else
    {
      return customers.sort((customer1, customer2) =>
        (customer1.Name.toLowerCase() > customer2.Name.toLowerCase() ? 1 : -1)
      );
    }
  }

  function filterAndSortCustomers(customers: TCustomer[], searchQuery: string) {
    var filteredCustomers = filterCustomers(customers, searchQuery);
    if (selected) 
    {
      return sortCustomers(filteredCustomers, selected);
    }
    else 
    {
      return filteredCustomers;
    }
  }
  
  // reactivity
  $: filteredCustomers = filterCustomers($customers.customers, filterInactiveCustomers, searchQuery)
  $ : processedCustomers = filterAndSortCustomers($customers.customers, searchQuery)

</script>


{#each filteredCustomers as customer}
    <Customer customerId={customer.Id} {selectCustomer}/>
{/each}

<style>

</style>