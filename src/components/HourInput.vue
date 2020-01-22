<template>
  <form novalidate>
    <input :type="type" v-model="value" @touchstart="onTouchStart" novalidate />
  </form>
</template>

<script>
export default {
  props: ["timeEntrie"],
  data() {
    return {
      type: "text",
    };
  },

  computed: {
    value: {
      get() {
        const { id, date } = this.timeEntrie;
        const entrie = this.$store.state.timeEntries.find(
          entrie => entrie.id === id && entrie.date === date
        );
        return entrie ? entrie.value : 0;
      },
      set(str) {
        const payload = {
          timeEntrie: { ...this.timeEntrie, value: Number(str) },
        };
        this.$store.commit("UPDATE_TIME_ENTRIE", payload);
      },
    },
  },

  methods: {
    onTouchStart() {
      this.type = "number";
      setTimeout(() => (this.type = "text"), 200);
    },
  },
};
</script>

<style scoped>
input {
  appearance: none;
  -moz-appearance: textfield;
  width: 2.1rem;
  padding: 0.4rem;
  font-size: 0.8rem;
  border-radius: 0;
}
</style>
