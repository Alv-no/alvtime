<script lang="ts">
    export let updateFunction: Function;
</script>

<button class="w-8 h-8 items-center justify-center rounded hover:border hover:border-black" on:click={() => updateFunction()}>
    <iconify-icon icon={"fluent:edit-20-regular"} class="text-alv-dark-blue" width="22px"></iconify-icon>
</button>
