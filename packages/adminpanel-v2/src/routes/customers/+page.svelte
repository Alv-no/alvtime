<script lang="ts">
    import 'iconify-icon'
    import { customers } from '$lib/mock/customers';  
    import type { TProject, TActivity, TCustomer } from '$lib/types';
	import CustomerDetails from '../../components/customerpage/customerdetails/CustomerDetails.svelte';
	import CustomerList from '../../components/customerpage/customerlist/CustomerList.svelte';
    let activeCustomer : TCustomer | null =  null
    let chosenProjects: TProject[] = [];
    let activeProject: TProject | null = null
    let chosenActivities: TActivity[] = [];
    function selectCustomer(customerId: number) {
        let customer = customers.find(c => c.id == customerId)
        if (activeCustomer != customer) {
            activeCustomer = customer!
            chosenProjects = activeCustomer?.projects
            activeProject = null
            chosenActivities = []
        }
    }
    function selectProject(projectId: number) {
        let project = chosenProjects.find(p => p.id == projectId)
        if (activeProject != project) {
            activeProject = project!
            chosenActivities = activeProject.activities
            updateActivitiesPrices()
        }
    }
    function getRandomPrice() {
        return Math.floor(Math.random() * 10000) + 1000; // Generates a random number between 1000 and 11000
    }
    function updateRandomPrice(activity: TActivity) {
        activity.price = getRandomPrice();
    }
    function updateActivitiesPrices() {
        chosenActivities.forEach(activity => {
            activity.price = getRandomPrice();
        });
    }
  </script>

<div class="container">
    <CustomerList {customers} {selectCustomer} />
    <CustomerDetails {activeCustomer} {activeProject} {chosenActivities} {chosenProjects} {selectProject} {updateRandomPrice} />
</div>

<style>
    .container {
        display: flex;
    }
    .activity-button {
        display: flex;
    }
    .button {
        width: 100%;
    }
  </style>