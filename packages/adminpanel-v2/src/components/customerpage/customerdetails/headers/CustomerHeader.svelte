<script lang="ts">
	import type { TCustomer } from "$lib/types";

    export let activeCustomer: TCustomer | null

</script>


<div class="header">
    <p> {activeCustomer?.name || "Kunde"} </p>
    {#if activeCustomer}
        <p> Kundenummer: { activeCustomer.id}</p>
    {/if}
</div>


<style>
    .header {
        border: 1px solid black;
        width: 100%;
    }
</style>
