<script lang="ts">
    import { customers } from "../../../../../stores/CustomerStore";
    export let edit: boolean;
    

    $: project = $customers.projects.find((p) => p.Id == $customers.active.project)
    $: editStyling = edit 
        ? "rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent" 
        : "bg-transparent rounded border border-transparent"

</script>

{#if project}
    <p>
        Prosjekt: 
        <input type="text" bind:value={project.Name} class={editStyling} disabled={!edit}/>
    </p>
    <p>
        Prosjektnr:
        <input type="text" bind:value={project.ProjectNumber} class={editStyling} disabled={!edit}/>
    </p>
{:else}
    <p>Prosjekt:</p>
    <p>Prosjektnr:</p>
{/if}
