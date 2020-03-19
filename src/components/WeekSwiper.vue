<template>
  <div>
    <WeekHeader @backClick="onPrev" @forwardClick="onNext" :week="week" />
    <swiper @slideChange="onSlideChange" ref="mySwiper" :options="swiperOption">
      <swiperSlide v-for="(week, index) in weeks" :key="index">
        <TimeEntrieWeekList :week="week" />
      </swiperSlide>
    </swiper>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import moment from "moment";

import { swiper, swiperSlide } from "vue-awesome-swiper";
import TimeEntrieWeekList from "./TimeEntrieWeekList.vue";
import WeekHeader from "./WeekHeader.vue";

export default Vue.extend({
  components: {
    swiper,
    swiperSlide,
    TimeEntrieWeekList,
    WeekHeader,
  },

  data() {
    const currentComponent = this;
    return {
      swiperOption: {
        initialSlide: 3,
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
      weeks: createWeeks(),
      swiperObject: null,
    };
  },

  methods: {
    onSlideChange() {
      // @ts-ignore
      this.$store.commit("UPDATE_ACTVIE_SLIDE", this.swiper.activeIndex);
    },
    onNext() {
      // @ts-ignore
      this.swiper.slideNext();
    },
    onPrev() {
      // @ts-ignore
      this.swiper.slidePrev();
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
        return this.weeks[this.$store.state.activeSlideIndex];
      }
      return "";
    },
  },
});

function createWeeks() {
  return [-3, -2, -1, 0, 1, 2, 3]
    .map(n => moment().add(n, "week"))
    .map(createWeek);
}

function createWeek(day: moment.Moment) {
  const monday = day.startOf("week");
  return [0, 1, 2, 3, 4, 5, 6].map(n => monday.clone().add(n, "day"));
}
</script>
