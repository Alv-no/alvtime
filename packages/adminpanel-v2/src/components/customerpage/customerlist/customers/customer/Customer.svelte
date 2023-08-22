<script lang="ts">
    import type {TCustomer} from "../../../../../lib/types.ts"
    export let customer : TCustomer
    export let selectCustomer : Function
</script>


<button on:click={() => selectCustomer(customer.id)} class="border-r border-b border-gray-300 bg-white w-full flex">
    <div class="p-4">
        <span class="text-sm  text-gray-800">{customer.name}</span>
    </div>
</button>
