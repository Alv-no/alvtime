<template>
  <div class="grid">
    <div class="hour_sums"><HourSums v-if="userFound" /></div>
    <div class="title"><Title v-if="userFound" /></div>
    <mq-layout mq="sm">
      <AvailableHours small />
    </mq-layout>
    <div class="nav_buttons"><NavButtons v-if="userFound" /></div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import NavButtons from "@/components/NavButtons.vue";
import Title from "@/components/Title.vue";
import HourSums from "@/components/HourSums.vue";
import AvailableHours from "@/components/AvailableHours.vue";

export default Vue.extend({
  components: {
    NavButtons,
    Title,
    HourSums,
    AvailableHours,
  },

  computed: {
    userFound(): boolean {
      return this.$store.getters.isValidUser;
    },
  },
});
</script>

<style scoped>
.grid {
  display: grid;
  height: 2.75rem;
  grid-template-columns: 210px auto 210px;
  align-items: center;
  justify-content: space-between;
}

.hour_sums {
  margin-left: 1rem;
}

.title {
  justify-self: center;
}

.nav_buttons {
  margin-right: 1rem;
}

.available_hours {
  margin-right: 1rem;
}

@media only screen and (max-width: 650px) {
  .grid {
    grid-template-columns: 50px auto 210px;
  }
}

@media only screen and (max-width: 449px) {
  .grid {
    grid-template-columns: 50px auto 50px;
  }
}
</style>
