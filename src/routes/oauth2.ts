import express from "express";
import session from "express-session";
import jwt from "jwt-simple";
import passport from "passport";
import OAuth2Strategy from "passport-oauth2";
import config from "../config";
import env from "../environment";

const oauth2Router = express.Router();

interface Params {
  token_type: string;
  scope: string;
  expires_in: string;
  ext_expires_in: string;
  expires_on: string;
  not_before: string;
  resource: string;
  access_token: string;
  id_token: string;
}

interface Profile {
  provider: string;
}

interface User {
  aud: string;
  iss: string;
  iat: number;
  nbf: number;
  exp: number;
  amr: string[];
  ipaddr: string;
  name: string;
  oid: string;
  sub: string;
  tid: string;
  unique_name: string;
  upn: string;
  ver: string;
}

const azureAdBaseUrl = "https://login.windows.net/";
const authorizationURL =
  azureAdBaseUrl + config.TENANT_ID + "/oauth2/authorize";
const tokenURL = azureAdBaseUrl + config.TENANT_ID + "/oauth2/token";

const oauth2Options = {
  authorizationURL,
  tokenURL,
  clientID: env.AZURE_AD_CLIENT_ID,
  clientSecret: env.AZURE_AD_CLIENT_SECTRET,
  callbackURL: "http://18856e31.eu.ngrok.io/oauth2/cb",
};

function azureAdLoginSuccess(
  accessToken: string,
  refreshtoken: string,
  params: Params,
  profile: Profile,
  done: (error: Error | null, user: User) => void
) {
  const user = jwt.decode(params.id_token, "", true);
  console.log("accessToken: ", accessToken);
  console.log("refreshtoken: ", refreshtoken);
  console.log("params: ", params);
  console.log("profile: ", profile);
  console.log("user: ", user);
  done(null, user);
}

const azureAdStrategy = new OAuth2Strategy(oauth2Options, azureAdLoginSuccess);

passport.use("azureAd", azureAdStrategy);
passport.serializeUser((user, done) => done(null, user));
// passport.deserializeUser((user, done) => done(null, user));

oauth2Router.use(passport.initialize());
oauth2Router.use(passport.session());
oauth2Router.use(
  session({
    secret: "somesecret",
    resave: true,
    saveUninitialized: true,
  })
);

oauth2Router.get(
  "/azuread",
  function (req, _res, next) {
    console.log("req.session: ", req.session);
    req.session.slackToken = "hei";
    console.log("req.session: ", req.session);
    next();
  },
  passport.authenticate("azureAd", { successRedirect: "/" })
);

oauth2Router.get(
  "/cb",
  function (req, _res, next) {
    console.log("cb req.session: ", req.session);
    next();
  },
  passport.authenticate("azureAd", {
    successRedirect: "/",
    failureRedirect: "/login",
  }),
  function (_req, res) {
    res.redirect("/");
  }
);

export default oauth2Router;
