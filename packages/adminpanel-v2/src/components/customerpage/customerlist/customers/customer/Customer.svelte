<script lang="ts">
	import { customers } from "../../../../../stores/CustomerStore.ts";
    export let customerId: number
    export let selectCustomer: Function



    $: customer = $customers.customers.find((c) => c.Id == customerId)
    $: customerStyling = customer?.Id == $customers.active.customer ? "border-2 border-blue-500 bg-sky-50 w-full flex" : inactive ? "border-r border-b border-gray-300 bg-gray-100 w-full flex" : "border-r border-b border-gray-300 bg-white w-full flex"
    $: inactive = $customers.projects.filter((p) => p.Customer == customer?.Id && !p.EndDate).length ? false : true
</script>


{#if customer}
    <button on:click={() => selectCustomer(customerId)} class={customerStyling}>
        <div class="p-4">
            <span class="text-sm text-gray-800">{customer.Name}</span>
        </div>
    </button>
{/if}
