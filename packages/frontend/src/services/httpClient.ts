import axios from "axios";
import config from "@/config";
import authenticationService from "./auth";

const protectedUrls = [config.API_HOST].map(url => {
  if (url) {
    const parsed = new URL(url);
    return `${parsed.protocol}//${parsed.hostname}:${parsed.port}`;
  }
  return "";
});

const isProtectedUrl: (url: string | undefined) => boolean = (
  url: string | undefined
) => {
  if (!url) return false;

  const parsedUrl = new URL(url);

  const complete = `${parsedUrl.protocol}//${parsedUrl.hostname}:${parsedUrl.port}`;
  return protectedUrls.some(item => item === complete);
};

// Define an interceptor that only adds an access-token whenever we target our API's
axios.interceptors.request.use(request => {
  return new Promise(resolve => {
    request.headers["Content-Type"] = "application/json";
    if (isProtectedUrl(request.url)) {
      authenticationService.getAccessToken().then(token => {
        request.headers["Authorization"] = `Bearer ${token}`;
        resolve(request);
      });
    } else {
      resolve(request);
    }
  });
});

export default axios;
