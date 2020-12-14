<template>
  <div ref="cl" class="color-list">
    <div
      v-for="color in filteredColors"
      :key="color.priority"
      :style="createWidthString(color)"
      class="color-bar"
    >
      <template v-if="color.value > 0">
        <div class="color-bar-content">
          <div class="color-bar-name">{{ color.name }}</div>
          <div :style="createColorString(color)" class="color-bar-value">
            {{ color.value }}
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
// eslint-disable-next-line
import { State } from "../store/index";
import { CategorizedFlexHours } from "../store/overtime";
import { mapState, Store } from "vuex";
import { OvertimeVisualizerHelper } from "../services/overtimeVisualizer";

export default Vue.extend({
  props: {
    subtract: {
      default: "",
      type: String,
    },
  },
  data() {
    return {
      colors: [],
      unsubscribe: () => {},
    };
  },
  computed: {
    filteredColors(): CategorizedFlexHours[] {
      return OvertimeVisualizerHelper.WithDrawFromList(
        this.colors as CategorizedFlexHours[],
        Number.parseInt(this.subtract, 10)
      ).reverse();
      //return (this.colors as CategorizedFlexHours[]).sort((a, b) => a.priority - b.priority).reverse();
    },
    subtractedColors(): CategorizedFlexHours[] {
      return OvertimeVisualizerHelper.WithDrawFromList(
        this.colors as CategorizedFlexHours[],
        Number.parseInt(this.subtract, 10)
      );
    },
  },
  async created() {
    await this.$store.dispatch("FETCH_AVAILABLE_HOURS");
    this.colors = (this.$store as Store<State>).getters.getCategorizedFlexHours;
    this.unsubscribe = (this.$store as Store<State>).subscribe(
      (mutation, state) => {
        if (mutation.type === "SET_AVAILABLEHOURS") {
          this.colors = (this.$store as Store<
            State
          >).getters.getCategorizedFlexHours;
        }
      }
    );
  },
  beforeDestroy() {
    this.unsubscribe();
  },
  methods: {
    createColorString(colorConfig: CategorizedFlexHours): string {
      return `background: ${colorConfig.colorValue};`;
    },
    createWidthString(colorConfig: CategorizedFlexHours): string {
      //const nr = Number.parseInt(this.subtract, 10) || 0;
      //if (nr == 0)
      //return this.createFlexWidthString(colorConfig.value);

      return this.createCalculatedWidthString(colorConfig.value);
    },
    createFlexWidthString(value: number): string {
      return `flex-grow: ${Math.floor(
        (value / this.getValuePercentage()) * 100
      )};`;
    },
    createCalculatedWidthString(value: number): string {
      const width: number = this.getElementWidth();
      const pixelWidth: number = Math.floor(
        (value / this.getValuePercentage()) * width
      );
      return `width: ${pixelWidth}px;`;
    },
    getValuePercentage(): number {
      let valueSum = 0;
      (this.colors as CategorizedFlexHours[]).forEach(item => {
        valueSum += item.value;
      });

      return valueSum;
    },
    getElementWidth(): number {
      if (this.$refs.cl) return (this.$refs.cl as HTMLDivElement).clientWidth;
      else return window.innerWidth;
    },
  },
});
</script>

<style scoped>
.color-list {
  display: flex;
  align-content: stretch;
}

.color-bar {
  transition: width 0.5s;
}

.color-bar-value {
  height: 30px;
  display: flex;
  justify-content: center;
  align-items: center;
  font-weight: 600;
}

.color-bar-name {
  text-align: center;
  font-weight: 600;
}
</style>
