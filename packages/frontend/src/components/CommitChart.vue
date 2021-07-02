<script lang="ts">
import { Bar } from "vue-chartjs";
import Vue from "vue";

export default Vue.extend({
  props: {
    data: {
      type: Object as () => number[],
    },
  },
  extends: Bar,
  mounted() {
    // Overwriting base render method with actual data.
    this.renderChart({
      labels: [
        "Januar",
        "Februar",
        "Mars",
        "April",
        "Mai",
        "Juni",
        "Juli",
        "August",
        "September",
        "Oktober",
        "November",
        "Desember",
      ],
      datasets: [
        {
          label: "Timer registrert",
          backgroundColor: "#f87979",
          data: this.data,
        },
      ],
    });
  },
});
</script>
