<script lang="ts">

import ListHeaderText from "../customerdetails/headers/ListHeaderText.svelte";
	import Customers from "./customers/Customers.svelte";

    export let selectCustomer : Function
    const CUSTOMER_HEADER_TEXT: string[] = ["Kunde"];

    let searchQuery: string = "";
</script>

<div class="w-1/6 bg-gray-200 h-screen items-center">
    <input
        type="text"
        placeholder="Search..."
        class="w-full px-3 py-2 rounded border border-gray-300 focus:outline-none focus:border-blue-500"
        bind:value={searchQuery}
    />
        <!-- Customer list header-->
        <div class="flex justify-between border-r border-b border-slate-300 bg-slate-50">
            <ListHeaderText headerTexts={CUSTOMER_HEADER_TEXT} />
        </div>
    <Customers {selectCustomer} {searchQuery}/>
</div>
