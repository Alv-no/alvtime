<script lang="ts">
    import type {TCustomer} from "../../../../../lib/types.ts"
	import { customers } from "../../../../../stores/CustomerStore.ts";
    export let customer : TCustomer
    export let selectCustomer : Function
</script>


{#if customer.id == customers.getActiveCustomer()?.id}
    <button 
        on:click={() => selectCustomer(customer.id)} 
        class="border-2 border-blue-500 bg-sky-50 w-full flex"
    >
        <div class="p-4">
            <span class="text-sm  text-gray-800">{customer.name}</span>
        </div>
    </button>
{:else}
    <button on:click={() => selectCustomer(customer.id)} class="border-r border-b border-gray-300 bg-white w-full flex">
        <div class="p-4">
            <span class="text-sm  text-gray-800">{customer.name}</span>
        </div>
    </button>
{/if}