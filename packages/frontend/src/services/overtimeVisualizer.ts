import { CategorizedFlexHours } from "@/store/overtime";

export class OvertimeVisualizerHelper {
  public static CloneConfig(
    config: CategorizedFlexHours
  ): CategorizedFlexHours {
    return {
      key: config.key,
      name: config.name,
      colorValue: config.colorValue,
      value: config.value,
      priority: config.priority,
    };
  }

  // WithDrawFromList does the magic of withdrawing from higest priority first
  public static WithDrawFromList(
    colorConfigs: CategorizedFlexHours[],
    value: number
  ): CategorizedFlexHours[] {
    const newItems: CategorizedFlexHours[] = [];
    var sortedConfig = colorConfigs.sort((a, b) => a.priority - b.priority);
    for (var item of sortedConfig) {
      const clone = OvertimeVisualizerHelper.CloneConfig(item);
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
  }
}
