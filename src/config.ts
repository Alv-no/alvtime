export default Object.freeze({
  HOST: process.env.VUE_APP_HOST,
  DATE_FORMAT: "YYYY-MM-DD",
  HOURS_IN_WORKDAY: 7.5,
  AUTHORITY: "https://login.microsoftonline.com/",
  TENANT_ID: "76749190-4427-4b08-a3e4-161767dd1b73",
  CLIENT_ID: process.env.VUE_APP_CLIENT_ID,
  ACCESS_SCOPE: process.env.VUE_APP_ACCESS_SCOPE,
});
