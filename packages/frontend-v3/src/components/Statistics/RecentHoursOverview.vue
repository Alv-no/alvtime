<template>
	<h3>Timesoversikt siste 3 måneder</h3>
</template>

<script setup lang="ts">
import type { TimeEntryOverview } from "@/stores/statisticsStore.ts";

const props = defineProps<{
  timeEntryOverview: TimeEntryOverview[],
}>();
</script>

<style scoped lang="scss">

</style>