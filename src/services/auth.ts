// https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-js-initializing-client-applications
import {
  AuthResponse,
  AuthError,
  UserAgentApplication,
  AuthenticationParameters,
} from "msal";
import config from "@/config";

const authParams: AuthenticationParameters = {
  scopes: [config.ACCESS_SCOPE],
};

const msalApp = new UserAgentApplication({
  auth: {
    clientId: config.CLIENT_ID,
    authority: config.AUTHORITY + config.TENANT_ID,
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
  },
  cache: {
    storeAuthStateInCookie: false,
  },

  system: {
    navigateFrameWait: 0,
  },
});

function createRedirectCallback(cb?: (error: Error) => void) {
  function redirectCallback(error: any, response: any) {
    // https://github.com/AzureAD/microsoft-authentication-library-for-js/wiki/Known-issue-on-Safari
    const isSafari = navigator.userAgent.toLowerCase().indexOf("safari") > -1;
    const isIdToken = response && response.tokenType == "id_token";
    if (isSafari && !error && isIdToken) {
      msalApp.acquireTokenRedirect(authParams);
    }

    if (error) {
      const errorMessage = error.errorMessage
        ? error.errorMessage
        : "Unable to acquire access token.";
      console.error(errorMessage);
      if (cb) cb(Error(errorMessage));
    }
  }

  return redirectCallback;
}

msalApp.handleRedirectCallback(createRedirectCallback());

export function setRedirectCallback(cb: (error: Error) => void) {
  msalApp.handleRedirectCallback(createRedirectCallback(cb));
}

export function login() {
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
  const accessTokenResponse = await acquireToken();
  const authHeaders =
    accessTokenResponse && accessTokenResponse.accessToken
      ? { Authorization: `Bearer ${accessTokenResponse.accessToken}` }
      : { Authorization: "" };

  var options = {
    ...paramOptions,
    headers: {
      ...paramOptions.headers,
      ...authHeaders,
    },
  };

  return fetch(url, options);
}

function acquireToken() {
  try {
    // Call acquireTokenPopup (popup window) in case of acquireTokenSilent failure
    // due to consent or interaction required ONLY
    return msalApp.acquireTokenSilent(authParams);
  } catch (error) {
    if (requiresInteraction(error.errorCode)) {
      return msalApp.acquireTokenRedirect(authParams);
    } else {
      throw error;
    }
  }
}

function requiresInteraction(errorMessage: string[]) {
  if (!errorMessage || !errorMessage.length) {
    return false;
  }

  return (
    errorMessage.indexOf("consent_required") > -1 ||
    errorMessage.indexOf("interaction_required") > -1 ||
    errorMessage.indexOf("login_required") > -1
  );
}
