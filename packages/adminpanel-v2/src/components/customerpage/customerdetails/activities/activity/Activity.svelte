<script lang="ts">
    import type {TActivity} from "../../../../../lib/types.ts"
    export let activity : TActivity
    export let updateRandomPrice : Function
</script>


<li class="activity-item">
    <p>{activity.name}</p>
    <p>Pris: {activity.price} kr</p>
    <p>Overtidsfaktor: 1.5</p>
    <p>Endret: {activity.changedDate}</p>
    <p><iconify-icon icon="mdi:check" on:click={() => updateRandomPrice(activity)}></iconify-icon></p>
</li>

<style>
    .activity-item {
        width: 100%;
        display: flex;
        justify-content: space-around;
        border: 1px solid #ccc;
    }
</style>