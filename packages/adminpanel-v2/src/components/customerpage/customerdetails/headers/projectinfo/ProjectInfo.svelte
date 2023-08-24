<script lang="ts">
	import { customers } from '../../../../../stores/CustomerStore';
	import AddButton from '../../../../generic/buttons/AddButton.svelte';
	import EditButton from '../../../../generic/buttons/EditButton.svelte';
	import ProjectInfoAdd from './projectInfoAdd.svelte';
	import ProjectInfoEdit from './projectInfoEdit.svelte';

	$: activeCustomer = $customers.active.customer;
	$: project = $customers.projects.find((p) => p.id == $customers.active.project);


	let edit: boolean = false;
	let add: boolean = false;

	let addFunction = () => {
		if (add && project === undefined) {
			throw new Error('Not valid state for program...');
		} else if (add && project) {
			customers.updateProject(project);
		}
		add = !add;
	};

	let updateFunction = () => {
		if (edit && project === undefined) {
			throw new Error('Not valid state for program...');
		} else if (edit && project) {
			customers.updateProject(project);
		}
		edit = !edit;
	};
</script>

<div class="w-1/2 flex justify-between items-center">
	{#if add}
		<ProjectInfoAdd />
	{:else}
		<ProjectInfoEdit {edit} />
	{/if}

	{#if project !== undefined}
		<EditButton {updateFunction} isDisabled={add} />
	{/if}
	<AddButton
		{addFunction}
		isDisabled={edit || activeCustomer === undefined}
	/>
</div>
