jest.mock("@/config.ts");
jest.mock("@/services/auth", () => ({
  addCallback: () => Promise.resolve(),
  getAccount: () => {},
  getAccountAsync: () => Promise.resolve(null)
}));

