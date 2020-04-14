import { State } from "./index";
import { ActionContext } from "vuex";
import config from "@/config";
import moment, { Moment } from "moment";
import Swiper from "swiper";
import isInIframe from "@/mixins/isInIframe";
import { createWeek } from "@/mixins/date";
moment.locale("nb");

export interface SwiperState {
  swiper: Swiper;
  weeks: Moment[][];
  dates: Moment[];
}

export const GLOBAL_SWIPER_OPTIONS = {
  shortSwipes: false,
  simulateTouch: false,
  noSwipingSelector: "input, button",
  longSwipesRatio: 0.15,
  longSwipesMs: 100,
  keyboard: {
    enabled: true,
    onlyInViewport: false,
  },
};

const state = {
  swiper: {} as Swiper,
  weeks: [[]] as Moment[][],
  dates: [] as moment.Moment[],
};

const getters = {
  radiusOfWeeks(): number {
    return 52;
  },

  radiusOfDays(_state: State, { radiusOfWeeks }: { radiusOfWeeks: number }) {
    return 7 * radiusOfWeeks;
  },

  initialDaySlide(_state: State, { radiusOfDays }: { radiusOfDays: number }) {
    return radiusOfDays;
  },

  weeksDateRange(
    state: State
  ): { fromDateInclusive: string; toDateInclusive: string } | undefined {
    if (!state.weeks[0][0]) return;
    const firstDateInWeeks = state.weeks[0][0];
    const lastDateInWeeks = state.weeks[state.weeks.length - 1][6];
    const fromDateInclusive = firstDateInWeeks.format(config.DATE_FORMAT);
    const toDateInclusive = lastDateInWeeks.format(config.DATE_FORMAT);
    return { fromDateInclusive, toDateInclusive };
  },

  datesDateRange(
    state: State
  ): { fromDateInclusive: string; toDateInclusive: string } | undefined {
    const firstDate = state.dates[0];
    const lastDate = state.dates[state.dates.length - 1];
    const fromDateInclusive = firstDate.format(config.DATE_FORMAT);
    const toDateInclusive = lastDate.format(config.DATE_FORMAT);
    return { fromDateInclusive, toDateInclusive };
  },
};

const mutations = {
  SET_SWIPER(state: State, swiper: Swiper) {
    state.swiper = swiper;
  },

  SLIDE_NEXT(state: State) {
    state.swiper.slideNext();
  },

  SLIDE_PREV(state: State) {
    state.swiper.slidePrev();
  },

  SLIDE_TO_THIS_WEEK(state: State) {
    const weeks = state.swiper.virtual ? state.swiper.virtual.slides : [];
    const todayIndex = weeks.findIndex(findDateInWeek(moment()));
    if (todayIndex === -1) return;
    state.swiper.slideTo(todayIndex);
  },

  SLIDE_TO_TODAY(state: State) {
    const days = state.swiper.virtual ? state.swiper.virtual.slides : [];
    const todayIndex = days.findIndex(findDayInDates(moment()));
    if (todayIndex === -1) return;
    state.swiper.slideTo(todayIndex);
  },

  UPDATE_ACTVIE_DATE(state: State, date: moment.Moment) {
    state.activeDate = date;
  },

  UPDATE_ACTVIE_DATE_IN_WEEKS(state: State) {
    if (!state.swiper.activeIndex) return;
    const dayOfWeek = state.activeDate.weekday();
    const week = state.weeks[state.swiper.activeIndex];
    const date = week ? week[dayOfWeek] : moment();
    state.activeDate = date;
  },

  UPDATE_ACTVIE_DATE_IN_DATES(state: State) {
    if (!state.swiper.activeIndex) return;
    const activeDate = state.dates[state.swiper.activeIndex];
    const date = activeDate ? activeDate : moment();
    state.activeDate = date;
  },

  SET_WEEKS(state: State, weeks: Moment[][]) {
    state.weeks = weeks;
  },

  SET_DATES(state: State, dates: Moment[]) {
    state.dates = dates;
  },
};

const actions = {
  CREATE_WEEKS({ state, commit, getters }: ActionContext<State, State>) {
    const centerDate = state.activeDate;
    const radiusOfWeeks = getters.radiusOfWeeks;
    const weeks = createWeeksAround(centerDate, radiusOfWeeks);
    commit("SET_WEEKS", weeks);
  },

  CREATE_DATES({ state, commit, getters }: ActionContext<State, State>) {
    const centerDate = state.activeDate;
    const radiusOfDays = getters.radiusOfDays;
    const dates = createDatesAround(centerDate, radiusOfDays);
    commit("SET_DATES", dates);
  },

  FETCH_WEEK_ENTRIES({ dispatch, getters }: ActionContext<State, State>) {
    const weeksDateRange = getters.weeksDateRange;
    if (!isInIframe() && weeksDateRange) {
      dispatch("FETCH_TIME_ENTRIES", weeksDateRange);
    }
  },

  FETCH_DATE_ENTRIES({ dispatch, getters }: ActionContext<State, State>) {
    const datesDateRange = getters.datesDateRange;
    if (!isInIframe() && datesDateRange) {
      dispatch("FETCH_TIME_ENTRIES", datesDateRange);
    }
  },
};

function findDateInWeek(date: Moment): (week: Moment[]) => boolean {
  const isEqualDates = findDayInDates(date);
  return function findDateInWeek(week: Moment[]) {
    return week.some(isEqualDates);
  };
}

function findDayInDates(date: Moment): (day: Moment) => boolean {
  const format = config.DATE_FORMAT;
  const dateString = date.format(format);
  return function findDayInDates(day: Moment) {
    return day.format(format) === dateString;
  };
}

function createWeeksAround(centerDate: Moment, radiusOfWeeks: number) {
  const numbers = [
    ...descendingNumbers(radiusOfWeeks),
    0,
    ...ascendingNumbers(radiusOfWeeks),
  ];
  const weeks = numbers
    .map(n => centerDate.clone().add(n, "week"))
    .map(createWeek);
  return weeks;
}

function createDatesAround(
  date: Moment,
  radiusOfDays: number
): moment.Moment[] {
  const numbers = [
    ...descendingNumbers(radiusOfDays),
    0,
    ...ascendingNumbers(radiusOfDays),
  ];
  const dates = numbers.map(n => date.clone().add(n, "day"));
  return dates;
}

function ascendingNumbers(length: number) {
  return createArrayOf(length, i => i + 1);
}

function descendingNumbers(length: number) {
  return createArrayOf(length, i => (i + 1) * -1).reverse();
}

function createArrayOf(length: number, mapFunc: (index: number) => any) {
  return Array.apply(null, Array(length)).map((_n, i) => mapFunc(i));
}

export default { state, getters, mutations, actions };
