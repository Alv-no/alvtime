<template>
	<div class="paginator">
		<button
			class="prev-button"
			:disabled="currentPage === 1"
			@click="changePage(-1)"
		>
			<FeatherIcon
				name="arrow-left"
				:size="16"
			/>
			Forrige
		</button>
		<span>Side {{ currentPage }} av {{ totalPages }}</span>
		<button
			class="next-button"
			:disabled="currentPage === totalPages"
			@click="changePage(1)"
		>
			Neste
			<FeatherIcon
				name="arrow-right"
				:size="16"
			/>
		</button>
	</div>
</template>

<script setup lang="ts">
import FeatherIcon from "@/components/utils/FeatherIcon.vue";

const currentPage = defineModel("currentPage", {
	type: Number,
	default: 1,
});

const { totalPages } = defineProps<{
	totalPages: number;
}>();

const changePage = (change: number) => {
	if (
		(change === -1 && currentPage.value > 1) ||
		(change === 1 && currentPage.value < totalPages)
	) {
		currentPage.value = currentPage.value + change;
	}
};
</script>

<style scoped lang="scss">
.paginator {
	margin: 1rem 0;
	display: flex;
	justify-content: center;
	align-items: center;
	gap: 2rem;

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

		&:disabled {
			background-color: grey;
			cursor: not-allowed;
		}
	}
}
</style>