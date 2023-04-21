<template>
  <div ref="cl" class="color-list">
    <div v-for="color in filteredColors" :key="color.priority" :style="createWidthString(color)" class="color-bar">
      <template v-if="color.value > 0">
        <div class="color-bar-content">
          <div class="color-bar-name">{{ color.name }}</div>
          <div :style="createColorString(color)" class="color-bar-value" @mouseover="doSomething(canSplit)">
            {{ color.value }}
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Store } from "vuex";

export interface OvertimeData {
  key: string;
  name: string;
  colorValue: string;
  value: number;
  priority: number;
}
export interface TargetedSubtract {
  key: string;
  value: number;
}

export default Vue.extend({
  props: {
    subtract: {
      default: "",
      type: String,
    },
    barData: {
      default: () => [],
      type: Array as () => OvertimeData[],
    },
    targetedSubtract: {
      default: () => [],
      type: Array as () => TargetedSubtract[],
    },
    canSplit: {
      default: false,
      type: Boolean
    }
  },
  data() {
    return {
      unsubscribe: () => { },
      targetSubtract: [] as TargetedSubtract[],
    };
  },
  computed: {
    filteredColors(): OvertimeData[] {
      return this.withDrawFromList(
        this.barData as OvertimeData[],
        Number.parseInt(this.subtract, 10),
        this.targetSubtract as TargetedSubtract[]
      );
    },
  },
  beforeDestroy() {
    this.unsubscribe();
  },
  mounted() {
    setTimeout(() => {
      this.targetSubtract = this.targetedSubtract as TargetedSubtract[];
    }, 750);
  },
  methods: {
    doSomething(canSplit: Boolean) {
      if (canSplit) {
        console.log("heihei")
      }
    },
    createColorString(colorConfig: OvertimeData): string {
      return `background: ${colorConfig.colorValue};`;
    },
    createWidthString(colorConfig: OvertimeData): string {
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
      (this.barData as OvertimeData[]).forEach(item => {
        valueSum += item.value;
      });
      return valueSum;
    },
    getElementWidth(): number {
      if (this.$refs.cl) return (this.$refs.cl as HTMLDivElement).clientWidth;
      else return window.innerWidth;
    },
    withDrawFromList(
      colorConfigs: OvertimeData[],
      value: number,
      targetValues: TargetedSubtract[]
    ): OvertimeData[] {
      const newItems: OvertimeData[] = [];
      var sortedConfig = colorConfigs
        .sort((a, b) => b.priority - a.priority)
        .map(item => {
          return { ...item };
        });
      // Subtract by targetedValues
      for (var targetValue of targetValues) {
        console.log(targetValue);
        for (var config of sortedConfig) {
          if (targetValue.key == config.key) {
            config.value -= targetValue.value;
          }
        }
      }
      // Subtract by priority and inserted
      for (var item of sortedConfig) {
        const clone = { ...item };
        if (value > 0) {
          if (clone.value < value) {
            value = value - clone.value;
            clone.value = 0;
          } else {
            clone.value = clone.value - value;
            value = 0;
          }
        }
        newItems.push(clone);
      }
      return newItems;
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
