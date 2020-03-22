<template>
  <div>
    <WeekHeader @backClick="onPrev" @forwardClick="onNext" :week="week" />
    <swiper
      @slideChangeTransitionEnd="onSlideChangeTransitionEnd"
      @slideChange="onSlideChange"
      ref="mySwiper"
      :options="swiperOption"
    >
      <swiperSlide v-for="(week, index) in weeks" :key="index">
        <TimeEntrieWeekList :week="week" />
      </swiperSlide>
    </swiper>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import moment from "moment";
import config from "@/config";
import { swiper, swiperSlide } from "vue-awesome-swiper";
import TimeEntrieWeekList from "./TimeEntrieWeekList.vue";
import WeekHeader from "./WeekHeader.vue";
import isInIframe from "@/mixins/isInIframe";

export default Vue.extend({
  components: {
    swiper,
    swiperSlide,
    TimeEntrieWeekList,
    WeekHeader,
  },

  data() {
    return {
      swiperOption: {
        initialSlide: this.$store.state.weekActiveSlideIndex,
        shortSwipes: false,
        simulateTouch: false,
        noSwipingSelector: "input, button",
        longSwipesRatio: 0.15,
        longSwipesMs: 100,
        keyboard: {
          enabled: true,
          onlyInViewport: false,
        },
      },
      weeks: createThreeMonths(),
      swiperObject: null,
    };
  },

  created() {
    // @ts-ignore
    if (!isInIframe()) {
      // @ts-ignore
      this.$store.dispatch("FETCH_TIME_ENTRIES", this.dateRange);
    }
  },

  methods: {
    onSlideChange() {
      // @ts-ignore
      this.$store.commit("UPDATE_ACTVIE_SLIDE_INDEX", this.swiper.activeIndex);
    },

    onNext() {
      // @ts-ignore
      this.swiper.slideNext();
    },

    onPrev() {
      // @ts-ignore
      this.swiper.slidePrev();
    },

    appendSlides() {
      // @ts-ignore
      const firstDayInLastWeek = this.weeks[this.weeks.length - 1][0];
      const firstDayInFirstNewWeek = firstDayInLastWeek.clone().add(1, "week");
      const newWeeks = createFourWeeksFromDate(firstDayInFirstNewWeek);
      // @ts-ignore
      this.weeks = [...this.weeks.slice(4), ...newWeeks];
      // @ts-ignore
      this.swiper.slideTo(this.swiper.realIndex - 4, 0);
      // @ts-ignore
      this.$store.dispatch("FETCH_TIME_ENTRIES", this.dateRange);
    },

    prependSlides() {
      // @ts-ignore
      const firstDayInFirstWeek = this.weeks[0][0];
      const firstDayInFirstNewWeek = firstDayInFirstWeek
        .clone()
        .add(-4, "week");
      const newWeeks = createFourWeeksFromDate(firstDayInFirstNewWeek);
      // @ts-ignore
      this.weeks = [...newWeeks, ...this.weeks.slice(0, 8)];
      // @ts-ignore
      this.swiper.slideTo(this.swiper.realIndex + 4, 0);
      // @ts-ignore
      this.$store.dispatch("FETCH_TIME_ENTRIES", this.dateRange);
    },

    onSlideChangeTransitionEnd() {
      // @ts-ignore
      if (this.swiper.activeIndex > 8) {
        // @ts-ignore
        this.appendSlides();
      }
      // @ts-ignore
      if (this.swiper.activeIndex < 4) {
        // @ts-ignore
        this.prependSlides();
      }
    },
  },

  computed: {
    swiper() {
      // @ts-ignore
      return this.$refs.mySwiper.swiper;
    },

    week() {
      // @ts-ignore
      if (this.weeks.length) {
        // @ts-ignore
        return this.weeks[this.$store.state.weekActiveSlideIndex];
      }
      return "";
    },

    dateRange() {
      return {
        // @ts-ignore
        fromDateInclusive: this.weeks[0][0].format(config.DATE_FORMAT),
        // @ts-ignore
        toDateInclusive: this.weeks[this.weeks.length - 1][6].format(
          config.DATE_FORMAT
        ),
      };
    },
  },
});

function createFourWeeksFromDate(date: moment.Moment) {
  return Array.apply(null, Array(4))
    .map((n, i) => date.clone().add(i, "week"))
    .map(createWeek);
}

function createThreeMonths() {
  const future = Array.apply(null, Array(6)).map((n, i) => i);
  const past = Array.apply(null, Array(6))
    .map((n, i) => (i + 1) * -1)
    .reverse();
  return [...past, ...future].map(n => moment().add(n, "week")).map(createWeek);
}

function createWeek(day: moment.Moment) {
  const monday = day.startOf("week");
  return [0, 1, 2, 3, 4, 5, 6].map(n => monday.clone().add(n, "day"));
}
</script>
