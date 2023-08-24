<script lang="ts">
	import type { TTask } from '../../../../../lib/types.ts';
	import { customers } from '../../../../../stores/CustomerStore.ts';
	import EditButton from '../../../../generic/buttons/EditButton.svelte';
	export let task: TTask;
	let isEditingTask: boolean = false;

	$: inactive = task.endDate ? true : false;
	$: hourRate = $customers.hourRate.find((h) => h.taskId == task.id)!;
	$: compensationRate = $customers.compensationRate.find((c) => c.taskId == task.id)!;

	$: taskStyling = inactive
		? 'border-r border-b border-gray-300 bg-gray-100 w-full flex justify-center p-4 grid grid-cols-12 gap-4'
		: 'border-r border-b border-gray-300 bg-white w-full flex justify-center p-4 grid grid-cols-12 gap-4';

	$: nameStyling = isEditingTask
		? 'w-full col-span-3 rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent'
		: 'w-full col-span-3 bg-transparent rounded border border-transparent';

	$: hourRateStyling = isEditingTask
		? 'col-span-2 bg-transparent rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent'
		: 'col-span-2 bg-transparent rounded border border-transparent';

	$: compensationRateStyling = isEditingTask
		? 'col-span-2 bg-transparent rounded border border-gray-300 focus:outline-none focus:border-blue-500 disabled:bg-transparent'
		: 'col-span-2 bg-transparent rounded border border-transparent';

	$: editButtonStyling = isEditingTask
		? 'rounded border-2 justify-center align-center border-blue-500'
		: 'rounded border-2 justify-center align-center border-transparent';

	let editTask = () => {
		isEditingTask = !isEditingTask;
		if (isEditingTask) {
			customers.setTask(task.id);
			customers.updateTask(task, hourRate, compensationRate);
		}
	};
</script>

<!-- LEGGE INN NY AKTIVTET!!! -->
<!-- Currently editing -->

<button class={taskStyling}>
	<input type="text" disabled={!isEditingTask} class={nameStyling} bind:value={task.name} />
	<input type="text" class={hourRateStyling} bind:value={hourRate.rate} disabled={!isEditingTask} />

	<span class="col-span-2" />
	<input
		type="text"
		class={compensationRateStyling}
		bind:value={compensationRate.value}
		disabled={!isEditingTask}
	/>
	<span class="col-span-2" />

	<EditButton updateFunction={editTask} isDisabled={false} />
</button>
