<template>
	<div class="input-wrapper">
		<input
			:id="`${timeEntry.taskId}-${timeEntry.date}`"
			v-model="timeValue"
			type="text"
			class="form-control"
			:class="{ 'has-value': timeEntry.value > 0, weekend, 'is-complete': noTimeRemainingInWorkday }"
			@focus="onInputFocus"
			@blur="onInputBlur"
			@change="updateTimeEntry(timeValue)"
		/>
		<TrackRestOfDayButton
			v-if="isInputActive || commentIsActive"
			:currentValue="parseFloat(timeValue.replace(',', '.'))"
			:date="timeEntry.date"
			@track-rest-of-day="trackRestOfDay"
		/>
		<CommentPill v-if="comment" />
		<TimeEntryComment 
			v-if="(isInputActive && enableComments) || commentIsActive"
			v-model:isActive="commentIsActive"
			v-model:comment="comment"
			:timeEntry="timeEntry"
			@comment-updated="updateComment"
		/>
	</div>
</template>

<script lang="ts" setup>
import { ref, computed } from "vue";
import { type TimeEntry } from "@/types/TimeEntryTypes";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";
import TrackRestOfDayButton from "./TrackRestOfDayButton.vue";
import TimeEntryComment from "./TimeEntryComment.vue";
import CommentPill from "./CommentPill.vue";

const isInputActive = ref(false);
const commentIsActive = ref(false);
const timeEntriesStore = useTimeEntriesStore();

const { timeEntry, enableComments } = defineProps<{
	timeEntry: TimeEntry;
	enableComments?: boolean;
}>();

const comment = ref<string>(timeEntry.comment || "");

const timeValue = ref<string>(timeEntry.value.toLocaleString("nb-NO"));

const weekend = computed(() => {
	const day = new Date(timeEntry.date).getDay();
	return day === 0 || day === 6;
});

const updateTimeEntry = (timeValue: string) => {
	if (timeValue) {
		timeEntriesStore.updateTimeEntry({ ...timeEntry, value: parseFloat(timeValue.replace(",", ".")) });
	}
};

const updateComment = (comment: string) => {
	timeEntriesStore.updateTimeEntry({ ...timeEntry, comment });
};

const trackRestOfDay = (currentValue: number) => {
	timeValue.value = currentValue.toLocaleString("nb-NO");
	updateTimeEntry(currentValue.toLocaleString("nb-NO"));
};


const hideTrackButton = () => {
	setTimeout(() => { isInputActive.value = false; }, 200);
};

const onInputBlur = () => {
	hideTrackButton();
	if(!timeValue.value) {
		timeValue.value = "0";
		timeEntriesStore.updateTimeEntry({ ...timeEntry, value: 0 });
	}
};

const onInputFocus = () => {
	isInputActive.value = true;
	const inputElement = document.getElementById(`${timeEntry.taskId}-${timeEntry.date}`) as HTMLInputElement;
	inputElement.select();
};

const noTimeRemainingInWorkday = computed(() => {
	return timeEntriesStore.getRemainingTimeInWorkday(timeEntry.date) <= 0;
});

</script>

<style lang="scss" scoped>
.input-wrapper {
	position: relative;

	input {
		text-align: center;

		&.weekend {
			background-color: #f0f0f0;
		}

		&.is-complete {
			background-color: $secondary-color-light;
		}
	}
}
</style>
