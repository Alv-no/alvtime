<template>
  <form novalidate>
    <input type="text" pattern="\d*" v-model="value" novalidate />
  </form>
</template>

<script>
export default {
  props: ["timeEntrie"],

  computed: {
    value: {
      set(str) {
        const payload = {
          timeEntrie: { ...this.timeEntrie, value: Number(str) },
        };
        this.$store.commit("UPDATE_TIME_ENTRIE", payload);
      },
      get() {
        const { id, date } = this.timeEntrie;
        const entrie = this.$store.getters.getTimeEntrie(id, date);
        return entrie ? entrie.value : 0;
      },
    },
  },
};
</script>

<style scoped>
input {
  appearance: none;
  -moz-appearance: textfield;
  width: 1.5rem;
  height: 1rem;
  padding: 5px;
}
</style>
