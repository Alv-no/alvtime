<!--
    This Svelte component renders a list of header texts with sorting icons and a filter option.

    Props:
    - headerTexts (array of strings): An array of header text strings to be displayed.

    Example usage:

    <script>
        import ListHeaderText from './ListHeaderText.svelte';

        let headers = ['Header 1', 'Header 2', 'Header 3'];
    </script>

    <ListHeaderText {headerTexts} />

-->

<script lang="ts">
    export let headerTexts: string[] = []

    import { createEventDispatcher } from "svelte"

    const dispatch = createEventDispatcher()

    const dispatchFilter = () => {
        dispatch(headerTexts[0])
    }
</script>



{#each headerTexts as headerText, i}
    <p class="text-slate-500 ml-1">
        <span>{headerText}</span>
        <span class="text-slate-300">
            <iconify-icon icon="icon-park:sort" ></iconify-icon>
        </span>
    </p>
    {#if i == 0}
        <span class="col-span-1"></span>
        <span class="col-span-1"></span>
        <span class="col-span-1"></span>
    {/if}
{/each}
<button class="text-slate-500 mr-1" on:click={() => dispatchFilter()}>
    <span class="border-slate-300">
        <iconify-icon icon="akar-icons:sort"></iconify-icon>
        </span>
    <span>Filter</span>
</button>