<script lang="ts">
	import type { TProject } from "$lib/types";
	import { customers } from "../../../../../stores/CustomerStore";
    $: project = $customers.customers.find((c) => c.id == $customers.active.customer)?.projects.find((p) => p.id == $customers.active.project)
</script>


<div class="header">
    <p> {project?.name || "Prosjekt"} </p>
    {#if project}
        <p> Prosjektnummer: { project.id}</p>
    {/if}
</div>


<style>
    .header {
        border: 1px solid black;
        width: 100%;
        display: flex;
        justify-content: space-between;
    }
</style>