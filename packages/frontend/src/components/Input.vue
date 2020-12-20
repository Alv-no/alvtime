<template>
  <input
    ref="inputRef"
    :class="{ error }"
    :value="value"
    :placeholder="placeholder"
    @input="update"
    :type="type"
  />
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({
  props: {
    value: {
      type: String,
      default: "",
    },
    placeholder: {
      type: String,
      default: "",
    },
    error: {
      type: Boolean,
      default: false,
    },
    type: {
      type: String,
      default: "text"
    }
  },

  methods: {
    update() {
      // @ts-ignore
      this.$emit("input", this.$refs.inputRef.value);
    },
  },
});
</script>

<style scoped>
input {
  appearance: none;
  -moz-appearance: textfield;
  width: 100%;
  padding: 0.4rem;
  font-size: 0.8rem;
  border-radius: 5px;
  border: 1px solid #e0e0e0;
  background-color: #f7f7f7;
  font-size: 0.8rem;
  line-height: 0.7rem;
  height: 2rem;
}

input:focus {
  outline: none;
}

input:hover {
  border-color: #008dcf;
  transition: border-color 500ms ease-out;
}

.error {
  background-color: #d7312540;
  transition: border-color 500ms ease-in-out;
}

.error:hover {
  border-color: #008dcf;
  transition: border-color 500ms ease-out;
}
</style>
