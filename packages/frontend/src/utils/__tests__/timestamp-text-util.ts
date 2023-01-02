import {
  mapTimestampToMothYearString,
  decimalToRoundedPercentage,
} from "../timestamp-text-util";

describe("mapTimeStampToMonthYearString", () => {
  it("should map month and year correctly", () => {
    expect(mapTimestampToMothYearString("2022-01-01T00:00:00")).toBe(
      "Januar 2022"
    );

    expect(mapTimestampToMothYearString("2022-12-01T00:00:00")).toBe(
      "Desember 2022"
    );
  });
});

describe("decimalToRoundedPercentage", () => {
  it("should make a value below zero into a nicely rounded number", () => {
    expect(decimalToRoundedPercentage(0.943)).toBe(94.3);
    expect(decimalToRoundedPercentage(0.9432)).toBe(94.3);
    expect(decimalToRoundedPercentage(0.9438)).toBe(94.4);
  });
});
