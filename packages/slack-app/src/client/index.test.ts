import config from "../config";
import createAlvtimeClient, { RequestOptions } from "./index";

interface MockFetchResult {
  result: any;
  uri: string;
  init: RequestOptions;
}

function createMockFetch(result: any, status: number, statusText: string) {
  const mockFetch = async (uri: string, init: RequestOptions) => {
    return {
      status,
      statusText,
      async json() {
        return {
          result,
          uri,
          init,
        };
      },
    };
  };

  return mockFetch;
}

const mockPayload = "payload";
const mockHost = "https://url";
const mockDateRange = {
  fromDateInclusive: "2019-01-01",
  toDateInclusive: "2020-03-01",
};
const mockAccessToken = "super secret access token";

describe("getTasks", () => {
  it("Should return tasks", async () => {
    const mockFetch = createMockFetch(mockPayload, 200, "");
    const client = createAlvtimeClient(mockHost, mockFetch);

    const mockResult = (await client.getTasks(
      config.TEST_ACCESS_TOKEN
    )) as undefined as MockFetchResult;

    expect(mockResult.result).toEqual(mockPayload);
  });
});

describe("getTimeEntries", () => {
  describe("Happy path", () => {
    let mockResult: MockFetchResult;

    beforeEach(async () => {
      const mockFetch = createMockFetch(mockPayload, 200, "");
      const client = createAlvtimeClient(mockHost, mockFetch);

      mockResult = (await client.getTimeEntries(
        mockDateRange,
        mockAccessToken
      )) as undefined as MockFetchResult;
    });

    it("Should return timeEntries", async () => {
      expect(mockResult.result).toEqual(mockPayload);
    });

    it("Should use correct url", () => {
      expect(mockResult.uri).toEqual(
        `${mockHost}/api/user/TimeEntries?fromDateInclusive=${mockDateRange.fromDateInclusive}&toDateInclusive=${mockDateRange.toDateInclusive}`
      );
    });

    it("Should include access token", () => {
      expect(mockResult.init.headers["Authorization"]).toEqual(
        "Bearer " + mockAccessToken
      );
    });

    it("Should set Content-Type to application/json", () => {
      expect(mockResult.init.headers["Content-Type"]).toEqual(
        "application/json"
      );
    });
  });
});
