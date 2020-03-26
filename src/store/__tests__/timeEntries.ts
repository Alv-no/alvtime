import store from "@/store";

describe("UPDATE_TIME_ENTRIES", () => {
  it("Adds timentries to map", () => {
    store.commit("UPDATE_TIME_ENTRIES", [
      {
        id: 1,
        date: "2020-02-25",
        value: "7.5",
        taskId: 17,
      },
    ]);

    expect(store.state.timeEntriesMap).toEqual({
      "2020-02-25": { 17: { value: "7.5", id: 1 } },
    });
    expect(store.state.timeEntries).toEqual([
      {
        id: 1,
        date: "2020-02-25",
        value: "7.5",
        taskId: 17,
      },
    ]);
  });

  it("Updates timentries in map", () => {
    store.commit("UPDATE_TIME_ENTRIES", [
      {
        id: 1,
        date: "2020-02-25",
        value: "7.5",
        taskId: 17,
      },
    ]);
    store.commit("UPDATE_TIME_ENTRIES", [
      {
        id: 1,
        date: "2020-02-25",
        value: "5",
        taskId: 17,
      },
    ]);

    expect(store.state.timeEntriesMap).toEqual({
      "2020-02-25": { 17: { value: "5", id: 1 } },
    });
    expect(store.state.timeEntries).toEqual([
      {
        id: 1,
        date: "2020-02-25",
        value: "5",
        taskId: 17,
      },
    ]);
  });
});
