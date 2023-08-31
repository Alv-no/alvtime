<script lang="ts">
	import { clickOutside } from "$lib/functions/clickOutside.js";
    import type { TTask } from "../../../../../lib/types.ts"
    import { customers } from "../../../../../stores/CustomerStore.ts";
	import EditButton from "../../../../generic/buttons/EditButton.svelte";
    export let task: TTask;
    let isEditingTask: boolean = false;

    $: inactive = task.endDate ? true : false
    let hourRate = $customers.hourRate.find(h => h.taskId == task.id && !h.endDate)!.rate
    let compensationRate = $customers.compensationRate.find(c => c.taskId == task.id && !c.endDate)!.value
    let endDate = task.endDate ? task.endDate.toISOString().split('T')[0] : undefined
	$: taskStyling = inactive
		? 'border-r border-b border-gray-300 bg-gray-100 w-full flex justify-center p-4 grid grid-cols-16 gap-4'
		: 'border-r border-b border-gray-300 bg-white w-full flex justify-center p-4 grid grid-cols-16 gap-4';

	$: nameStyling = isEditingTask
		? 'w-full col-span-4 bg-transparent rounded border border-gray-300 focus:outline-none focus:border-blue-500'
		: 'w-full col-span-4 bg-transparent rounded border border-transparent';

    $: hourRateStyling = isEditingTask ? "col-span-2 bg-transparent rounded border border-gray-300 focus:outline-none focus:border-blue-500" 
        : "col-span-2 bg-transparent rounded border border-transparent"

    $: compensationRateStyling = isEditingTask ? "col-span-2 bg-transparent rounded border border-gray-300 focus:outline-none focus:border-blue-500" 
        : "col-span-2 bg-transparent rounded border border-transparent"

    $: endDateStyling = isEditingTask ? "col-span-3 bg-transparent rounded border border-gray-300 focus:outline-none focus:border-blue-500" 
        : "col-span-3 bg-transparent rounded border border-transparent"

    let editTask = () => {
        console.log(isEditingTask)
        if (isEditingTask) {
            console.log("edit task")
            if (hourRate && compensationRate) {
                customers.setTask(task.id);
                customers.updateTask(task, hourRate!, compensationRate!, endDate);
            }
            else {
                throw new Error("Not in valid state of program, task must have valid hour rate and compensation rate");
            }
        }
    }

    const setEdit = () => {
        console.log("set task", isEditingTask)
        if (isEditingTask) console.log("click edit task"); editTask()
        isEditingTask = !isEditingTask
    }

    const handleClickOutside = () => {
        isEditingTask = false
    }

</script>


<div 
    class={taskStyling}
    use:clickOutside
    on:message={handleClickOutside}
    >
    <input
        type="text"
        disabled={!isEditingTask}
        class={nameStyling}
        on:focusout={(editTask)}
        bind:value={task.name}
    />
    <span class="col-span-1"></span>
    <input
        type="number"
        class={hourRateStyling}
        bind:value={hourRate}
        on:focusout={(editTask)}
        disabled={!isEditingTask}
    />
    
    <span class="col-span-1"></span>
    <input
        type="number"
        class={compensationRateStyling}
        bind:value={compensationRate}
        on:focusout={(editTask)}
        disabled={!isEditingTask} 
    />
    <span class="col-span-2"></span>
    <input 
        type="date"
        class={endDateStyling}
        bind:value={endDate}
        on:focusout={(editTask)}
        disabled={!isEditingTask}
    />
	<EditButton updateFunction={setEdit} isActive={isEditingTask} isDisabled={false} />
</div>
