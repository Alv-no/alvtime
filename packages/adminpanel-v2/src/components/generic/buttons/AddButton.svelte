<script lang="ts">
    export let addFunction: Function;
    export let isDisabled: boolean;
    export let buttonClassDefault: string;
    let isActive: boolean = false;

    function onClick() {
        isActive = !isActive;
        addFunction();
    }
    
    const BUTTON_ACTIVE: string = "border border-blue-500";
    const BUTTON_HOVER: string = "hover:border hover:border-black"
    const BUTTON_DISABLE: string = "border border-gray-400"
    const ICON_DISABLE: string = "text-gray-400"
    const ICON_ACTIVE: string = "text-blue-500"
    const ICON_DEFAULT: string = "text-alv-dark-blue"
</script>


{#if isDisabled}
    <button disabled class="{buttonClassDefault} {BUTTON_DISABLE}">
        <iconify-icon icon={"ic:baseline-plus"} class={ICON_DISABLE} width="30px"></iconify-icon>
    </button>
{:else}
    <button class="{buttonClassDefault} {isActive ? BUTTON_ACTIVE : BUTTON_HOVER}" on:click={() => onClick()}>
        <iconify-icon icon={"ic:baseline-plus"} class={isActive ? ICON_ACTIVE : ICON_DEFAULT} width="30px"></iconify-icon>
    </button>
{/if}