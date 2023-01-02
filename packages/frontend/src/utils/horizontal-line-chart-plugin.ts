import { Plugin, ChartTypeRegistry, ChartType } from "chart.js";

export interface HorizontalLinePluginOptions {
  yValue: number;
  color?: string;
}

export const HorizontalLinePlugin: Plugin<
  keyof ChartTypeRegistry,
  HorizontalLinePluginOptions
> = {
  id: "horizontalLinePlugin",
  afterDraw: (chart, args, options) => {
    const ctx = chart.ctx;
    const style = options.color || "rgba(169,169,169, .6)";
    const yScale = chart.scales["y"];
    const xScale = chart.scales["x"];
    if (!yScale || !xScale) {
      return;
    }

    const yCoordinate = yScale.getPixelForValue(options.yValue);
    const xCoordinate = xScale.getPixelForValue(chart.data.labels?.length || 0);

    ctx.beginPath();
    ctx.moveTo(0, yCoordinate);
    ctx.setLineDash([15, 15]);
    ctx.lineTo(xCoordinate, yCoordinate), (ctx.strokeStyle = style);
    ctx.stroke();
  },
};

declare module "chart.js" {
  interface PluginOptionsByType<TType extends ChartType> {
    /**
     * Per chart datalabels plugin options.
     * @since 0.1.0
     */
    horizontalLinePlugin?: HorizontalLinePluginOptions;
  }
}
