<script lang="ts">
	import Tasks from "./tasks/Tasks.svelte";
	import Projects from "./projects/Projects.svelte";
	import CustomerHeader from "./headers/CustomerHeader.svelte";
    import ListHeader from "./headers/ListHeader.svelte";
    export let selectProject : Function
    
    let filterInactiveProjects: boolean = false
    const handleProjectFilter = () => {
        filterInactiveProjects = !filterInactiveProjects
    }

    let filterInactiveTasks: boolean = false
    const handleTaskFilter = () => {
        filterInactiveTasks = !filterInactiveTasks
    }
</script>

<div class="bg-gray-200 w-full">
    <CustomerHeader />
    <ListHeader on:Prosjekt={() => handleProjectFilter()} on:Aktivitet={() => handleTaskFilter()} /> 
    <div class="flex">
        <Projects {filterInactiveProjects} {selectProject} />
        <Tasks {filterInactiveTasks}/>
    </div>
</div>
