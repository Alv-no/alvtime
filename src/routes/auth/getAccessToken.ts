import fetch from "node-fetch";
import config from "../../config";
import env from "../../environment";
import UserModel, { UserData } from "../../models/user";

interface RefreshAccessTokenRespons {
  token_type: string;
  scope: string;
  expires_in: number;
  ext_expires_in: number;
  access_token: string;
  refresh_token: string;
  id_token: string;
}

interface RefreshAccessTokenErrorRespons {
  error: string;
  error_description: string;
  error_codes: number[];
  timestamp: string;
  trace_id: string;
  correlation_id: string;
}

export default async function getAccessToken(userData: UserData) {
  let accessToken;
  const nowInSecounds = Math.floor(new Date().getTime() / 1000);
  const expiresOn = parseInt(userData.auth.expiresOn);
  const isExpired = nowInSecounds > expiresOn;

  if (isExpired) {
    const refreshTokenBody = await refreshAccessToken(userData);
    updateUserAuth(userData.slackUserID, refreshTokenBody);
    accessToken = refreshTokenBody.access_token;
  } else {
    accessToken = userData.auth.accessToken;
  }

  return accessToken;
}

async function refreshAccessToken(userData: UserData) {
  const body = {
    client_id: env.AZURE_AD_CLIENT_ID,
    scope: userData.auth.scope,
    grant_type: "refresh_token",
    client_secret: env.AZURE_AD_CLIENT_SECTRET,
    refresh_token: userData.auth.refreshToken,
  };

  const url = config.AUTHORITY + config.TENANT_ID + "/oauth2/v2.0/token";
  const headers = { "Content-Type": "application/x-www-form-urlencoded" };
  const formBody = createFormBody(body);

  const response = await fetch(url, {
    method: "POST",
    headers,
    body: formBody,
  });

  if (response.status !== 200) {
    throw response.statusText;
  }

  const json = ((await response.json()) as unknown) as RefreshAccessTokenRespons &
    RefreshAccessTokenErrorRespons;

  if (json.error) {
    throw response;
  }
  return json;
}

function updateUserAuth(
  slackUserID: string,
  refreshTokenBody: RefreshAccessTokenRespons
) {
  const nowInSecounds = Math.floor(new Date().getTime() / 1000);
  const auth = {
    tokenType: refreshTokenBody.token_type,
    scope: refreshTokenBody.scope,
    expiresIn: refreshTokenBody.expires_in,
    expiresOn: nowInSecounds + refreshTokenBody.expires_in,
    accessToken: refreshTokenBody.access_token,
    refreshToken: refreshTokenBody.refresh_token,
    idToken: refreshTokenBody.id_token,
  };
  UserModel.findOneAndUpdate(
    { _id: slackUserID },
    { $set: { auth, __enc_message: false } }
  )
    .then(() => console.log("Succesfully updated auth on user."))
    .catch((e) => console.error("Unable to replace user data: ", e));
}

function createFormBody(obj: { [key: string]: string | number }) {
  const formBody = [];
  for (const [key, value] of Object.entries(obj)) {
    const encodedKey = encodeURIComponent(key);
    const encodedValue = encodeURIComponent(value);
    formBody.push(encodedKey + "=" + encodedValue);
  }
  return formBody.join("&");
}
