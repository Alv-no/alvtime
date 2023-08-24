<script lang="ts">
    import { onMount } from 'svelte'
	import type { TCustomer } from "$lib/types";
    import { customers } from "../../../../../stores/CustomerStore";
    
    let newCustomer: TCustomer;
    console.log("Mounting")
    const customer: TCustomer = {
        Id: Date.now(),
        Name: "",
        InvoiceAddress: "ExampleInvoiceAddress",
        ContactPerson: "ExampleContactPerson",
        ContactEmail: "ExampleContactEmail",
        ContactPhone: "ExampleContactPhone",
        CustomerNumber: 0,
        Project: [],
    };
    
    
    customers.addNewCustomer(customer);
    customers.setCustomer(customer.Id);
    $: reactiveCustomer = customer;
    
    const addStyling: string ="rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent"

</script>

<p>
    Kunde: 
    <input type="text" bind:value={reactiveCustomer.Name} class={addStyling}/>
</p>
<p>
    Kundenummer: 
    <input type="text" bind:value={reactiveCustomer.CustomerNumber} class={addStyling}/>
</p>
