<template>
	<div class="navigation-bar">
		<div class="navigation-container">
			<div>
				<img
					class="logo"
					src="@/assets/alv.svg"
				/>
			</div>
			<p>{{ user?.name }}</p>
			<LogOutButton />
		</div>
	</div>
</template>

<script setup lang="ts">
import LogOutButton from "@/components/LogOutButton.vue";
import { useUserStore } from "@/stores/useUserStore";

const { user } = useUserStore();
</script>

<style scoped lang="scss">
.navigation-bar {
	display: flex;
	justify-content: center;
	align-items: center;
	height: 60px;
	background-color: rgb(36, 63, 77);
	color: rgb(248, 241, 227);

	.navigation-container {
		width: 1200px;
		padding: 0 20px;
		display: flex;
		justify-content: space-between;
		align-items: center;

		a {
			color: white;
			text-decoration: none;
			margin-right: 20px;

			&:hover {
				text-decoration: underline;
			}
		}
	}

	.logo {
		height: 32px;
	}
}
</style>