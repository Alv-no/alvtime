<script lang="ts">
	import { customers } from "../../../../stores/CustomerStore";
    import Project from "./project/Project.svelte";
    export let selectProject : Function
    
    $: projects = $customers.projects.filter((p) => p.Customer == $customers.active.customer)
    
</script>

<div class="w-3/12 bg-gray-200 h-screen items-center">
    {#each projects as project}
        <Project {project} {selectProject} />
    {/each}
</div>