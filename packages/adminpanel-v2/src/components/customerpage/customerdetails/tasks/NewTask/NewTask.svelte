<script lang="ts">
	import { HourRate } from "$lib/mock/customers.ts";
    import type { TTask } from "../../../../../lib/types.ts"
    import { customers } from "../../../../../stores/CustomerStore.ts";
    let name: string
    let price: number
    let overtimeFactor: number
    
    /* let addTask = () => {
        let newTask: TTask = {
            Id: Date.now(),
            Name: name, 
            CompensationRate: //todo
            HourRate: //todo
            //price: price, 
            //overtimeFactor: overtimeFactor
        };

        customers.addNewTask(newTask);
        name = "";
        
    } */
</script>

<button 
    class="border-r border-b border-gray-300 bg-white w-full flex justify-center p-4 grid grid-cols-12 gap-4">

    <input
        type="text"
        class="w-full col-span-3 rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent"
        bind:value={name} />

    <input
        type="text"
        class="col-span-2 rounded border border-gray-300 focus:outline-none focus:border-blue-500"
    bind:value={price} />

    <span class="col-span-2"></span>
    <input
        type="text"
        class="w-1/3 col-span-4 rounded border border-gray-300 focus:outline-none focus:border-blue-500"
        bind:value={overtimeFactor} />
    <button 
        class="text-sm col-span-1 text-gray-800 flex justify-end align-center leading-none"
        on:click={/* addTask */() => {}}>       

        <div class="rounded border-2 border-transparent justify-center align-center">
            <iconify-icon width="2em" icon="icon-park:plus"></iconify-icon>
        </div>
    </button>
</button>