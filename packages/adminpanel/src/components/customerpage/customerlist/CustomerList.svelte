<script lang="ts">
	import Customers from "./customers/Customers.svelte";
    import SearchBar from "./searchbar/SearchBar.svelte";
    import type { TCustomer } from "$lib/types";

    export let customers : TCustomer[]
    export let selectCustomer : Function


</script>
<div class="customerlist">
    <SearchBar />
    <Customers {customers} {selectCustomer} />
</div>

<style>
    .customerlist {
        width: 16.67%; /* 2 out of 12 columns */
        background-color: #f0f0f0;
        padding: 10px;
        box-sizing: border-box;
        border-right: 1px solid #ccc;
        height: 100vh; /* Full viewport height */
    }
</style>
