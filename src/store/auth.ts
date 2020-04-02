import { getAccount } from "../services/auth";

export interface AuthState {
  account: Account | null;
}

interface Account {
  name: string;
}

const state = {
  account: getAccount(),
};

export default { state };
