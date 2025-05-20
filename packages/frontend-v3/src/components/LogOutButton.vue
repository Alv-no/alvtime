<template>
	<button @click="logOut">
		Log Out
	</button>
</template>

<script setup lang="ts">
import { useMsal } from "@/composables/useMsal";

const { instance } = useMsal();

const logOut = async () => {
	// Log out the user
	await instance.logoutRedirect();
};
</script>

<style scoped lang="scss">
button {
	background-color: transparent;
	color: $background-color;
	border: none;
	padding: 10px 20px;
	cursor: pointer;
	font-size: 16px;

	&:hover {
		background-color: $background-color;
		color: $primary-color;
	}
}
</style>
