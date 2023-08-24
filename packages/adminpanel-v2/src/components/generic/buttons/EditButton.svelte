<script lang="ts">
    export let updateFunction: Function;
    export let isDisabled: boolean;
    export let buttonClassDefault: string;
    let isActive: boolean = false;

    function onClick() {
        isActive = !isActive;
        updateFunction();
    }

</script>


{#if isDisabled}
    <button disabled class="{buttonClassDefault} border border-gray-400" on:click={() => onClick()}>
        <iconify-icon icon={"fluent:edit-20-regular"} class="text-gray-400" width="22px"></iconify-icon>
    </button>
{:else}
    <button class="{buttonClassDefault} {isActive ? "border border-blue-500" : "hover:border hover:border-black"}" on:click={() => onClick()}>
        <iconify-icon icon={"fluent:edit-20-regular"} class="{isActive ? "text-blue-500" : "text-alv-dark-blue"}" width="22px"></iconify-icon>
    </button>
{/if}