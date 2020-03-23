<template>
  <div>
    <WeekHeader @backClick="onPrev" @forwardClick="onNext" />
    <div ref="mySwiper" class="swiper-container">
      <div class="swiper-wrapper">
        <div
          class="swiper-slide"
          v-for="(week, index) in virtualData.slides"
          :key="index"
          :style="{ left: `${virtualData.offset}px` }"
        >
          <TimeEntrieWeekList :week="week" />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import moment from "moment";
import config from "@/config";
import TimeEntrieWeekList from "./TimeEntrieWeekList.vue";
import WeekHeader from "./WeekHeader.vue";
import isInIframe from "@/mixins/isInIframe";
import { createWeek } from "@/mixins/date";

import Swiper from "swiper";

export default Vue.extend({
  components: {
    TimeEntrieWeekList,
    WeekHeader,
  },

  data() {
    return {
      swiperOption: {},
      // @ts-ignore
      weeks: this.createManyWeeks(),
      virtualData: [],
    };
  },

  mounted() {
    const self = this;
    const swiper = new Swiper(".swiper-container", {
      initialSlide: 142,
      shortSwipes: false,
      simulateTouch: false,
      noSwipingSelector: "input, button",
      longSwipesRatio: 0.15,
      longSwipesMs: 100,
      keyboard: {
        enabled: true,
        onlyInViewport: false,
      },
      on: {
        // @ts-ignore
        slideChangeTransitionEnd: self.onSlideChangeTransitionEnd,
      },
      virtual: {
        // @ts-ignore
        slides: self.weeks,
        renderExternal(data: any) {
          // @ts-ignore
          self.virtualData = data;
        },
      },
    });
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

    onSlideChangeTransitionEnd() {
      const dayOfWeek = this.$store.state.activeDate.weekday();
      this.$store.commit(
        "UPDATE_ACTVIE_DATE",
        // @ts-ignore
        this.weeks[this.swiper.activeIndex][dayOfWeek]
      );
    },

    createManyWeeks() {
      const date = this.$store.state.activeDate;
      const future = Array.apply(null, Array(142)).map((n, i) => i);
      const past = Array.apply(null, Array(142))
        .map((n, i) => (i + 1) * -1)
        .reverse();
      return [...past, ...future]
        .map(n => date.clone().add(n, "week"))
        .map(createWeek);
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
</script>
