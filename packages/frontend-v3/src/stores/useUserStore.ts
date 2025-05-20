import { defineStore } from "pinia";
import { ref } from "vue";

export const useUserStore = defineStore("user", () => {
	const user: any = ref(null);

	const setUser = (userData: any) => {
		user.value = userData;
	};

	return { user, setUser };
});