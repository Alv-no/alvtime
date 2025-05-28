<template>
	<div>{{ task.name }} - {{ compensationRateInPercentage }}</div>
</template>

<script lang="ts" setup>
import { defineProps, computed } from 'vue';

interface Task {
  id: string;
  name: string;
  description: string;
  favorite: boolean;
  locked: boolean;
  compensationRate: number; // Assuming this is a decimal value (e.g., 0.15 for 15%)
}

const { task } = defineProps<{
  task: Task;
}>();

const compensationRateInPercentage = computed(() => {
  return `${task.compensationRate * 100}%`;
});
</script>

<style lang="scss" scoped>

</style>