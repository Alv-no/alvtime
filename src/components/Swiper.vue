<template>
  <swiper :options="swiperOption" ref="mySwiper">
    <swiperSlide v-for="(slide, index) in swiperSlides" :key="index">
      <TimeEntrieDayList :number="slide"></TimeEntrieDayList>
    </swiperSlide>
    <div class="swiper-button-prev" slot="button-prev"></div>
    <div class="swiper-button-next" slot="button-next"></div>
  </swiper>
</template>

<script>
import "swiper/dist/css/swiper.css";

import { swiper, swiperSlide } from "vue-awesome-swiper";
import TimeEntrieDayList from "./TimeEntrieList";

export default {
  components: {
    swiper,
    swiperSlide,
    TimeEntrieDayList,
  },

  data() {
    return {
      swiperOption: {
        navigation: {
          nextEl: ".swiper-button-next",
          prevEl: ".swiper-button-prev",
        },
      },
      swiperSlides: [],
    };
  },

  computed: {
    swiper() {
      return this.$refs.mySwiper.swiper;
    },
  },

  mounted() {
    setInterval(() => {
      const length = this.swiperSlides.length;
      if (length < 10) {
        this.swiperSlides.push(length + 1);
      }
      if (length === 3) {
        this.swiper.slideTo(2, 1000, false);
      }
    }, 200);
  },
};
</script>
