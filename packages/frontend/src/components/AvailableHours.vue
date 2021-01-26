<template>
  <div v-if="$store.state.currentRoute.name === 'hours'" class="sums">
    <template v-if="!small">
      Timebanken:
      {{ availableHours }}
    </template>
    <template v-if="small">
      {{ smallAvailableHoursText }}
    </template>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { mapState, Store } from "vuex";
import { State } from "../store/index";
export default Vue.extend({
  props: {
    small: {
      type: Boolean,
      default: false,
    },
  },
  data: function() {
    return {
      unsubscribe: () => {},
    };
  },
  computed: {
    availableHours(): number {
      return this.$store.getters.getAvailableHours;
    },
    smallAvailableHoursText(): string {
      return this.availableHours > 0 ? `+${this.availableHours}` : "";
    },
  },
  async created() {
    this.unsubscribe = (this.$store as Store<State>).subscribe(
      (mutation, state) => {
        if (
          mutation.type === "UPDATE_TIME_ENTRIES_AFTER_UPDATE" ||
          mutation.type === "SET_SWIPER"
        ) {
          this.$store.dispatch("FETCH_AVAILABLE_HOURS");
        }
      }
    );
  },
  beforeDestroy() {
    this.unsubscribe();
  },
});
</script>

<style></style>
