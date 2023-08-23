<script lang="ts">
	import Activities from "./activities/Activities.svelte";
	import Projects from "./projects/Projects.svelte";
	import CustomerHeader from "./headers/CustomerHeader.svelte";
    import ListHeader from "./headers/ListHeader.svelte";
    export let selectProject : Function
</script>

<div class="bg-gray-200 w-full">
    <CustomerHeader />
    <ListHeader /> 
    <div class="flex">
        <Projects {selectProject} />
        <Activities/>
    </div>
</div>
