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

export const fetchWithToken = async (url: string, accessToken: string) => {
  const response = await fetch(url, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  });

  return response.json();
};

export const msalApp = new UserAgentApplication({
  auth: {
    clientId: "e53a094c-d524-4e2b-8e03-c2f168924fd7",
    authority:
      "https://login.microsoftonline.com/76749190-4427-4b08-a3e4-161767dd1b73",
  },
  cache: {
    cacheLocation: "localStorage",
  },
  system: {
    navigateFrameWait: 0,
  },
});

export const GRAPH_SCOPES = {
  OPENID: "openid",
  PROFILE: "profile",
  USER_READ: "User.Read",
  MAIL_READ: "Mail.Read",
};

export const GRAPH_REQUESTS = {
  LOGIN: {
    scopes: [
      // GRAPH_SCOPES.OPENID,
      // GRAPH_SCOPES.PROFILE,
      GRAPH_SCOPES.USER_READ,
    ],
  },
  EMAIL: {
    scopes: [GRAPH_SCOPES.MAIL_READ],
  },
};
