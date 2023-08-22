<script lang="ts">
	import ListHeaderText from "./ListHeaderText.svelte";

    let projectHeaderTexts: string[] = ["Prosjekt"]
    let activityHeaderTexts: string[] = ["Aktivitet", "Pris", "Overtidsfaktor"]
</script>

<div class="grid-container grid grid-cols-12 items-center bg-slate-50">
    <!-- Project header list-->
    <div class="col-span-2 flex justify-between border-r border-b border-slate-300">
        <ListHeaderText headerTexts={projectHeaderTexts} />
    </div>

    <!-- Activity header list-->
    <div class="col-span-10 flex justify-between border-r border-b border-slate-300">
        <ListHeaderText headerTexts={activityHeaderTexts} />
    </div>
    
</div>
