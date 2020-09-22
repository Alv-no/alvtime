// https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-js-initializing-client-applications
import config from "../config";
import {
  PublicClientApplication,
  InteractionRequiredAuthError,
  AccountInfo,
} from "@azure/msal-browser";

const request = {
  scopes: [config.ACCESS_SCOPE],
};

const msalInstance = new PublicClientApplication({
  auth: {
    clientId: config.CLIENT_ID,
    authority: config.AUTHORITY + config.TENANT_ID,
    redirectUri: window.location.origin + "/adminpanel",
    postLogoutRedirectUri: window.location.origin + "/adminpanel",
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
});

export function acquireTokenSilent(account: AccountInfo) {
  const silentRequest = {
    ...request,
    forceRefresh: false,
    account,
  };
  return msalInstance.acquireTokenSilent(silentRequest);
}

export function handleRedirect() {
  return msalInstance.handleRedirectPromise();
}

export function login() {
  msalInstance.loginRedirect(request);
}

export function logout() {
  return msalInstance.logout();
}

export function getAllAccounts() {
  const accounts = msalInstance.getAllAccounts();
  return accounts;
}

export function getAccountByHomeId(homeAccountId: string) {
  return msalInstance.getAccountByHomeId(homeAccountId);
}

export function createAdAuthenticatedFetch(account: AccountInfo) {
  async function getAccessToken() {
    const res = await getTokenRedirect();
    return res ? res.accessToken : "";
  }

  async function getTokenRedirect() {
    try {
      return acquireTokenSilent(account);
    } catch (err) {
      console.log(
        "silent token acquisition fails. acquiring token using redirect"
      );
      if (err instanceof InteractionRequiredAuthError) {
        return msalInstance.acquireTokenRedirect(request);
      }
    }
  }

  return async function (
    url: string,
    paramOptions: RequestInit = { headers: {} }
  ) {
    const accessToken = await getAccessToken();
    const authHeaders = { Authorization: `Bearer ${accessToken}` };

    paramOptions = paramOptions ? paramOptions : { headers: {} };

    var options = {
      ...paramOptions,
      headers: {
        ...paramOptions.headers,
        ...authHeaders,
      },
    };

    return fetch(url, options);
  };
}
