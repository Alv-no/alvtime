<script lang="ts">
    import type { TActivity } from "$lib/types";
    import Activity from "./activity/Activity.svelte";
    export let chosenActivities : TActivity[]
    export let updateRandomPrice : Function
</script>

<div class="right-component">
    <ul class="activity-list">
        {#each chosenActivities as activity (activity.name)}
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