// https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-js-initializing-client-applications
import config from "@/config";
import createMsalApp from "@/services/createMsalApp.js";

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

function createRedirectCallback(
  cb?: (error: Error) => void,
  setAccount?: (account: Account) => void
) {
  function redirectCallback(error: any) {
    if (error) {
      if (cb) cb(error);
    } else {
      if (msalApp.getAccount()) {
        if (setAccount) setAccount(msalApp.getAccount());
      }
    }
  }

  return redirectCallback;
}

msalApp.handleRedirectCallback(createRedirectCallback());

export function setRedirectCallback(
  cb: (error: Error) => void,
  setAccount: (account: Account) => void
) {
  msalApp.handleRedirectCallback(createRedirectCallback(cb, setAccount));
}

export async function login() {
  msalApp.loginRedirect(authParams);
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
  const accessTokenResponse = await getTokenRedirect();
  const authHeaders =
    accessTokenResponse && accessTokenResponse.accessToken
      ? { Authorization: `Bearer ${accessTokenResponse.accessToken}` }
      : { Authorization: "" };

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

async function getTokenRedirect() {
  return await msalApp.acquireTokenSilent(authParams).catch(() => {
    console.log(
      "silent token acquisition fails. acquiring token using redirect"
    );
    // fallback to interaction when silent call fails
    return msalApp.acquireTokenRedirect(authParams);
  });
}

async function acquireToken() {
  try {
    // Call acquireTokenPopup (popup window) in case of acquireTokenSilent failure
    // due to consent or interaction required ONLY
    return await msalApp.acquireTokenSilent(authParams);
  } catch (error) {
    return await msalApp.acquireTokenPopup(authParams);
  }
}
