<script lang="ts">
	import type { TCustomer, TProject } from "$lib/types";
	import CustomerInfo from "./customerinfo/CustomerInfo.svelte";
	import ProjectInfo from "./projectinfo/ProjectInfo.svelte";
	import { customers } from "../../../../stores/CustomerStore";
    $: customer = $customers.customers.find((c) => c.id == $customers.active.customer)
</script>

<div class="container">
    <CustomerInfo />
    <div class="line"></div>
    <ProjectInfo />
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