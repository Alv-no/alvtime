<script lang="ts">
	import { clickOutside } from '$lib/functions/clickOutside';
	import { customers } from '../../../../../stores/CustomerStore';
	import AddButton from '../../../../generic/buttons/AddButton.svelte';
	import EditButton from '../../../../generic/buttons/EditButton.svelte';
	import ProjectInfoAdd from './projectInfoAdd.svelte';
	import ProjectInfoEdit from './projectInfoEdit.svelte';

	$: activeCustomer = $customers.active.customer;
	$: project = $customers.projects.find((p) => p.id == $customers.active.project);

	let newProjectName: string|undefined;
    let newProjectNumber: number|undefined;

	let edit: boolean = false;
	let add: boolean = false;

	let addFunction = () => {
		if (add && !(activeCustomer && newProjectName && newProjectNumber)) {
			throw new Error('Not valid state for program...');
		} else if (add) {
			customers.addNewProject(newProjectName!, newProjectNumber!, activeCustomer!)
			add = !add;
			newProjectName = undefined;
			newProjectNumber = undefined;
			console.log(newProjectName, newProjectNumber)
		} else add = !add
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
		add = false
	}

	const updateAddNewProject = (projectName: string, projectNumber: number) => {
		newProjectName = projectName;
		newProjectNumber = projectNumber;
	}

</script>

<div class="w-3/5 flex justify-between items-center"
	use:clickOutside
	on:message={handleClickOutside}
	>
	{#if add}
		<ProjectInfoAdd updateFunction={updateAddNewProject} {newProjectName} {newProjectNumber} />
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
		isActive={add}
		isDisabled={edit || activeCustomer === undefined}
	/>
</div>
