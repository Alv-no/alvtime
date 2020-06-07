import createAlvtimeClient from "./client/index";
import env from "./environment";
export default createAlvtimeClient(env.ALVTIME_API_URI);
