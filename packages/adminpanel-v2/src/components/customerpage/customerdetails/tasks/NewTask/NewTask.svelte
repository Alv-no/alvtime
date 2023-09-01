<script lang="ts">
	import { HourRate } from "$lib/mock/customers.ts";
    import type { TCompensationRate, THourRate, TProject, TTask } from "../../../../../lib/types.ts"
    import { customers } from "../../../../../stores/CustomerStore.ts";
    let name: string|undefined = undefined;
    let hourRate: number|undefined = undefined;
    let compensationRate: number|undefined = undefined;
    
    const addTask = () => {

        let taskId: number =  Date.now();
        let activeProject: TProject | undefined = customers.getActiveProject();
        if (activeProject === undefined) {
            throw new Error("Not in valid state of program, can't add a task without an active project");
        } else if (!(name && hourRate && compensationRate)) {
            throw new Error("Not all fields have valid input");

        } else {

            
            let newTask: TTask = {
                id: taskId,
                name: name, 
                project: activeProject.id,
                compensationRate: [],
                hourRate: [],
                description: "",
            };
            customers.addNewTask(newTask, hourRate, compensationRate);
            customers.setTask(taskId);
            clearInputFields()
        }
    }

    const clearInputFields = () => {
        name = undefined;
        hourRate = undefined;
        compensationRate = undefined;
    }
</script>

<button 
    class="border-r border-b border-gray-300 bg-white w-full flex justify-center p-4 grid grid-cols-16 gap-4">

    <input
        type="text"
        class="w-full col-span-4 rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent"
        bind:value={name} />
    <span class="col-span-1"></span>
    <input
        type="number"
        min="0"
            class="col-span-2 rounded border border-gray-300 focus:outline-none focus:border-blue-500"
    bind:value={hourRate} />
    <span class="col-span-1"></span>
    <input
        type="number"
        min="0"
        class="col-span-2 rounded border border-gray-300 focus:outline-none focus:border-blue-500"
        bind:value={compensationRate} />
    <span class="col-span-5"></span>
    <button 
        class="text-sm col-span-1 text-gray-800 flex justify-end align-center leading-none"
        on:click={addTask}>       
        <span class="col-span-1"></span>
        <div class="rounded border-2 border-transparent justify-center align-center">
            <iconify-icon width="2em" icon="icon-park:plus"></iconify-icon>
        </div>
    </button>
</button>