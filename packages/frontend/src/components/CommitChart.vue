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
        "January",
        "February",
        "March",
        "April",
        "May",
        "June",
        "July",
        "August",
        "September",
        "October",
        "November",
        "December",
      ],
      datasets: [
        {
          label: "Hours recorded",
          backgroundColor: "#f87979",
          data: this.data,
        },
      ],
    });
  },
});
</script>
