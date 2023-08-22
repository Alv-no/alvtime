<script lang="ts">
    import type { TProject } from "$lib/types.ts";
    import Project from "./project/Project.svelte";
    export let chosenProjects : TProject[]
    export let selectProject : Function
</script>

<div class="mid-component">
    <ul class="project-list">
        {#each chosenProjects as project}
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