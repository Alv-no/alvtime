<script lang="ts">
	import type { TProject } from '../../../../../lib/types.ts';
	import { customers } from '../../../../../stores/CustomerStore.ts';
	export let project: TProject;
	export let selectProject: Function;

	$: inactive = project.endDate ? true : false;
	$: buttonStyling =
		project.id == $customers.active.project
			? 'border-2 border-blue-500 bg-sky-50 w-full flex'
			: inactive
			? 'border-r border-b border-gray-300 bg-gray-100 w-full flex'
			: 'border-r border-b border-gray-300 bg-white w-full flex';
</script>

<button on:click={() => selectProject(project.id)} class={buttonStyling}>
	<div class="p-4 w-full flex justify-between">
		<span class="text-sm text-gray-800">{project.name}</span>
		<span class="text-sm text-slate-400">Sist oppdatert: {project.changedDate}</span>
	</div>
</button>
