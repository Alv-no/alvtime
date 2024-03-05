<script lang="ts">
	import ListHeaderText from "./ListHeaderText.svelte";

    let PROJECT_HEADER_TEXTS: string[] = ["Prosjekt"]
    let ACTIVITY_HEADER_TEXTS: string[] = ["Aktivitet", "Pris", "Overtidsfaktor", "Sist oppdatert", "Sluttdato"]
</script>

<div class="grid-container grid grid-cols-12 items-center bg-slate-50">
    
    <!-- Project header list-->
    <div class="col-span-3 flex justify-between border-r border-b border-slate-300">
        <ListHeaderText on:Prosjekt headerTexts={PROJECT_HEADER_TEXTS} />
    </div>

    <!-- Activity header list-->
    <div class="col-span-9 flex justify-between border-r border-b border-slate-300">
        <ListHeaderText on:Aktivitet headerTexts={ACTIVITY_HEADER_TEXTS} />
    </div>
    
</div>
