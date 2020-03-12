import { State } from "./index";
import router from "../router";
import { logout, login, getAccount } from "../services/auth";

export default {
  state: {
    account: getAccount(),
  },
};
