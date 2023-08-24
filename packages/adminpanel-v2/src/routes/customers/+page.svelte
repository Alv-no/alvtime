<script lang="ts">
    import 'iconify-icon'
	import CustomerDetails from '../../components/customerpage/customerdetails/CustomerDetails.svelte';
	import CustomerList from '../../components/customerpage/customerlist/CustomerList.svelte';
	import { customers } from '../../stores/CustomerStore';


    function selectCustomer(customerId: number) {
        customers.setCustomer(customerId)
    }
    function selectProject(projectId: number) {
        customers.setProject(projectId)
    }
  </script>

<div class="w-full flex">
    <CustomerList {selectCustomer} />
    <CustomerDetails {selectProject} />
</div>
