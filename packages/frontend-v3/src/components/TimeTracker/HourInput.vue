<template>
	<div class="input-wrapper">
		<input
			:id="`${timeEntry.taskId}-${timeEntry.date}`"
			v-model="timeValue"
			type="text"
			class="form-control"
			:class="{ 'has-value': timeEntry.value > 0 }"
			@focus="isInputActive = true"
			@blur="hideTrackButton"
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
import { ref } from "vue";
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

const updateTimeEntry = (timeValue: string) => {
	if (timeValue) {
		timeEntriesStore.updateTimeEntry({ ...timeEntry, value: parseFloat(timeValue.replace(",", ".")) });
	}
};

const updateComment = (comment: string) => {
	timeEntriesStore.updateTimeEntry({ ...timeEntry, comment });
};

const trackRestOfDay = (currentValue: number) => {
	console.log("currentValue:", currentValue);
	timeValue.value = currentValue.toLocaleString("nb-NO");
	updateTimeEntry(currentValue.toLocaleString("nb-NO"));
};


const hideTrackButton = () => {
	setTimeout(() => { isInputActive.value = false; }, 200);
};

</script>

<style lang="scss" scoped>
.input-wrapper {
	position: relative;
}

input {
	border-radius: 5px;
	border: 1px solid #ccc;
	background-color: rgb(250, 245, 235);
	transition: border-color 0.3s ease;
	padding: 8px;
	width: 48px;
	text-align: center;
	font-size: 1rem;

	&.has-value {
		border-color: $primary-color;
	}
}
</style>
