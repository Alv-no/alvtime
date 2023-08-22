<script lang="ts">
	import { customers } from "../../../../stores/CustomerStore";
    import Project from "./project/Project.svelte";
    export let selectProject : Function

    $: projects = $customers.customers.find((c) => c.id == $customers.active.customer)?.projects || []
</script>

<div class="w-3/12 bg-gray-200 h-screen items-center">
    {#each projects as project}
        <Project {project} {selectProject} />
    {/each}
</div>