<script lang="ts">
	import Activities from "./activities/Activities.svelte";
	import Projects from "./projects/Projects.svelte";
    import type { TCustomer, TProject } from "$lib/types.ts";
    import type { TActivity } from "$lib/types";
	import CustomerHeader from "./headers/CustomerHeader.svelte";
    import ListHeader from "./headers/ListHeader.svelte";
    export let activeCustomer : TCustomer | null
    export let activeProject: TProject | null
    export let chosenActivities : TActivity[]
    export let updateRandomPrice : Function
    export let chosenProjects : TProject[]
    export let selectProject : Function
</script>

<div class="customer">
    <CustomerHeader {activeCustomer} {activeProject} />
    <ListHeader /> 
    <div class="listcontainer">
        <Projects {chosenProjects} {selectProject} />
        <Activities {chosenActivities} {updateRandomPrice} />
    </div>
</div>

<style>
.customer {
    width: 100%;
    
}

</style>