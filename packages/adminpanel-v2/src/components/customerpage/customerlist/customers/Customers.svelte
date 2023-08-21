<script lang="ts">
	import type { TCustomer } from "$lib/types";
	import { customers } from "../../../../stores/CustomerStore";
    import Customer from "./customer/Customer.svelte";
    export let selectCustomer : Function
</script>

<div class="left-component">
    <ul class="customer-list">
        {#each $customers.customers as customer}
            <Customer {customer} {selectCustomer}/>
        {/each}
    </ul>
</div>

<style>
    .left-component {
      width: 100%; /* 2 out of 12 columns */
      
    }
    .customer-list {
      list-style: none;
      padding: 0;
    }
</style>