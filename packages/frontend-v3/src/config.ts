interface Config {
  API_HOST: string;
  DATE_FORMAT: string;
  HOURS_IN_WORKDAY: number;
  AUTHORITY: string;
  TENANT_ID: string;
  CLIENT_ID: string;
  ACCESS_SCOPE: string;
  SLACK_TEAM_ID: string;
  SLACK_CHANNEL_ID: string;
  BASE_URL_ADMINPANEL: string;
}

let config = ({} as unknown) as Config;

var request = new XMLHttpRequest();
request.open("GET", "./config.json", false);
request.send(null);

if (request.status === 200 && request.responseText.length) {
  config = JSON.parse(request.responseText);
} else {
  throw "Unable to fetch config";
}

export default config;
