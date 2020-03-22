<template>
  <div>
    <MobileHeader />
    <swiper
      @slideChangeTransitionEnd="onSlideChangeTransitionEnd"
      :options="swiperOption"
      ref="mySwiper"
    >
      <swiperSlide v-for="(date, index) in dates" :key="index">
        <TimeEntrieDayList :date="date" />
      </swiperSlide>
    </swiper>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import moment from "moment";
import config from "@/config";
import { swiper, swiperSlide } from "vue-awesome-swiper";
import TimeEntrieDayList from "./TimeEntrieDayList.vue";
import MobileHeader from "./MobileHeader.vue";
import isInIframe from "@/mixins/isInIframe";

export default Vue.extend({
  components: {
    swiper,
    swiperSlide,
    TimeEntrieDayList,
    MobileHeader,
  },

  data() {
    return {
      swiperOption: {
        initialSlide: 10,
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
      // @ts-ignore
      dates: this.create21Dates(),
    };
  },

  created() {
    // @ts-ignore
    if (!isInIframe()) {
      // @ts-ignore
      this.$store.dispatch("FETCH_TIME_ENTRIES", this.dateRange);
    }
  },

  computed: {
    swiper() {
      // @ts-ignore
      return this.$refs.mySwiper.swiper;
    },

    dateRange() {
      return {
        // @ts-ignore
        fromDateInclusive: this.dates[0].format(config.DATE_FORMAT),
        // @ts-ignore
        toDateInclusive: this.dates[this.dates.length - 1].format(
          config.DATE_FORMAT
        ),
      };
    },
  },

  methods: {
    appendSlides() {
      // @ts-ignore
      const lastDate = this.dates[this.dates.length - 1];
      const nextDate = lastDate.clone().add(1, "day");
      const nextDates = createSevenDates(nextDate);
      // @ts-ignore
      this.dates = [...this.dates.slice(7), ...nextDates];
      // @ts-ignore
      this.swiper.slideTo(this.swiper.realIndex - 7, 0);
      // @ts-ignore
      this.$store.dispatch("FETCH_TIME_ENTRIES", this.dateRange);
    },

    prependSlides() {
      // @ts-ignore
      const firstDate = this.dates[0];
      const firstNewDate = firstDate.clone().add(-7, "day");
      const newDates = createSevenDates(firstNewDate);
      // @ts-ignore
      this.dates = [...newDates, ...this.dates.slice(0, 14)];
      // @ts-ignore
      this.swiper.slideTo(this.swiper.realIndex + 7, 0);
      // @ts-ignore
      this.$store.dispatch("FETCH_TIME_ENTRIES", this.dateRange);
    },

    onSlideChangeTransitionEnd() {
      // @ts-ignore
      if (this.swiper.activeIndex > 16) {
        // @ts-ignore
        this.appendSlides();
      }
      // @ts-ignore
      if (this.swiper.activeIndex < 5) {
        // @ts-ignore
        this.prependSlides();
      }
      this.$store.commit(
        "UPDATE_ACTVIE_DATE",
        // @ts-ignore
        this.dates[this.swiper.activeIndex]
      );
    },

    create21Dates() {
      const date = this.$store.state.activeDate;
      const future = Array.apply(null, Array(11)).map((n, i) => i);
      const past = Array.apply(null, Array(10))
        .map((n, i) => (i + 1) * -1)
        .reverse();
      return [...past, ...future].map(n => date.clone().add(n, "day"));
    },
  },
});

function createSevenDates(date: moment.Moment) {
  return Array.apply(null, Array(7)).map((n, i) => date.clone().add(i, "day"));
}
</script>
