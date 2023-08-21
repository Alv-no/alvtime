<script lang="ts">
	import type { TCustomer } from "$lib/types";
	import { customers } from "../../../../../stores/CustomerStore";
    $: customer = $customers.customers.find((c) => c.id == $customers.active.customer)
</script>


<div class="header">
    <p> {customer?.name || "Kunde"} </p>
    {#if customer}
        <p> Kundenummer: { customer.id}</p>
    {/if}
</div>


<style>
    .header {
        border: 1px solid black;
        width: 100%;
        display: flex;
        justify-content: space-between;
    }
</style>