<script lang="ts">
	import type { TCustomer } from "$lib/types";
    import { customers } from "../../../../../stores/CustomerStore";
	import AddButton from "../../../../generic/buttons/AddButton.svelte";
    import EditButton from "../../../../generic/buttons/EditButton.svelte";
	import CustomerInfoAdd from "./customerInfoAdd.svelte";
	import CustomerInfoEdit from "./customerInfoEdit.svelte";
    
    const BUTTON_CLASS_DEFAULT: string = "w-8 h-8 flex items-center justify-center content-center rounded";
    
    $: customer = $customers.customers.find((c: TCustomer) => c.Id == $customers.active.customer)

    let edit: boolean = false
    let add: boolean = false

    let updateFunction = () => {
        if (edit && customer === undefined) {
            throw new Error('Not valid state for program...');
        }
        if (edit && customer) {
            customers.updateCustomer(customer)
        }
        edit = !edit;
    }
    
    let addFunction = () => {
        if (add && customer === undefined) {
            throw new Error('Not valid state for program...');
        }
        if (add && customer) {
            customers.updateCustomer(customer)
        }
        add = !add;
    };

</script>

<div class="w-1/2 flex justify-between items-center">
    {#if add}
        <CustomerInfoAdd />
    {:else if customer}
        <CustomerInfoEdit {edit} />
    {:else}
        <p> Kunde </p>
    {/if}

    {#if customer!==undefined}
        <EditButton buttonClassDefault={BUTTON_CLASS_DEFAULT} {updateFunction} isDisabled={add} />
        {/if}
    <AddButton buttonClassDefault={BUTTON_CLASS_DEFAULT}  {addFunction} isDisabled={edit} />
    
</div>