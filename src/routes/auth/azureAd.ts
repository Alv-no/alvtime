import jwt from "jwt-simple";
import OAuth2Strategy from "passport-oauth2";
import config from "../../config";
import env from "../../environment";

interface Params {
  token_type: "Bearer";
  scope: string;
  expires_in: number;
  ext_expires_in: number;
  access_token: string;
  id_token: string;
  refreshToken: string;
}

interface Profile {
  provider: string;
}

interface DecodedIdToken {
  aud: string;
  iss: string;
  iat: number;
  nbf: number;
  exp: number;
  aio: string;
  email: string;
  sub: string;
  tid: string;
  uti: string;
  ver: string;
}

interface DecodedAccessToken {
  aud: string;
  iss: string;
  iat: number;
  nbf: number;
  exp: number;
  aio: string;
  azp: string;
  azpacr: string;
  name: string;
  oid: string;
  preferred_username: string;
  scp: string;
  sub: string;
  tid: string;
  uti: string;
  ver: string;
}

export interface AuthenticatedUser {
  name: string;
  email: string;
  auth: {
    tokenType: string;
    scope: string;
    expiresOn: string;
    expiresIn: number;
    extExpiresIn: number;
    accessToken: string;
    idToken: string;
  };
}

export type DoneFunc = (error: Error | null, user: any) => void;

const azureAdBaseUrl = "https://login.microsoftonline.com/";
const authorizationURL =
  azureAdBaseUrl +
  config.TENANT_ID +
  "/oauth2/v2.0/authorize?prompt=select_account";
const tokenURL = azureAdBaseUrl + config.TENANT_ID + "/oauth2/v2.0/token";
const scope = [
  "openid",
  "offline_access",
  "https://d8aedd.alvtime-ap-alvtime-api-dev-f9b4e4.westeurope.cloudapp.azure.com/access_as_user",
].join(" ");

const oauth2Options = {
  scope,
  authorizationURL,
  tokenURL,
  clientID: env.AZURE_AD_CLIENT_ID,
  clientSecret: env.AZURE_AD_CLIENT_SECTRET,
  callbackURL: env.HOST + "/oauth2/cb",
};

export default new OAuth2Strategy(oauth2Options, azureAdLoginSuccess);

function azureAdLoginSuccess(
  accessToken: string,
  refreshToken: string,
  params: Params,
  _profile: Profile,
  done: DoneFunc
) {
  const { name, preferred_username, exp } = jwt.decode(
    accessToken,
    "",
    true
  ) as DecodedAccessToken;
  done(null, {
    name,
    email: preferred_username,
    auth: {
      tokenType: params.token_type,
      scope: params.scope,
      expiresOn: exp,
      expiresIn: params.expires_in,
      extExpiresIn: params.ext_expires_in,
      accessToken: params.access_token,
      idToken: params.id_token,
      refreshToken,
    },
  });
}
