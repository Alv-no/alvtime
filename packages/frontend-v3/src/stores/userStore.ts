import { defineStore } from "pinia";
import { ref } from "vue";
import type { UserProfile } from "@/types/UserProfile.ts";
import userService from "@/services/userService.ts";

export const useUserStore = defineStore("user", () => {
	const userProfile = ref<UserProfile | null>(null);

	const getUserProfile = async () => {
		try {
			const response = await userService.getUserProfile();
			if (response.status === 200) {
				userProfile.value = response.data as UserProfile;
			} else {
				console.error("Failed to fetch user profile:", response.statusText);
			}
		} catch (error) {
			console.error("Error fetching user profile:", error);
		}
	};

	return {
		userProfile,
		getUserProfile,
	};
});