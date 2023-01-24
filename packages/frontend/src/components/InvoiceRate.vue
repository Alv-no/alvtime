<template>
  <p>{{ invoiceRate }}</p>
</template>

<script lang="ts">
import { State } from "@/store";
import Vue from "vue";
import { Store } from "vuex";
import { getCurrentMonthName } from "../utils/timestamp-text-util";
export default Vue.extend({
  props: {
    small: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      unsubscribe: () => {},
    };
  },
  computed: {
    invoiceRate(): string {
      return `Fakturering ${getCurrentMonthName(true)} ${
        this.$store.getters.getInvoiceRate
      }%`;
    },
  },
  async created() {
    await this.$store.dispatch("FETCH_INVOICE_RATE");
    this.unsubscribe = (this.$store as Store<State>).subscribe(
      (mutation, _) => {
        if (
          mutation.type === "UPDATE_TIME_ENTRIES_AFTER_UPDATE" ||
          mutation.type === "SET_SWIPER"
        ) {
          this.$store.dispatch("FETCH_INVOICE_RATE");
        }
      }
    );
  },
  beforeDestroy() {
    this.unsubscribe();
  },
});
</script>
<style></style>
