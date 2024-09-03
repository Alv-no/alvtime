<template>
  <div>
    <div id="week-swiper-container" ref="mySwiper" class="swiper-container">
      <div class="swiper-wrapper">
        <div
          v-for="(week, index) in virtualData.slides"
          :key="index"
          class="swiper-slide"
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
import { Moment } from "moment";
import TimeEntrieWeekList from "./TimeEntrieWeekList.vue";
import { GLOBAL_SWIPER_OPTIONS } from "@/store/swiper";

import Swiper from "swiper";

export default Vue.extend({
  components: {
    TimeEntrieWeekList,
  },

  data() {
    return {
      virtualData: [[]] as Moment[][],
      preventEvent: true,
    };
  },

  beforeCreate() {
    this.$store.commit("CREATE_WEEKS");
    this.$store.dispatch("FETCH_WEEK_ENTRIES");
  },

  mounted() {
    const swiperOptions = {
      ...GLOBAL_SWIPER_OPTIONS,
      initialSlide: this.$store.getters.initialWeekSlide,
      on: {
        transitionEnd: this.onTransitionEnd,
      },
      virtual: {
        slides: this.$store.state.weeks,
        renderExternal: this.onRenderExternal,
      },
    };

    const swiper = new Swiper("#week-swiper-container", swiperOptions);
    this.$store.commit("SET_SWIPER", swiper);
  },

  methods: {
    onTransitionEnd() {
      if (this.preventEvent) {
        this.preventEvent = false;
      } else {
        this.$store.commit("UPDATE_ACTVIE_DATE_IN_WEEKS");
      }
    },

    onRenderExternal(data: Moment[][]) {
      this.virtualData = data;
    },
  },
});
</script>

<style>
.swiper-container {
  overflow-y: visible !important; /* Ensure comment box is not hidden behind the swiper */
  min-height: 50vh !important;
}

@media only screen and (max-width: 1000px) {
  .center {
    overflow: visible !important;
  }
}
</style>
