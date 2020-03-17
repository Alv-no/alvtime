import { getAccount } from "../services/auth";

export default {
  state: {
    account: getAccount(),
  },
};
