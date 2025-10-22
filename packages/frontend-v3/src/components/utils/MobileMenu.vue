<template>
	<div class="mobile-menu-wrapper">
		<!-- This empty div is needed to make the overlay cover the entire screen -->
		<div
			class="mobile-menu-overlay"
			:class="{ open }"
			@click="close"
		/>
		<div
			class="mobile-menu"
			:class="{ open }"
		>
			<div class="close-button-wrapper">
				<FeatherIcon
					name="x"
					:size="32"
					style="cursor: pointer;"
					@click="close"
				/>
			</div>
			<div>
				<router-link
					to="/"
					:class="{ active: $route.path === '/' }"
					@click="close"
				>
					Timeføring
				</router-link>
			</div>
			<div>
				<router-link
					to="/aktiviteter"
					:class="{ active: $route.path === '/aktiviteter' }"
					@click="close"
				>
					Aktiviteter
				</router-link>
			</div>
			<div>
				<router-link
					to="/timebank"
					:class="{ active: $route.path === '/timebank' }"
					@click="close"
				>
					Timebank
				</router-link>
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import FeatherIcon from "./FeatherIcon.vue";

const { open } = defineProps<{
	open: boolean;
}>();

const emit = defineEmits<{
	(e: "close"): void;
}>();

const close = () => {
	emit("close");
};
</script>

<style scoped lang="scss">
.mobile-menu-wrapper {
	overflow: hidden;
	width: 100vw;
}
.mobile-menu-overlay {
	background-color: rgba($primary-color, 0.7);
	height: 100%;
	left: 0;
	position: fixed;
	top: 0;
	width: 100%;
	z-index: 999;
	opacity: 0;
	pointer-events: none;
	transition: opacity 0.2s ease-in-out;

	&.open {
		opacity: 1;
		pointer-events: auto;
	}
}

.mobile-menu {
	background-color: $background-color;
	box-sizing: border-box;
	padding: 24px;
	color: $primary-color;
	box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
	position: fixed;
	top: 0;
	right: -100%;
	z-index: 1001;
	width: 70%;
	height: 100vh;
	transition: right 0.2s ease-in-out;

	&.open {
		right: 0;
	}

	a {
		color: $primary-color;
		display: block;
		font-size: 18px;
		font-weight: 500;
		margin: 24px 0;
		text-decoration: none;
		padding: 8px 12px 6px;

		&.active {
			background-color: $secondary-color-light;
			border-radius: 5px;
		}
	}

	.close-button-wrapper {
		display: flex;
		justify-content: flex-end;
	}
}
</style>