import createAlvtimeClient from "./index";
import env from "../environment";
import config from "../config";

describe("getTasks", () => {
  it("Should return tasks", async () => {
    const client = createAlvtimeClient(env.ALVTIME_API_URI);
    const result = await client.getTasks(config.TEST_ACCESS_TOKEN);
    expect(result).toBeTruthy();
  });
});

describe("getTasks", () => {
  it("Should return timeEntries", async () => {
    const client = createAlvtimeClient(env.ALVTIME_API_URI);
    const dateRange = {
      fromDateInclusive: "2019-01-01",
      toDateInclusive: "2020-03-01",
    };
    const result = await client.getTimeEntries(
      dateRange,
      config.TEST_ACCESS_TOKEN
    );
    expect(result).toBeTruthy();
  });
});
