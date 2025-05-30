<template>
	<div>{{ task.name }} - {{ compensationRateInPercentage }}</div>
</template>

<script lang="ts" setup>
import { defineProps, computed } from "vue";
import { type Task } from "@/types/ProjectTypes";

const { task } = defineProps<{
  task: Task;
}>();

const compensationRateInPercentage = computed(() => {
  return `${task.compensationRate * 100}%`;
});
</script>

<style lang="scss" scoped>

</style>