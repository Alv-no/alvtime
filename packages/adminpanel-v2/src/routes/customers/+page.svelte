<script lang="ts">
    import 'iconify-icon'
    import type { TProject, TActivity, TCustomer } from '$lib/types';
	import CustomerDetails from '../../components/customerpage/customerdetails/CustomerDetails.svelte';
	import CustomerList from '../../components/customerpage/customerlist/CustomerList.svelte';
	import { customers } from '../../stores/CustomerStore';


    function selectCustomer(customerId: number) {
        customers.setCustomer(customerId)
    }
    function selectProject(projectId: number) {
        customers.setProject(projectId)
        console.log($customers)
    }
    function getRandomPrice() {
        return Math.floor(Math.random() * 10000) + 1000; // Generates a random number between 1000 and 11000
    }
    function updateRandomPrice(activity: TActivity) {
        activity.price = getRandomPrice();
    }
    function updateActivitiesPrices() {
        //
    }
  </script>

<div class="container">
    <CustomerList {selectCustomer} />
    <CustomerDetails {selectProject} />
</div>

<style>
    .container {
        display: flex;
        width: 100%;
    }
  </style>