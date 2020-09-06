// https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-js-initializing-client-applications
import config from "../config";
import createMsalApp from "./createMsalApp.js";
import { TokenResponse } from "@azure/msal-common";

const authParams = {
  scopes: [config.ACCESS_SCOPE],
};

const msalApp = createMsalApp({
  auth: {
    clientId: config.CLIENT_ID,
    authority: config.AUTHORITY + config.TENANT_ID,
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },

  system: {
    navigateFrameWait: 0,
  },
});

export async function login(): Promise<TokenResponse> {
  return msalApp.loginPopup(authParams);
}

export function logout() {
  return msalApp.logout();
}

export function getAccount() {
  return msalApp.getAccount();
}

export async function adAuthenticatedFetch(
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
}

async function getAccessToken() {
  if (getAccount()) {
    const res = await getTokenRedirect();
    return res ? res.accessToken : "";
  } else {
    return config.TEST_ACCESS_TOKEN;
  }
}

export function requireLogin() {
  return !getAccount() && process.env.NODE_ENV !== "development";
}

async function getTokenRedirect() {
  try {
    return await msalApp.acquireTokenSilent(authParams);
  } catch {
    console.log(
      "silent token acquisition fails. acquiring token using redirect"
    );
    return msalApp.acquireTokenRedirect(authParams);
  }
}
