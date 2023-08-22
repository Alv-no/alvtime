<script lang="ts">
	import ListHeaderText from "./ListHeaderText.svelte";



    let customerHeaderTexts: string[] = ["Kunde"]
    let projectHeaderTexts: string[] = ["Prosjekt"]
    let activityHeaderTexts: string[] = ["Aktivitet", "Pris", "Overtidsfaktor"]
</script>

<div class="grid-container grid grid-cols-12 items-center bg-slate-50">
    
    <!-- Customer header list-->
    <div class="col-span-2 flex justify-between border-r-2 border-b-2 border-slate-300">
        <ListHeaderText headerTexts={customerHeaderTexts} />
    </div>

    <!-- Project header list-->
    <div class="col-span-3 flex justify-between border-r-2 border-b-2 border-slate-300">
        <ListHeaderText headerTexts={projectHeaderTexts} />
    </div>

    <!-- Activity header list-->
    <div class="col-span-7 flex justify-between border-r-2 border-b-2 border-slate-300">
        <ListHeaderText headerTexts={activityHeaderTexts} />
    </div>
    
</div>
