<template>
	<StatusBar />
	<TimeTracker class="margin-top" />
</template>

<script setup lang="ts">
import StatusBar from "@/components/StatusBar.vue";
import TimeTracker from "@/components/TimeTracker/TimeTracker.vue";

</script>

<style scoped lang="scss">
.margin-top {
	margin-top: 16px;
}
</style>