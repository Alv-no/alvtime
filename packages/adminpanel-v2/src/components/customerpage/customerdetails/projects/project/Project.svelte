<script lang="ts">
    import type {TProject} from "../../../../../lib/types.ts"
    export let project : TProject
    export let selectProject : Function
</script>


<li class="project-item">
    <button on:click={() => selectProject(project.id)} class="button">
        {project.name}
    </button>
</li>