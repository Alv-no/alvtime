<template>
  <div>
    <WeekHeader @backClick="onPrev" @forwardClick="onNext" />
    <swiper
      @slideChangeTransitionEnd="onSlideChangeTransitionEnd"
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
import { createWeek } from "@/mixins/date";

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
        initialSlide: 6,
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
      weeks: createThreeMonths(this.$store.state.activeDate),
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

      const dayOfWeek = this.$store.state.activeDate.weekday();
      this.$store.commit(
        "UPDATE_ACTVIE_DATE",
        // @ts-ignore
        this.weeks[this.swiper.activeIndex][dayOfWeek]
      );
    },
  },

  computed: {
    swiper() {
      // @ts-ignore
      return this.$refs.mySwiper.swiper;
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

function createThreeMonths(date: moment.Moment) {
  const future = Array.apply(null, Array(6)).map((n, i) => i);
  const past = Array.apply(null, Array(6))
    .map((n, i) => (i + 1) * -1)
    .reverse();
  return [...past, ...future]
    .map(n => date.clone().add(n, "week"))
    .map(createWeek);
}
</script>
