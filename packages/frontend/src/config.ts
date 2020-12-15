export default Object.freeze({
  API_HOST: process.env.VUE_APP_API_HOST,
  DATE_FORMAT: "YYYY-MM-DD",
  HOURS_IN_WORKDAY: 7.5,
  AUTHORITY: "https://login.microsoftonline.com/",
  TENANT_ID: "76749190-4427-4b08-a3e4-161767dd1b73",
  CLIENT_ID: process.env.VUE_APP_CLIENT_ID,
  ACCESS_SCOPE: process.env.VUE_APP_ACCESS_SCOPE,
  SLACK_TEAM_ID: "TJ70QLJRL",
  SLACK_CHANNEL_ID: "CNEHG527J",
  TEST_ACCESS_TOKEN: "5801gj90-jf39-5j30-fjk3-480fj39kl409",
  BASE_URL_DEV: "http://localhost:3000",
  BASE_URL_ADMINPANEL_PROD: "https://alvtime-admin-react-pwa-as.azurewebsites.net",
});
