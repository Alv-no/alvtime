<script lang="ts">
    import type { TTask } from "../../../../../lib/types.ts"
    import { customers } from "../../../../../stores/CustomerStore.ts";
    export let task : TTask
    let isEditingTask: boolean = false;

    $: inactive = task.EndDate ? true : false

    $: taskStyling = inactive ? "border-r border-b border-gray-300 bg-gray-100 w-full flex justify-center p-4 grid grid-cols-12 gap-4" : "border-r border-b border-gray-300 bg-white w-full flex justify-center p-4 grid grid-cols-12 gap-4"
    $: nameStyling = isEditingTask ? "w-full col-span-3 rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent" : "w-full col-span-3 bg-transparent rounded border border-transparent"
    $: editButtonStyling = isEditingTask ? "rounded border-2 justify-center align-center border-blue-500" : "rounded border-2 justify-center align-center border-transparent"

    let editTask = () => {
        isEditingTask = !isEditingTask;
        customers.setTask(task.Id);
        customers.updateTask(task);
    }
    

</script>

<!-- LEGGE INN NY AKTIVTET!!! --> 
<!-- Currently editing --> 

    <button 
        class={taskStyling}>
        <input type="text" class={nameStyling} bind:value={task.Name} />
        <!-- <input
            type="text"
            class="col-span-2 bg-transparent rounded border border-transparent"
            bind:value={activity.price} 
            disabled />

        <span class="col-span-2"></span>
        <input
            type="text"
            class="w-1/3 col-span-4 bg-transparent rounded border border-transparent"
            bind:value={activity.overtimeFactor}
            disabled  /> -->
        <button 
            class="text-sm col-span-1 text-gray-800 flex justify-end align-center leading-none"
            on:click={editTask}>       

            <div class={editButtonStyling}>
                <iconify-icon class="text-blue-500" width="1.5em" icon="pajamas:pencil"></iconify-icon>
            </div>
        </button>
    </button>
