<template>
	<AlvtimeButton
		iconRight
		@click="logOut"
	>
		Logg ut <FeatherIcon name="log-out" />
	</AlvtimeButton>
</template>

<script setup lang="ts">
import FeatherIcon from "./utils/FeatherIcon.vue";
import AlvtimeButton from "./utils/AlvtimeButton.vue";
import { useAuthStore } from "@/stores/authStore";

const { logout } = useAuthStore();

const logOut = async () => {
	logout();
};
</script>