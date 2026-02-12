<template>
	<div class="time-tracker-container">
		<div class="time-tracker-header">
			<div class="week-number-container">
				{{ getWeekNumberString(currentWeek[0]) }}
			</div>
			<div class="current-week-string">
				{{ currentWeekString }}
			</div>
			<div class="button-wrapper">
				<AlvtimeButton
					@click="sortProjects"
				>
					<HugeiconsIcon
						:icon="SortByDown02Icon"
						class="sort-icon"
						:size="16"
					/>
				</AlvtimeButton>
				<AlvtimeButton
					id="prev-button"
					iconLeft
					@click="handlePrevClick"
          :disabled="backwardSlideDisabled"
				>
					<FeatherIcon name="chevron-left" /> Tilbake
				</AlvtimeButton>
				<AlvtimeButton
					@click="goToCurrentWeek"
				>
					I dag
				</AlvtimeButton>
				<AlvtimeButton
					id="next-button"
					iconRight
					@click="handleNextClick"
				>
					Fremover <FeatherIcon name="chevron-right" />
				</AlvtimeButton>
			</div>
		</div>
		<swiper-container
			id="week-swiper-container"
			ref="mySwiper"
			:initialSlide="getInitialWeekSlide()"
			class="swiper-container"
		>
			<swiper-slide
				v-for="(week, index) in dateStore.weeks"
				:key="index"
				class="swiper-wrapper"
			>
				<div
					class="swiper-slide"
				>
					<div class="project-list-wrapper">
						<ProjectExpandable
							v-for="project in favoriteProjects"
							:key="`${project.name}-${project.customerName}`"
							:project="project"
						>
							<template #header>
								<ProjectStats
									v-if="!isMobile"
									:project="project"
									:currentWeek="currentWeek"
								/>
							</template>
							<template #content>
								<ProjectStats
									v-if="isMobile"
									:project="project"
									:currentWeek="currentWeek"
								/>
								<DayPillStrip
									:week="week"
								/>
								<TaskStrip
									v-for="task in project.tasks"
									:key="task.id"
									:task="task"
									:week="week"
								/>
							</template>
						</ProjectExpandable>
					</div>
				</div>
			</swiper-slide>
		</swiper-container>
		<ProjectSorter v-if="taskStore.editingProjectOrder" />
		<TimeTrackerError />
	</div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import ProjectExpandable from "./ProjectExpandable.vue";
import { useTaskStore } from "@/stores/taskStore";
import { useDateStore } from "@/stores/dateStore";
import { getWeekNumber, getInitialWeekSlide} from "@/utils/weekHelper";
import FeatherIcon from "@/components/utils/FeatherIcon.vue";
import type Swiper from "swiper";
import DayPillStrip from "./DayPillStrip.vue";
import TaskStrip from "./TaskStrip.vue";
import ProjectSorter from "./ProjectSorter.vue";
import { HugeiconsIcon } from "@hugeicons/vue";
import { SortByDown02Icon } from "@hugeicons/core-free-icons";
import { storeToRefs } from "pinia";
import TimeTrackerError from "./TimeTrackerError.vue";
import AlvtimeButton from "../utils/AlvtimeButton.vue";
import ProjectStats from "./ProjectStats.vue";

const swiper = ref<Swiper | null>(null);
const taskStore = useTaskStore();
const dateStore = useDateStore();

const { favoriteProjects, editingProjectOrder } = storeToRefs(taskStore);

const isMobile = window.innerWidth <= 768;

const getWeekNumberString = (date: Date) => {
	if(date.getFullYear() !== new Date().getFullYear()) {
		return `Uke ${getWeekNumber(date)} (${date.getFullYear()})`;
	} else {
		return `Uke ${getWeekNumber(date)}`;
	}
};

const backwardSlideDisabled = computed(() => {
  return currentSlideIndex.value === 0;
})

const currentSlideIndex = computed(() => {
	if (swiper.value) {
		dateStore.setActiveWeekIndex(swiper.value.activeIndex);
		return swiper.value.activeIndex;
	}
	return 0;
});

const currentWeek = computed(() => {
	if (dateStore.weeks.length > 0) {
		return dateStore.weeks[currentSlideIndex.value];
	}
	return [];
});

const currentWeekString = computed(() => {
	if (currentWeek.value.length === 7) {
		const startDate = currentWeek.value[0];
		const endDate = currentWeek.value[6];
		const dayOptions: Intl.DateTimeFormatOptions = { day: "numeric" };
		const monthOptions: Intl.DateTimeFormatOptions = { month: "long" };
		const currentYear = new Date().getFullYear();

		const showYearStart = startDate.getFullYear() !== currentYear;
		const showYearEnd = endDate.getFullYear() !== currentYear;

		if (startDate.getFullYear() !== endDate.getFullYear()) {
			const startStr = startDate.toLocaleDateString("no-NO", { day: "numeric", month: "long", year: "numeric" });
			const endStr = endDate.toLocaleDateString("no-NO", { day: "numeric", month: "long", year: "numeric" });
			return `${startStr} til ${endStr}`;
		} else if (startDate.getMonth() === endDate.getMonth()) {
			const startDay = startDate.toLocaleDateString("no-NO", dayOptions);
			const endDay = endDate.toLocaleDateString("no-NO", dayOptions);
			const month = endDate.toLocaleDateString("no-NO", monthOptions);
			const year = showYearEnd ? ` ${endDate.getFullYear()}` : "";
			return `${startDay} til ${endDay} ${month}${year}`;
		} else {
			const startStr = startDate.toLocaleDateString("no-NO", { day: "numeric", month: "long" }) + (showYearStart ? ` ${startDate.getFullYear()}` : "");
			const endStr = endDate.toLocaleDateString("no-NO", { day: "numeric", month: "long" }) + (showYearEnd ? ` ${endDate.getFullYear()}` : "");
			return `${startStr} til ${endStr}`;
		}
	}
	return "";
});

const sortProjects = () => {
	editingProjectOrder.value = true;
};

const handleNextClick = async () => {
  if (swiper.value) {
    if (currentSlideIndex.value === dateStore.weeks.length - 1) {
      await dateStore.extendWeeks();
    }
    swiper.value?.slideNext();
  }
};

const handlePrevClick = async () => {
  if (swiper.value) {
    swiper.value?.slidePrev();
  }
};

const goToCurrentWeek = () => {
  if (swiper.value) {
    swiper.value.slideTo(getInitialWeekSlide());
  }
};

onMounted(() => {
	const swiperContainer = document.getElementById("week-swiper-container");
	if (swiperContainer && "swiper" in swiperContainer) {
		swiper.value = swiperContainer.swiper as Swiper;
	}
});
</script>

<style scoped lang="scss">
.time-tracker-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
	flex-wrap: wrap;
	gap: 12px;

	.current-week-string {
		font-size: 14px;
		font-weight: 500;
		text-wrap: nowrap;
	}
}

.week-number-container {
	background-color: rgb(230, 192, 141);
	border-radius: 10px;
	padding: 12px 12px 9px;
	font-weight: 600;
	font-size: 14px;
	text-wrap: nowrap;
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

.button-wrapper {
	display: flex;
	justify-content: flex-end;
	gap: 8px;
}

.sort-icon {
	position: relative;
	top: 4px;
}
</style>
