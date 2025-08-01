<template>
	<div class="time-entry-comment">
		<textarea
			v-model="comment"
			rows="3"
			@focus="isActive = true"
			@blur="isActive = false"
			@input="updateComment(comment)"
		/>
		<p
			v-if="commentedAt"
			class="last-updated"
		>
			Sist endret:<br />{{ formattedTime }}
		</p>
	</div>
</template> 

<script setup lang="ts">
import { computed } from "vue";
import type { TimeEntry } from "@/types/TimeEntryTypes";
import { debounce } from "@/utils/generalHelpers";

const isActive = defineModel<boolean>("isActive", { default: false });
const comment = defineModel<string>("comment", { default: "" });
const commentedAt = defineModel<string | null>("commentedAt", { default: "" });

const { timeEntry } = defineProps<{
	timeEntry: TimeEntry;
}>();

const emit = defineEmits<{
	(e: "comment-updated", value: string): void;
}>();

const updateComment = debounce((newComment: string) => {
	if(newComment !== timeEntry.comment) {
		emit("comment-updated", newComment);
	}
}, 300);

const formattedTime = computed(() => {
	if (commentedAt.value) {
		const date = new Date(commentedAt.value);
		return date.toLocaleString("nb-NO", { hour: "2-digit", minute: "2-digit", day: "2-digit", month: "2-digit", year: "numeric" });
	}
	return "";
});
</script>

<style scoped lang="scss">
.time-entry-comment {
	position: fixed;
	z-index: 1000;
	margin-top: 12px;
	padding: 4px 4px 0;
	background-color: $background-color;
	border: 1px solid $primary-color
}

.last-updated {
	font-size: 12px;
	color: $primary-color;
	margin-top: 4px;
	margin-bottom: 4px;
}
</style>
