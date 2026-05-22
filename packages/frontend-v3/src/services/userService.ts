import { api } from "@/services/apiClient";
import type { UserProfile } from "@/types/UserProfile.ts";
import type { AxiosResponse } from "axios";

export default {
	getUserProfile: async (): Promise<AxiosResponse<UserProfile>> => api.get("/api/user/profile"),
};