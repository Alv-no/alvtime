import { api } from "@/services/apiClient.ts";

export default {
  getAccessTokens: () => api.get("/api/user/ActiveAccessTokens"),
  createAccessToken: (data: { friendlyName: string }) => {
    return api.post("/api/user/AccessToken", data);
  },
  deleteAccessToken: (tokenId: number) => api.delete(`/api/user/AccessToken/${tokenId}`)
};