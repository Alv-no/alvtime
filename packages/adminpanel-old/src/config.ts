interface Config {
  API_HOST: string;
  CLIENT_ID: string;
  ACCESS_SCOPE: string;
  DATE_FORMAT: string;
  AUTHORITY: string;
  TENANT_ID: string;
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
