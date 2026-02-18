import { defineStore } from "pinia";
import { ref } from "vue";
import accessTokenService from "@/services/accessTokenService.ts";

export type PersonalAccessToken = {
  id: number;
  friendlyName: string;
  expiryDate: Date;
}

export type CreatedTokenResponse = {
  token: string;
  expiryDate: Date;
}

export const useAccessTokenStore = defineStore('accessTokenStore', () => {
  const accessTokens = ref<PersonalAccessToken[]>([])

  const getAccessTokens = async () => {
    try {
      const response = await accessTokenService.getAccessTokens();
      accessTokens.value = response.data;
    } catch (error) {
      console.error("Failed to fetch access tokens:", error);
      throw error;
    }
  };

  const createAccessToken = async (friendlyName: string): Promise<CreatedTokenResponse | undefined> => {
    try {
      const response = await accessTokenService.createAccessToken({ friendlyName });
      await getAccessTokens();
      return response.data;
    } catch (error) {
      console.error("Failed to create access token:", error);
    }
  }

  const deleteAccessToken = async (id: number) => {
    try {
      await accessTokenService.deleteAccessToken(id);
      await getAccessTokens();
    } catch (error) {
      console.error("Failed to delete access token:", error);
    }
  }

  return { accessTokens, getAccessTokens, createAccessToken, deleteAccessToken }
})