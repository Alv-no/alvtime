import { registerHoursReminderMessage } from "./index";

const userReport = {
  sum: 33,
  entries: [
    {
      date: "2020-06-22",
      value: 0.0,
      taskId: 24,
    },
    {
      date: "2020-06-23",
      value: 8.5,
      taskId: 21,
    },
    {
      date: "2020-06-24",
      value: 7.75,
      taskId: 21,
    },
    {
      date: "2020-06-25",
      value: 9.25,
      taskId: 21,
    },
    {
      date: "2020-06-25",
      value: 0.0,
      taskId: 25,
    },
    {
      date: "2020-06-26",
      value: 7.5,
      taskId: 21,
    },
    {
      date: "2020-06-26",
      value: 0.0,
      taskId: 25,
    },
  ],
};

const tasks = [
  {
    id: 21,
    name: "Senior Utvikler",
    description: "",
    favorite: false,
    locked: false,
    compensationRate: 1.0,
    project: {
      id: 9,
      name: "Penger",
      customer: {
        id: 6,
        name: "Navn",
        invoiceAddress: "",
        contactPerson: "",
        contactEmail: "",
        contactPhone: "",
      },
    },
  },
  {
    id: 25,
    name: "UbetaltFerie",
    description: "",
    favorite: false,
    locked: false,
    compensationRate: 1.0,
    project: {
      id: 8,
      name: "Lars AS",
      customer: {
        id: 8,
        name: "Lars AS",
        invoiceAddress: "",
        contactPerson: "",
        contactEmail: "",
        contactPhone: "",
      },
    },
  },
];

describe("registerHoursReminderMessage", () => {
  it.only("Should return message", async () => {
    const message = registerHoursReminderMessage("userID", userReport, tasks);

    expect(message.blocks).toHaveLength(10);
  });
});
