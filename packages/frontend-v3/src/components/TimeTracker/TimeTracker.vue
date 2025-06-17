<template>
	<div class="time-tracker-container">
		<div class="time-tracker-header">
			<div class="week-number-container">
				{{ getWeekNumberString(currentWeek[0]) }}
			</div>
			<div class="button-wrapper">
				<button
					id="prev-button"
					class="prev-button"
					@click="prevSlide"
				>
					<FeatherIcon name="chevron-left" /> Tilbake
				</button>
				<button @click="goToCurrentWeek">
					I dag
				</button>
				<button
					id="next-button"
					class="next-button"
					@click="nextSlide"
				>
					Fremover <FeatherIcon name="chevron-right" />
				</button>
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
import { onMounted, ref, computed } from "vue";
import ProjectExpandable from "./ProjectExpandable.vue";
import { useTaskStore } from "@/stores/taskStore";
import { useDateStore } from "@/stores/dateStore";
import { getWeekNumber, getInitialWeekSlide} from "@/utils/weekHelper";
import FeatherIcon from "@/components/utils/FeatherIcon.vue";
import type Swiper from "swiper";

const swiper = ref<Swiper | null>(null);
const taskStore = useTaskStore();
const dateStore = useDateStore();

const getWeekNumberString = (date: Date) => {
	if(date.getFullYear() !== new Date().getFullYear()) {
		return `Uke ${getWeekNumber(date)} (${date.getFullYear()})`;
	} else {
		return `Uke ${getWeekNumber(date)}`;
	}
};

const currentSlideIndex = computed(() => {
	if (swiper.value) {
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

const nextSlide = () => {
	if (swiper.value) {
		swiper.value?.slideNext();
	}
};

const prevSlide = () => {
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
	width: 100%;
	gap: 8px;

	button {
		border: none;
		cursor: pointer;
		background-color: $secondary-color;
		color: $primary-color;
		border-radius: 25px;
		padding: 9px 16px 12px 16px;
		font-size: 14px;
		font-weight: 600;

		&:hover {
			background-color: $secondary-color-light;
		}

		&.next-button {
			padding: 9px 12px 12px 16px;
		}

		&.prev-button {
			padding: 9px 16px 12px 12px;
		}
	}
}
</style>
