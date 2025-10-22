<template>
	<div class="day-pill-strip">
		<DayPill
			v-for="day in week"
			:key="day.getMilliseconds()"
			:day="day"
		/>
	</div>
</template>

<script setup lang="ts">
import DayPill from "./DayPill.vue";

const { week } = defineProps<{
	week: Date[];
}>();
</script>

<style scoped lang="scss">
.day-pill-strip {
	display: flex;
	justify-content: flex-end;
	align-items: center;
	gap: 1rem;
	margin-top: .2rem;

	@media screen and (max-width: 768px) {
		gap: unset;
		justify-content: space-between;
	}
}
</style>