<script lang="ts">
	import { customers } from '../../../../../stores/CustomerStore';
	import EditButton from '../../../../generic/buttons/EditButton.svelte';
	$: project = $customers.projects.find((p) => p.id == $customers.active.project);

	let edit: boolean = false;

	let updateFunction = () => {
		edit = !edit;
	};

	let updateProjectName = () => {
		customers.updateProject(project!);
	};

	let updateProjectNumber = () => {
		customers.updateProject(project!);
	};
</script>

<div class="header">
	{#if project}
		<p>
			Prosjekt: <input
				type="text"
				disabled={!edit}
				bind:value={project.name}
				on:change={(e) => updateProjectName()}
			/>
		</p>
		<p>
			Prosjektnummer: <input
				type="text"
				disabled={!edit}
				bind:value={project.projectNumber}
				on:change={(e) => updateProjectNumber()}
			/>
		</p>
		<EditButton {updateFunction} />
	{:else}
		<p>Prosjekt</p>
	{/if}
</div>

<style>
	.header {
		width: 45%;
		display: flex;
		justify-content: space-between;
		align-items: center;
	}
</style>
