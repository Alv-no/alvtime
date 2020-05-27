import { capitalizeFirstLetter } from "./text";

describe("Should capitalize first letter in string", () => {
  it("Capitalizes first letter", () => {
    expect(capitalizeFirstLetter("hei")).toEqual("Hei");
  });
});
