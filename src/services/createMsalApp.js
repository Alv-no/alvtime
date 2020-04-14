// This is in its own js file because msal-browser is not properly typed yet
import { PublicClientApplication } from "@azure/msal-browser";
export default options => new PublicClientApplication(options);
