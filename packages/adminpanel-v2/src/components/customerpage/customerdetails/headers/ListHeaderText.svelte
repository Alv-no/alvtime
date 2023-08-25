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
	import type { EnsureDefined } from "../../../../routes/$types";
    import type {sortOptions} from "../../../../lib/types";

    export let headerTexts: string[] = []

    import { createEventDispatcher } from "svelte"

    const dispatch = createEventDispatcher()

    const dispatchFilter = () => {
        dispatch(headerTexts[0])
    }
    export let activeSearchOptions : sortOptions[]

    let isDropdownOpen = false

    const handleDropdownClick = () => {
        isDropdownOpen = !isDropdownOpen // togle state on click
    }

    export let selected : sortOptions;
</script>



{#each headerTexts as headerText}
    <p class="text-slate-500 ml-1">
        <span>{headerText}</span>
        <button class="text-slate-300" on:click={handleDropdownClick}>
            <iconify-icon icon="icon-park:sort" ></iconify-icon>
        </button>
    </p>
    {#if isDropdownOpen}
    <select
        bind:value={selected}
    >
        {#each activeSearchOptions as searchOption}
            <option value={searchOption}>
                {searchOption}
            </option>
        {/each}
    </select>
    {/if}
{/each}
<button class="text-slate-500 mr-1" on:click={() => dispatchFilter()}>
    <span class="border-slate-300">
        <iconify-icon icon="akar-icons:sort"></iconify-icon>
        </span>
    <span>Filter</span>
</button>