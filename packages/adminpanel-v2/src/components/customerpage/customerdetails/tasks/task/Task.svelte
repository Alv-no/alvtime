<script lang="ts">
    import type { TTask } from "../../../../../lib/types.ts"
    import { customers } from "../../../../../stores/CustomerStore.ts";
	import EditButton from "../../../../generic/buttons/EditButton.svelte";
    export let task : TTask
    let isEditingTask: boolean = false;

    $: inactive = task.EndDate ? true : false
    $: hourRate = $customers.hourRate.find(h => h.TaskId == task.Id)!
    $: compensationRate = $customers.compensationRate.find(c => c.TaskId == task.Id)!

    $: taskStyling = inactive ? "border-r border-b border-gray-300 bg-gray-100 w-full flex justify-center p-4 grid grid-cols-12 gap-4" 
        : "border-r border-b border-gray-300 bg-white w-full flex justify-center p-4 grid grid-cols-12 gap-4"

    $: nameStyling = isEditingTask ? "w-full col-span-3 rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent" 
        : "w-full col-span-3 bg-transparent rounded border border-transparent"

    $: hourRateStyling = isEditingTask ? "col-span-2 bg-transparent rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent" 
        : "col-span-2 bg-transparent rounded border border-transparent"

    $: compensationRateStyling = isEditingTask ? "col-span-2 bg-transparent rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent" 
        : "col-span-2 bg-transparent rounded border border-transparent"
        
    $: editButtonStyling = isEditingTask ? "rounded border-2 justify-center align-center border-blue-500" 
        : "rounded border-2 justify-center align-center border-transparent"

    let editTask = () => {
        isEditingTask = !isEditingTask;
        if (isEditingTask) {
            customers.setTask(task.Id);
            customers.updateTask(task, hourRate, compensationRate);
        }
    }
    
</script>

<!-- LEGGE INN NY AKTIVTET!!! --> 
<!-- Currently editing --> 

    <button 
        class={taskStyling}>
        <input type="text" disabled={!isEditingTask} class={nameStyling} bind:value={task.Name} />
        <input
            type="text"
            class={hourRateStyling}
            bind:value={hourRate.Rate}
            disabled={!isEditingTask}
        />
        
     
        <span class="col-span-2"></span>
        <input
            type="text"
            class={compensationRateStyling}
            bind:value={compensationRate.Value}
            disabled={!isEditingTask} 
        />
        <span class="col-span-2"></span>

        <EditButton updateFunction={editTask} isDisabled={false} />
    </button>
