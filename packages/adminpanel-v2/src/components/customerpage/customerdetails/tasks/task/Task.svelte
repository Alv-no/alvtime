<script lang="ts">
    import type { TTask } from "../../../../../lib/types.ts"
    import { customers } from "../../../../../stores/CustomerStore.ts";
	import EditButton from "../../../../generic/buttons/EditButton.svelte";
    export let task: TTask;
    let isEditingTask: boolean = false;

    $: inactive = task.endDate ? true : false
    let hourRate = $customers.hourRate.find(h => h.taskId == task.id && !h.endDate)!.rate
    let compensationRate = $customers.compensationRate.find(c => c.taskId == task.id && !c.endDate)!.value

	$: taskStyling = inactive
		? 'border-r border-b border-gray-300 bg-gray-100 w-full flex justify-center p-4 grid grid-cols-12 gap-4'
		: 'border-r border-b border-gray-300 bg-white w-full flex justify-center p-4 grid grid-cols-12 gap-4';

	$: nameStyling = isEditingTask
		? 'w-full col-span-3 rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent'
		: 'w-full col-span-3 bg-transparent rounded border border-transparent';

    $: hourRateStyling = isEditingTask ? "col-span-2 rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent" 
        : "col-span-2 bg-transparent rounded border border-transparent"

    $: compensationRateStyling = isEditingTask ? "col-span-2 rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent" 
        : "col-span-2 bg-transparent rounded border border-transparent"

    let editTask = () => {
        if (isEditingTask) {
            if (hourRate && compensationRate) {
                customers.setTask(task.id);
                customers.updateTask(task, hourRate!, compensationRate!);
                isEditingTask = !isEditingTask
            }
            else {
                throw new Error("Not in valid state of program, task must have valid hour rate and compensation rate");
            }
        }
        else {
            isEditingTask = !isEditingTask;
        }
    }
    
</script>


<button 
    class={taskStyling}>
    <input type="text" disabled={!isEditingTask} class={nameStyling} bind:value={task.name} />
    <input
        type="number"
        class={hourRateStyling}
        bind:value={hourRate}
        disabled={!isEditingTask}
    />
    
    
    <span class="col-span-2"></span>
    <input
        type="number"
        class={compensationRateStyling}
        bind:value={compensationRate}
        disabled={!isEditingTask} 
    />
    <span class="col-span-2"></span>
	<EditButton updateFunction={editTask} isDisabled={false} />
</button>
