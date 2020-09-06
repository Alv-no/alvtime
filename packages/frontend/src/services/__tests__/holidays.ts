import createNorwegianHolidays from "@/services/holidays";
import moment from "moment";

const norskeHelligdager = [
  { date: moment("2020-01-01T00:00:00"), description: "Første nyttårsdag" },
  { date: moment("2020-04-05T00:00:00"), description: "Palmesøndag" },
  { date: moment("2020-04-09T00:00:00"), description: "Skjærtorsdag" },
  { date: moment("2020-04-10T00:00:00"), description: "Langfredag" },
  { date: moment("2020-04-12T00:00:00"), description: "Første påskedag" },
  { date: moment("2020-04-13T00:00:00"), description: "Andre påskedag" },
  { date: moment("2020-05-01T00:00:00"), description: "Offentlig høytidsdag" },
  { date: moment("2020-05-17T00:00:00"), description: "Grunnlovsdag" },
  { date: moment("2020-05-21T00:00:00"), description: "Kristi himmelfartsdag" },
  { date: moment("2020-05-31T00:00:00"), description: "Første pinsedag" },
  { date: moment("2020-06-01T00:00:00"), description: "Andre pinsedag" },
  { date: moment("2020-12-25T00:00:00"), description: "Første juledag" },
  { date: moment("2020-12-26T00:00:00"), description: "Andre juledag" },
  { date: moment("2021-01-01T00:00:00"), description: "Første nyttårsdag" },
  { date: moment("2021-03-28T00:00:00"), description: "Palmesøndag" },
  { date: moment("2021-04-01T00:00:00"), description: "Skjærtorsdag" },
  { date: moment("2021-04-02T00:00:00"), description: "Langfredag" },
  { date: moment("2021-04-04T00:00:00"), description: "Første påskedag" },
  { date: moment("2021-04-05T00:00:00"), description: "Andre påskedag" },
  { date: moment("2021-05-01T00:00:00"), description: "Offentlig høytidsdag" },
  { date: moment("2021-05-13T00:00:00"), description: "Kristi himmelfartsdag" },
  { date: moment("2021-05-17T00:00:00"), description: "Grunnlovsdag" },
  { date: moment("2021-05-23T00:00:00"), description: "Første pinsedag" },
  { date: moment("2021-05-24T00:00:00"), description: "Andre pinsedag" },
  { date: moment("2021-12-25T00:00:00"), description: "Første juledag" },
  { date: moment("2021-12-26T00:00:00"), description: "Andre juledag" },
  { date: moment("2022-01-01T00:00:00"), description: "Første nyttårsdag" },
  { date: moment("2022-04-10T00:00:00"), description: "Palmesøndag" },
  { date: moment("2022-04-14T00:00:00"), description: "Skjærtorsdag" },
  { date: moment("2022-04-15T00:00:00"), description: "Langfredag" },
  { date: moment("2022-04-17T00:00:00"), description: "Første påskedag" },
  { date: moment("2022-04-18T00:00:00"), description: "Andre påskedag" },
  { date: moment("2022-05-01T00:00:00"), description: "Offentlig høytidsdag" },
  { date: moment("2022-05-17T00:00:00"), description: "Grunnlovsdag" },
  { date: moment("2022-05-26T00:00:00"), description: "Kristi himmelfartsdag" },
  { date: moment("2022-06-05T00:00:00"), description: "Første pinsedag" },
  { date: moment("2022-06-06T00:00:00"), description: "Andre pinsedag" },
  { date: moment("2022-12-25T00:00:00"), description: "Første juledag" },
  { date: moment("2022-12-26T00:00:00"), description: "Andre juledag" },
];

const notHolidays = [{ date: moment("2020-12-27T00:00:00"), description: "" }];

describe("isHoliday", () => {
  const norwegianHolidays = createNorwegianHolidays([2020, 2021, 2022]);
  for (let holliday of norskeHelligdager) {
    it(
      "It returns true if date is " +
        holliday.description +
        " " +
        holliday.date.format("YYYY"),
      () => {
        expect(norwegianHolidays.isHoliday(holliday.date)).toBeTruthy();
      }
    );
  }

  it("It returns false if date is not a holiday", () => {
    for (let day of notHolidays) {
      expect(norwegianHolidays.isHoliday(day.date)).toBeFalsy();
    }
  });
});
