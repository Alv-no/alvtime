<script lang="ts">
    import type {TProject} from "../../../../../lib/types.ts"
    import { customers } from "../../../../../stores/CustomerStore.ts";
    export let project : TProject
    export let selectProject : Function
</script>


{#if project.id == customers.getActiveProject()?.id}
    <button 
        on:click={() => selectProject(project.id)} 
        class="border-2 border-blue-500 bg-sky-50 w-full flex"
    >
        <div class="p-4">
            <span class="text-sm  text-gray-800">{project.name}</span>
        </div>
    </button>
{:else}
    <button on:click={() => selectProject(project.id)} class="border-r border-b border-gray-300 bg-white w-full flex">
        <div class="p-4">
            <span class="text-sm  text-gray-800">{project.name}</span>
        </div>
    </button>
{/if}