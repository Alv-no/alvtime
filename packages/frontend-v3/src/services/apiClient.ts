import axios from "axios";
import config from "@/config";
import { msalInstance } from "@/authConfig";

const getAccessToken = async () => {
	const accounts = msalInstance.getAllAccounts();
	if (accounts.length > 0) {
		const account = accounts[0];
			const response = await msalInstance.acquireTokenSilent({
				scopes: [config.ACCESS_SCOPE],
				account: account,
			});

			return response.accessToken;
	} else {
		throw new Error("No accounts found");
	}
};

const api = axios.create({
	baseURL: config.API_HOST,
});

api.interceptors.request.use(
    async (config) => {
        const token = await getAccessToken();
        config.headers.Authorization = `Bearer ${token}`;
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

export {
	api
};