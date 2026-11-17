<template>
	<AlvtimeButton
		type="button"
		@click="open = true"
	>
		Registrer fravær
	</AlvtimeButton>
	<AbsencePlannerModal
		v-if="open"
		@close="open = false"
	/>
</template>

<script setup lang="ts">
import { ref } from "vue";
import AlvtimeButton from "@/components/utils/AlvtimeButton.vue";
import AbsencePlannerModal from "./AbsencePlannerModal.vue";

const open = ref(false);
</script>
