<template>
	<div class="search-wrapper">
		<input v-model="filterQuery" placeholder="Søk i listen" />
		<button>
			<FeatherIcon name="x" :size="24" @click="clearQuery" />
		</button>
	</div>
</template>

<script setup lang="ts">
import { useTaskStore } from "@/stores/taskStore";
import { computed } from "vue";
import FeatherIcon from "@/components/utils/FeatherIcon.vue";

const taskStore = useTaskStore();
const filterQuery = computed({
	get: () => taskStore.filterQuery,
	set: (value) => {
		taskStore.filterQuery = value;
	}
});

const clearQuery = () => {
	filterQuery.value = "";
};
</script>

<style lang="scss" scoped>
.search-wrapper {
	display: flex;
	gap: 4px;

	input {
		flex-grow: 1;
	}

	button {
		border-radius: 5px;
		outline: none;
		background-color: $secondary-color;
		color: $primary-color;
		padding: 4px 4px;
		border: none;

		&:hover {
			background-color: $secondary-color-light;
		}

		&:active {
			background-color: $secondary-color;
		}
	}
}
</style>
