<template
  ><div>
    <mq-layout mq="sm">
      <swiper :options="swiperOption" ref="mySwiper">
        <swiperSlide v-for="(date, index) in dates" :key="index">
          <TimeEntrieDayList :date="date" />
        </swiperSlide>
      </swiper>
    </mq-layout>
    <mq-layout mq="md+">
      <swiper :options="swiperOption" ref="mySwiper">
        <swiperSlide v-for="(week, index) in weeks" :key="index">
          <TimeEntrieWeekList :week="week" />
        </swiperSlide>
        <div class="swiper-button-prev" slot="button-prev"></div>
        <div class="swiper-button-next" slot="button-next"></div>
      </swiper>
    </mq-layout>
  </div>
</template>

<script>
import "swiper/dist/css/swiper.css";
import moment from "moment";

import { swiper, swiperSlide } from "vue-awesome-swiper";
import TimeEntrieDayList from "./TimeEntrieDayList";
import TimeEntrieWeekList from "./TimeEntrieWeekList";

export default {
  components: {
    swiper,
    swiperSlide,
    TimeEntrieDayList,
    TimeEntrieWeekList,
  },

  data() {
    return {
      swiperOption: {
        navigation: {
          nextEl: ".swiper-button-next",
          prevEl: ".swiper-button-prev",
        },
      },
      dates: [],
      weeks: [],
    };
  },

  computed: {
    swiper() {
      return this.$refs.mySwiper.swiper;
    },
  },

  mounted() {
    this.dates = createDays();
    this.weeks = createWeeks();
    this.swiper.slideTo(3, 1000, false);
  },
};

function createDays() {
  return [-3, -2, -1, 0, 1, 2, 3].map(n => moment().add(n, "day"));
}

function createWeeks() {
  return [-3, -2, -1, 0, 1, 2, 3]
    .map(n => moment().add(n, "week"))
    .map(day => {
      const monday = day.startOf("week");
      return [0, 1, 2, 3, 4, 5, 6].map(n => monday.clone().add(n, "day"));
    });
}
</script>
