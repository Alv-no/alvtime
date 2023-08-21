<script lang="ts">
	import { customers } from "../../../../stores/CustomerStore";
    import Project from "./project/Project.svelte";
    export let selectProject : Function

    $: projects = $customers.customers.find((c) => c.id == $customers.active.customer)?.projects || []
</script>

<div class="mid-component">
    <ul class="project-list">
        {#each projects as project}
            <Project {project} {selectProject} />
        {/each}
    </ul>
</div>

<style>
    .mid-component {
      width: 16.67%; /* 2 out of 12 columns */
      padding: 10px;
      box-sizing: border-box;
      border-right: 1px solid #ccc;
      height: 100vh; /* Full viewport height */
    }
    
    .project-list {
      list-style: none;
      padding: 0;
    }
</style>