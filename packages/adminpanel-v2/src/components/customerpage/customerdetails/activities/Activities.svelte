<script lang="ts">
	import { customers } from "../../../../stores/CustomerStore";
    import Activity from "./activity/Activity.svelte";
    export let updateRandomPrice : Function

    $: activities = $customers.customers.find((c) => c.id == $customers.active.customer)?.projects.find((p) => p.id == $customers.active.project)?.activities || []

</script>

<div class="right-component">
    <ul class="activity-list">
        {#each activities as activity (activity.name)}
            <Activity {activity} {updateRandomPrice} />
        {/each}
    </ul>
</div>

<style>
    .right-component {
        width: 66.66%; /* 2 out of 12 columns */
      background-color: #f0f0f0;
      padding: 10px;
      box-sizing: border-box;
      float: left;
      border-right: 1px solid #ccc;
      height: 100vh; /* Full viewport height */
    }
  
    .activity-list {
      list-style: none;
      padding: 0;
      width: 100%;
    }
</style>