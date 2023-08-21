<script lang="ts">
	import Activities from "./activities/Activities.svelte";
	import Projects from "./projects/Projects.svelte";
	import CustomerHeader from "./headers/CustomerHeader.svelte";
    import ListHeader from "./headers/ListHeader.svelte";
    export let updateRandomPrice : Function
    export let selectProject : Function
</script>

<div class="customer">
    <CustomerHeader />
    <ListHeader /> 
    <div class="listcontainer">
        <Projects {selectProject} />
        <Activities {updateRandomPrice} />
    </div>
</div>

<style>
.customer {
    width: 100%;
    
}

</style>