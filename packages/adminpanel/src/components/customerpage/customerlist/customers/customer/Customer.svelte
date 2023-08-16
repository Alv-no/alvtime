<script lang="ts">
    import type {TCustomer} from "../../../../../lib/types.ts"
    export let customer : TCustomer
    export let selectCustomer : Function
</script>


<li class="customer-item">
    <iconify-icon icon="mdi:person"></iconify-icon>
    <button on:click={() => selectCustomer(customer.id)}>
        {customer.name}
    </button>
</li>

<style>
    .customer-item {
        gap: 10px;
    }
</style>
