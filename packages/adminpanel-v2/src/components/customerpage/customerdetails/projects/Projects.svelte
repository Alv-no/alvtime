<script lang="ts">
	import { customers } from '../../../../stores/CustomerStore';
	import Project from './project/Project.svelte';
	export let selectProject: Function;
	export let filterInactiveProjects: boolean;

	$: projects = $customers.projects.filter((p) =>
		filterInactiveProjects
			? p.customer == $customers.active.customer && !p.endDate
			: p.customer == $customers.active.customer
	);
</script>

<div class="w-3/12 bg-gray-200 h-screen items-center">
	{#each projects as project}
		<Project {project} {selectProject} />
	{/each}
</div>
