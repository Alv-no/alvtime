<script lang="ts">
    import type {TActivity} from "../../../../../lib/types.ts"
    import { customers } from "../../../../../stores/CustomerStore.ts";
    export let activity : TActivity
    let isEditingActivity: boolean = false;

    const IS_UPDATE: boolean = true;

    let editActivity = () => {
        isEditingActivity = !isEditingActivity;
        customers.setActivity(activity.id);
        customers.updateActivity(activity, IS_UPDATE);
    }
    

</script>

<!-- LEGGE INN NY AKTIVTET!!! --> 
<!-- Currently editing --> 

    <button 
        class="border-r border-b border-gray-300 bg-white w-full flex justify-center p-4 grid grid-cols-12 gap-4">

        <!-- Active - EDITING -->
        {#if isEditingActivity}
            <input
                type="text"
                class="w-full col-span-3 rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent"
                bind:value={activity.name} />

            <input
                type="text"
                class="col-span-2 rounded border border-gray-300 focus:outline-none focus:border-blue-500"
                bind:value={activity.price} />

            <span class="col-span-2"></span>
            <input
                type="text"
                class="w-1/3 col-span-4 rounded border border-gray-300 focus:outline-none focus:border-blue-500"
                bind:value={activity.overtimeFactor} />
            <button 
                class="text-sm col-span-1 text-gray-800 flex justify-end align-center leading-none"
                on:click={editActivity}>       

                <div class="rounded border-2 justify-center align-center border-blue-500">
                    <iconify-icon class="text-blue-500" width="1.5em" icon="pajamas:pencil"></iconify-icon>
                </div>
            </button>
        <!-- Default - NOT EDITING -->
        {:else}
        <input
            type="text"
            class="w-full col-span-3 bg-transparent rounded border border-transparent"
            bind:value={activity.name} 
            disabled />

        <input
            type="text"
            class="col-span-2 bg-transparent rounded border border-transparent"
            bind:value={activity.price} 
            disabled />

        <span class="col-span-2"></span>
        <input
            type="text"
            class="w-1/3 col-span-4 bg-transparent rounded border border-transparent"
            bind:value={activity.overtimeFactor}
            disabled  />
            <button 
                class="text-sm col-span-1 text-gray-800 flex justify-end align-center leading-none"
                on:click={editActivity}>       

                <div class="rounded border-2 justify-center align-center border-transparent">
                    <iconify-icon  width="1.5em" icon="pajamas:pencil"></iconify-icon>
                </div>
        </button>
        {/if}

    </button>
