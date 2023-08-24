<script lang="ts">
    
    import { customers } from "../../../../../stores/CustomerStore";
    export let edit: boolean;
    
    $: customer = $customers.customers.find((c) => c.Id == $customers.active.customer)
    $: editStyling = edit 
        ? "rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent" 
        : "bg-transparent rounded border border-transparent"

</script>
{#if customer}
    <p>Kunde: 
        <input type="text" bind:value={customer.Name} class={editStyling} disabled={!edit}/>
    </p>
    <p>
        Kundenummer: 
        <input type="text" bind:value={customer.CustomerNumber} class={editStyling} disabled={!edit}/>
    </p>
{:else}
    <p>Kunde</p>
{/if}