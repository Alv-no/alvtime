import moment, { Moment } from "moment";
import easterCalculator from "date-easter";

export interface Holiday {
  date: Moment;
  description: string;
}

export interface NorwegianHolidays {
  isHoliday: (date: Moment) => boolean;
  getHoliday: (date: Moment) => Holiday | undefined;
}

export default function createNorwegianHolidays(years: number[]) {
  const holidays = years.reduce((acc: Holiday[], year: number) => {
    const holidays = createNorwegianHolidaysForYear(year);
    return [...acc, ...holidays];
  }, []);

  const state = {
    holidays,
  };

  return {
    ...createIsHolday(state),
    ...createGetHoliday(state),
  };
}

function createNorwegianHolidaysForYear(yearNumber: number) {
  const yearString = yearNumber.toString();
  const { year, month, day } = easterCalculator.gregorianEaster(yearNumber);
  const easter = moment({ year, month: month - 1, day });

  const norwegianHolidays = [
    { date: moment(yearString + "-01-01"), description: "Første nyttårsdag" },
    { date: easter.clone().add(-1, "weeks"), description: "Palmesøndag" },
    { date: easter.clone().add(-3, "days"), description: "Skjærtorsdag" },
    { date: easter.clone().add(-2, "days"), description: "Langfredag" },
    { date: easter.clone(), description: "Første påskedag" },
    { date: easter.clone().add(1, "days"), description: "Andre påskedag" },
    {
      date: moment(yearString + "-05-01"),
      description: "Arbeidernes dag",
    },
    { date: moment(yearString + "-05-17"), description: "Grunnlovsdag" },
    {
      date: easter.clone().add(39, "days"),
      description: "Kristi himmelfartsdag",
    },
    { date: easter.clone().add(49, "days"), description: "Første pinsedag" },
    { date: easter.clone().add(50, "days"), description: "Andre pinsedag" },
    { date: moment(yearString + "-12-24"), description: "Juleaften" },
    { date: moment(yearString + "-12-25"), description: "Første juledag" },
    { date: moment(yearString + "-12-26"), description: "Andre juledag" },
    { date: moment(yearString + "-12-31"), description: "Nyttårsaften" },
  ];

  return norwegianHolidays;
}

function createIsHolday(state: { holidays: Holiday[] }) {
  function isHoliday(date: Moment) {
    return state.holidays.some((holiday: Holiday) =>
      holiday.date.isSame(date, "day")
    );
  }
  return { isHoliday };
}

function createGetHoliday(state: { holidays: Holiday[] }) {
  function getHoliday(date: Moment) {
    return state.holidays.find((holiday: Holiday) =>
      holiday.date.isSame(date, "day")
    );
  }
  return { getHoliday };
}
