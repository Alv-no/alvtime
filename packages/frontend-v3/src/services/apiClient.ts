import axios from "axios";
import config from "@/config";
import { msalInstance } from "@/authConfig";
import { InteractionRequiredAuthError } from "@azure/msal-browser";

const getAccessToken = async () => {
	const accounts = msalInstance.getAllAccounts();
	if (accounts.length > 0) {
		const account = accounts[0];
		try{
			const response = await msalInstance.acquireTokenSilent({
				scopes: [config.ACCESS_SCOPE],
				account: account,
			});

			return response.accessToken;

		} catch (e) {
			if (e instanceof InteractionRequiredAuthError) {
				await msalInstance.loginPopup({
					scopes: [config.ACCESS_SCOPE],
					account: account,
				});
			}
		}
	} else {
		throw new Error("No accounts found");
	}
};

const token = await getAccessToken();

const api = axios.create({
	baseURL: config.API_HOST,
	headers: {
		"Authorization": `Bearer ${token}`
	}
});

export {
	api
};