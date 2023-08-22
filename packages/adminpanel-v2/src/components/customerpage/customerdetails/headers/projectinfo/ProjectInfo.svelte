<script lang="ts">
	import type { TProject } from "$lib/types";
    export let activeProject: TProject | null
</script>


<div class="header">
    <p> {activeProject?.name || "Prosjekt"} </p>
    {#if activeProject}
        <p> Prosjektnummer: { activeProject.id}</p>
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