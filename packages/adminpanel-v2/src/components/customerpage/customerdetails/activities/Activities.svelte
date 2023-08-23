<script lang="ts">
	import { customers } from "../../../../stores/CustomerStore";
    import Activity from "./activity/Activity.svelte";
    import NewActivity from "./NewActivity/NewActivity.svelte";

    // checks if we should display the new activity input fields
    $: isActiveProject = $customers.active.project !== undefined;
    $: activities = $customers.customers.find((c) => c.id == $customers.active.customer)?.projects.find((p) => p.id == $customers.active.project)?.activities || []

</script>

<div class="w-9/12 bg-gray-200 h-screen">
    {#if isActiveProject}
        <NewActivity />
    {/if}
    {#each activities as activity (activity.id)}
        <Activity {activity} />
    {/each}
</div>
