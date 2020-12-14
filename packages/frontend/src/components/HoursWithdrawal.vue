<template>
  <CenterColumnWrapper>
    <div class="padding">
      <div class="availablehours">
        <div class="available available-flex">
          <h4>Kompenserte timer</h4>
          <div class="badge">
            {{ overtime }}
          </div>
        </div>
      </div>

      <hr />

      <small style="padding: 10px"
        >Tast inn antall timer du ønsker å ta ut. Maks antall timer er dine
        kompanserte timer</small
      >

      <div class="order-payout-field">
        <Input
          v-model="hours"
          :error="erroneousInput"
          placeholder="Antall timer"
        />
        <YellowButton
          icon-id="add_circle_outline"
          :text="buttonText"
          :disabled="disabled"
          @click="orderHours"
        />
      </div>
      <small class="validationtext">{{ errorMessage }}</small>

      <hr />

      <md-table v-model="transactions" md-fixed-header>
        <md-table-toolbar>
          <h2 class="md-title">Transaksjoner</h2>
        </md-table-toolbar>
        <md-table-row slot="md-table-row" slot-scope="{ item }">
          <md-table-cell md-sort-by="date" md-label="Dato">{{
            item.date
          }}</md-table-cell>
          <md-table-cell md-sort-by="type" md-label="Type">{{
            item.type
          }}</md-table-cell>
          <md-table-cell md-sort-by="hours" md-label="Timer">{{
            item.hours
          }}</md-table-cell>
          <md-table-cell md-sort-by="rate" md-label="Rate">{{
            item.rate
          }}</md-table-cell>
          <md-table-cell md-sort-by="rate" md-label=""
            ><md-icon
              v-if="item.active"
              class="delete-transaction"
              @click.native="removeHourOrder(item.id)"
              >delete</md-icon
            ></md-table-cell
          >
        </md-table-row>
      </md-table>
    </div>
  </CenterColumnWrapper>
</template>

<script lang="ts">
import Vue from "vue";
import YellowButton from "./YellowButton.vue";
import { isFloat } from "@/store/timeEntries";
import Input from "./Input.vue";
import moment, { Moment } from "moment";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";
import { MappedOvertimeTransaction } from "../store/overtime";

interface ValidationRule {
  errorMessage: string;
  validator: (hours: string) => boolean;
}

const rules: ValidationRule[] = [
  {
    errorMessage: "Skriv inn gyldig tall",
    validator: hours => isFloat(hours as string),
  },
  {
    errorMessage: "Antall timer må være større enn 0",
    validator: hours => Number(hours) > 0,
  },
  {
    errorMessage: "Kun utbetaling i halve timer",
    validator: hours => Number(hours) % 0.5 === 0,
  },
];

export default Vue.extend({
  components: {
    YellowButton,
    Input,
    CenterColumnWrapper,
  },
  data() {
    return {
      hours: "",
      transactions: [],
      today: moment().format("YYYY-MM-DD"),
    };
  },
  computed: {
    overtime(): number {
      return this.$store.getters.getAvailableHours;
    },
    buttonText(): string {
      // @ts-ignore
      return this.$mq === "sm" ? "" : "bestill";
    },
    erroneousInput(): boolean {
      return this.errorMessage.length > 0;
    },
    errorMessage(): string {
      if (this.hours.length === 0) return "";

      let error = "";
      for (let rule of rules) {
        if (!rule.validator(this.hours)) {
          error = rule.errorMessage;
          break;
        }
      }
      return error;
    },
    disabled(): boolean {
      for (let rule of rules) {
        if (!rule.validator(this.hours)) return true;
      }
      return false;
    },
  },
  async created() {
    await this.$store.dispatch("FETCH_TRANSACTIONS");
    this.processTransactions();
  },
  methods: {
    processTransactions() {
      const transactions = this.$store.getters
        .getTransactionList as MappedOvertimeTransaction[];
      const mapped = transactions.map(transaction => {
        return {
          id: transaction.transaction.id,
          date: transaction.transaction.date,
          hours: this.getTranslatedValue(
            transaction.transaction.hours,
            transaction.type
          ),
          type: this.getTranslatedType(
            transaction.type,
            transaction.transaction.active
          ),
          rate: transaction.transaction.rate
            ? `${transaction.transaction.rate * 100}%`
            : "",
          sum: transaction.transaction.rate
            ? transaction.transaction.hours * (transaction.transaction.rate + 1)
            : undefined,
          active: transaction.transaction.active,
        };
      });
      this.transactions = mapped as never[];
    },
    getTranslatedValue(value: number, type: string): number {
      switch (type) {
        case "available":
          return value;
        case "payout":
          return value * -1;
        case "flex":
          return value * -1;
      }
      return value;
    },
    getTranslatedType(type: string, active: boolean = false): string {
      switch (type) {
        case "available":
          return "Opptjent";
        case "payout":
          return active ? "Til utbetaling" : "Utbetalt";
        case "flex":
          return "Flex";
        default:
          return "";
      }
    },
    async orderHours() {
      await this.$store.dispatch("POST_ORDER_PAYOUT", {
        hours: parseFloat(this.hours),
        date: this.today,
      });
      await this.$store.dispatch("FETCH_TRANSACTIONS");
      this.processTransactions();
    },

    async removeHourOrder(id: number) {
      await this.$store.dispatch("CANCEL_PAYOUT_ORDER", { payoutId: id });
      await this.$store.dispatch("FETCH_TRANSACTIONS");
      this.processTransactions();
    },
  },
});
</script>

<style scoped>
.padding {
  padding: 1rem;
}

.order-payout-field {
  display: flex;
  justify-content: left;
}

hr {
  margin: 20px 0 20px 0;
}

@keyframes enter {
  0% {
    height: 0;
    padding: 0;
    transform: scale(1, 0);
  }
  100% {
    height: 2rem;
    padding: 0.5rem 0;
    transform: scale(1, 1);
  }
}

.md-table-fixed-header-container table {
  width: 100%;
}

.availablehours {
  display: flex;
  justify-content: center;
}

.md-table-head-label i:hover {
  background-color: #fff;
}

.badge {
  display: grid;
  align-content: center;
  font-weight: 600;
  text-align: center;
  background-color: #008dcf;
  color: white;
  height: 1.55rem;
  width: 100%;
  padding: 0.1rem;
  border-radius: 5px;
  font-size: 0.7rem;
  line-height: 1.5rem;
}

.delete-transaction {
  cursor: pointer;
}
.delete-transaction:hover {
  color: #000 !important;
}

.validationtext {
  color: #a22;
}
</style>
