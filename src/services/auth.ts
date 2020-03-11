// https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-js-initializing-client-applications
import { UserAgentApplication } from "msal";

export const requiresInteraction = (errorMessage: string[]) => {
  if (!errorMessage || !errorMessage.length) {
    return false;
  }

  return (
    errorMessage.indexOf("consent_required") > -1 ||
    errorMessage.indexOf("interaction_required") > -1 ||
    errorMessage.indexOf("login_required") > -1
  );
};

export const adAuthenticatedFetch = async (
  url: string,
  paramOptions: RequestInit = { headers: {} }
) => {
  try {
    const accessTokenResponse = await acquireToken(GRAPH_REQUESTS.LOGIN);
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
  } catch (error) {
    console.error("Non-interactive error:", error.errorCode);
    throw error;
  }
};

export const msalApp = new UserAgentApplication({
  auth: {
    clientId: "e53a094c-d524-4e2b-8e03-c2f168924fd7",
    authority:
      "https://login.microsoftonline.com/76749190-4427-4b08-a3e4-161767dd1b73",
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

export const GRAPH_SCOPES = {
  OPENID: "openid",
  PROFILE: "profile",
  USER_READ: "User.Read",
};

export interface Scopes {
  scopes: string[];
}

export const GRAPH_REQUESTS = {
  LOGIN: {
    scopes: [GRAPH_SCOPES.OPENID, GRAPH_SCOPES.PROFILE, GRAPH_SCOPES.USER_READ],
  },
};

export const acquireToken = async (request: Scopes) => {
  try {
    // Call acquireTokenPopup (popup window) in case of acquireTokenSilent failure
    // due to consent or interaction required ONLY
    if (msalApp.getAccount()) {
      const accessToken = await msalApp.acquireTokenSilent(request);
      return accessToken;
    }
  } catch (error) {
    if (requiresInteraction(error.errorCode)) {
      return msalApp.acquireTokenPopup(request);
    } else {
      throw error;
    }
  }
};
