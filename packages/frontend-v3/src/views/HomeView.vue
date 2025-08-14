<template>
	<StatusBar />
	<TimeTracker
		v-if="!loading"
		class="margin-top"
	/>
</template>

<script setup lang="ts">
import StatusBar from "@/components/StatusBar.vue";
import TimeTracker from "@/components/TimeTracker/TimeTracker.vue";
import { useTaskStore } from "@/stores/taskStore";
import { useDateStore } from "@/stores/dateStore";
import { ref, onMounted } from "vue";

const loading = ref(true);

onMounted(async () => {
	const taskStore = useTaskStore();
	const dateStore = useDateStore();
	await Promise.all([
		dateStore.setActiveDate(new Date()),
		taskStore.getTasks()
	]);
	loading.value = false;
});

</script>

<style scoped lang="scss">
.margin-top {
	margin-top: 16px;
}
</style>
