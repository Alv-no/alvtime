<script lang="ts">
	import { customers } from '../../../../stores/CustomerStore';
	import Task from './task/Task.svelte';
	import NewTask from './NewTask/NewTask.svelte';
	export let filterInactiveTasks: boolean;

	// checks if we should display the new activity input fields
	$: isActiveProject = $customers.active.project !== undefined;
	$: tasks = $customers.tasks.filter((t) =>
		filterInactiveTasks
			? t.project == $customers.active.project && !t.endDate
			: t.project == $customers.active.project
	);
</script>

<div class="w-9/12 bg-gray-200 h-screen">
	{#if isActiveProject}
		<NewTask />
	{/if}
	{#each tasks as task (task.id)}
		<Task {task} />
	{/each}
</div>
