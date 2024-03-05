<script lang="ts">
    export let addFunction: Function;
    export let isDisabled: boolean;
    export let isActive: boolean = false;

    
    
    function onClick() {
        addFunction();
    }
    
    const BUTTON_CLASS_DEFAULT: string = "w-8 h-8 flex items-center justify-center content-center rounded";
    const BUTTON_ACTIVE: string = "border border-blue-500";
    const BUTTON_HOVER: string = "hover:border hover:border-black"
    const BUTTON_DISABLE: string = "border border-gray-400"
    const ICON_DISABLE: string = "text-gray-400"
    const ICON_ACTIVE: string = "text-blue-500"
    const ICON_DEFAULT: string = "text-alv-dark-blue"
</script>


{#if isDisabled}
    <button disabled class="{BUTTON_CLASS_DEFAULT} {BUTTON_DISABLE}">
        <iconify-icon icon={"ic:baseline-plus"} class={ICON_DISABLE} width="30px"></iconify-icon>
    </button>
{:else}
    <button class="{BUTTON_CLASS_DEFAULT} {isActive ? BUTTON_ACTIVE : BUTTON_HOVER}" on:click={() => onClick()}>
        <iconify-icon icon={"ic:baseline-plus"} class={isActive ? ICON_ACTIVE : ICON_DEFAULT} width="30px"></iconify-icon>
    </button>
{/if}