<script lang="ts">
	import type { TProject } from "$lib/types";
    import { customers } from "../../../../../stores/CustomerStore";
    export let edit: boolean;
    export let project: TProject;
    

    $: projectStyling = 'w-full flex justify-center p-4 grid grid-cols-16 gap-4'
    
    $: nameStyling = edit
		? 'bg-transparent rounded border border-gray-300 focus:outline-none focus:border-blue-500'
		: 'bg-transparent rounded border border-transparent';
    
    $: projectNumberStyling = edit ? "w-12 rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent" 
        : "bg-transparent w-12 rounded border border-transparent"

    $: endDateStyling = edit ? "w-32 rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent" 
        : "bg-transparent w-32 rounded border border-transparent"




    let editProject = () => {
        if (edit && project) {
            customers.updateProject(project);
        }
        else {
            throw new Error("Not in valid state of program, Project must have valid hour rate and compensation rate");
        }
    }

</script>
<div class={projectStyling}>
    {#if project}
        <p class="col-span-7">
            Prosjekt: 
            <input
            type="text"
            class={nameStyling}
            bind:value={project.name}
            on:focusout={(editProject)}
            disabled={!edit}
            />
        </p>
        <p class="col-span-4">
            Prosjektnr:
            <input
            type="number"
            class={projectNumberStyling}
            bind:value={project.projectNumber}
            on:focusout={(editProject)}
            disabled={!edit}
            />
        </p>
        <p class="col-span-5">
            Sluttdato:
            <input 
            type="date"
            class={endDateStyling}
            bind:value={project.endDate}
            on:input={(editProject)}
            disabled={!edit}
            />
        </p>
    {:else}
        <p class="col-span-7">Prosjekt:</p>
        <p>Prosjektnr:</p>
    {/if}
</div>
