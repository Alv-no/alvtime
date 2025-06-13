<template>
	<div class="time-tracker-container">
		<swiper-container id="week-swiper-container" ref="mySwiper" class="swiper-container">
			<swiper-slide
				class="swiper-wrapper"
				v-for="(week, index) in dateStore.weeks"
				:key="index"
			>
				<div
					class="swiper-slide"
				>
					<div class="time-tracker-header">
						<div class="week-number-container">
							{{ `UKE ${getWeekNumber(week[0])}` }}
						</div>
					</div>
					<div class="project-list-wrapper">
						<ProjectExpandable
							v-for="project in taskStore.tasks"
							:key="project.id"
							:project="project"
							:week="week"
						/>
					</div>
				</div>
			</swiper-slide>
		</swiper-container>
	</div>
</template>

<script setup lang="ts">
import ProjectExpandable from "./ProjectExpandable.vue";
import { useTaskStore } from "@/stores/taskStore";
import { useDateStore } from "@/stores/dateStore";
import { getWeekNumber} from "@/utils/weekHelper";

const taskStore = useTaskStore();
const dateStore = useDateStore();
</script>

<style scoped lang="scss">
.time-tracker-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.week-number-container {
	background-color: rgb(230, 192, 141);
	border-radius: 10px;
	padding: 12px 12px 9px;
	font-weight: 600;
	font-size: 14px;
}

.project-list-wrapper {
	display: flex;
	flex-direction: column;
	gap: 16px;
	margin-top: 16px;
}

.swiper-container {
	width: 100%;
	height: 100%;
}
</style>
