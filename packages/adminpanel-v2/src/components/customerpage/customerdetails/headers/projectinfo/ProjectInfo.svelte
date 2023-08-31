<script lang="ts">
	import { clickOutside } from '$lib/functions/clickOutside';
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

	const handleClickOutside = () => {
		edit = false
	}

</script>

<div class="w-3/5 flex justify-between items-center"
	use:clickOutside
	on:message={handleClickOutside}
	>
	{#if add}
		<ProjectInfoAdd />
	{:else if project}
		<ProjectInfoEdit {project} {edit} />
	{:else}
		<p>Prosjekt</p>
	{/if}

	{#if project !== undefined}
		<EditButton {updateFunction} isActive={edit} isDisabled={add} />
	{/if}
	<AddButton
		{addFunction}
		isDisabled={edit || activeCustomer === undefined}
	/>
</div>
