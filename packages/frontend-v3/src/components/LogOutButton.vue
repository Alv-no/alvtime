<template>
	<button @click="logOut">
		Logg ut
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
	padding: 10px 10px;
	cursor: pointer;
	font-size: 14px;

	&:hover {
		background-color: $background-color;
		color: $primary-color;
	}
}
</style>
