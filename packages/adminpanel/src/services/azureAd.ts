// https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-js-initializing-client-applications
import config from "../config";
import {
  PublicClientApplication,
  InteractionRequiredAuthError,
} from "@azure/msal-browser";

const authParams = {
  scopes: [config.ACCESS_SCOPE],
};

const msalInstance = new PublicClientApplication({
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
});

export async function login() {
  return msalInstance.loginPopup(authParams);
}

export function logout() {
  return msalInstance.logout();
}

export function getAllAccounts() {
  const accounts = msalInstance.getAllAccounts();
  return accounts;
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
  if (getAllAccounts()) {
    const res = await getTokenRedirect();
    return res ? res.accessToken : "";
  } else {
    return config.TEST_ACCESS_TOKEN;
  }
}

export function requireLogin() {
  return !getAllAccounts() && process.env.NODE_ENV !== "development";
}

async function getTokenRedirect() {
  try {
    return msalInstance.ssoSilent(authParams);
  } catch (err) {
    console.log(
      "silent token acquisition fails. acquiring token using redirect"
    );
    if (err instanceof InteractionRequiredAuthError) {
      return login();
    }
  }
}
