<script lang="ts">
    export let updateFunction: Function;
    export let newProjectName: string|undefined;
    export let newProjectNumber: number|undefined;
    
    const addStyling: string ="rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent"

</script>

<p>
    Prosjekt:
    <input type="text" bind:value={newProjectName} on:change={() => updateFunction(newProjectName, newProjectNumber)} class={addStyling}/>
</p>
<p>
    Prosjektnr:
    <input type="text" bind:value={newProjectNumber} on:change={() => updateFunction(newProjectName, newProjectNumber)} class={addStyling}/>
</p>
