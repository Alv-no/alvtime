<script lang="ts">
	import type { TCustomer, TProject } from "$lib/types";
	import CustomerInfo from "./customerinfo/CustomerInfo.svelte";
	import ProjectInfo from "./projectinfo/ProjectInfo.svelte";

    export let activeCustomer: TCustomer | null
    export let activeProject: TProject | null

</script>

<div class="container">
    <CustomerInfo {activeCustomer} />
    <div class="line"></div>
    <ProjectInfo {activeProject} />
</div>

<style>
    .line {
        height: 100%;
        border-left: 1px solid black;
    }

    .container {
        display: flex;
        width: 100%;
    }
</style>

