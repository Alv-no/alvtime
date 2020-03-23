<template>
  <div>
    <MobileHeader />
    <div ref="mySwiper" class="swiper-container">
      <div class="swiper-wrapper">
        <div
          class="swiper-slide"
          v-for="(date, index) in virtualData.slides"
          :key="index"
          :style="{ left: `${virtualData.offset}px` }"
        >
          <TimeEntrieDayList :date="date" />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import config from "@/config";
import TimeEntrieDayList from "./TimeEntrieDayList.vue";
import MobileHeader from "./MobileHeader.vue";
import isInIframe from "@/mixins/isInIframe";

import Swiper from "swiper";

export default Vue.extend({
  components: {
    TimeEntrieDayList,
    MobileHeader,
  },

  data() {
    return {
      // @ts-ignore
      dates: this.createManySlides(),
      virtualData: [],
    };
  },

  mounted() {
    const self = this;
    const swiper = new Swiper(".swiper-container", {
      initialSlide: 1000,
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
        slideChangeTransitionEnd: self.onSlideChangeTransitionEnd,
      },
      virtual: {
        slides: self.dates,
        renderExternal(data: any) {
          self.virtualData = data;
        },
      },
    });
  },

  created() {
    if (!isInIframe()) {
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
    onSlideChangeTransitionEnd() {
      this.$store.commit(
        "UPDATE_ACTVIE_DATE",
        // @ts-ignore
        this.dates[this.swiper.activeIndex]
      );
    },

    createManySlides() {
      const date = this.$store.state.activeDate;
      const future = Array.apply(null, Array(1001)).map((n, i) => i);
      const past = Array.apply(null, Array(1000))
        .map((n, i) => (i + 1) * -1)
        .reverse();
      return [...past, ...future].map(n => date.clone().add(n, "day"));
    },
  },
});
</script>
