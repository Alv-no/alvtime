<script lang="ts">
    import type {TProject} from "../../../../../lib/types.ts"
    import { customers } from "../../../../../stores/CustomerStore.ts";
    export let project : TProject
    export let selectProject : Function

    $: buttonStyling = project.Id == $customers.active.project ? "border-2 border-blue-500 bg-sky-50 w-full flex" : "border-r border-b border-gray-300 bg-white w-full flex"
</script>


<button on:click={() => selectProject(project.Id)}
    class={buttonStyling}>
    <div class="p-4">
        <span class="text-sm  text-gray-800">{project.Name}</span>
    </div>
</button>