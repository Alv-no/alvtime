<template>
	<div v-if="user?.isAuthenticated">
		<NavigationBar />
		<div class="wrapper">
			<div class="content-container">
				<RouterView :key="$route.path" />
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import NavigationBar from "./components/NavigationBar.vue";
import { useAuthStore } from "@/stores/authStore";
import { storeToRefs } from "pinia";

const authStore = useAuthStore();
const { user } = storeToRefs(authStore);
</script>

<style lang="scss" scoped>
.wrapper {
	padding-bottom: 32px;
	.content-container {
		position: relative;
		max-width: 1200px;
		margin: 0 auto;
		margin-bottom: 20px;
		padding: 16px;
		background-color: $background-color;
		border-radius: 25px;
	}
}
</style>
