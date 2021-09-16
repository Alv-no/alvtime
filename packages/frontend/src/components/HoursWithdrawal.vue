<template>
  <CenterColumnWrapper>
    <div class="padding">
      <div class="availablehours">
        <div class="absense available-flex">
          <h2>Feriedager</h2>
          <OvertimeVisualizer :bar-data="holidayData"></OvertimeVisualizer>
        </div>
        <div class="available available-flex">
          <h2>Overtidstimer</h2>
          <OvertimeVisualizer
            :bar-data="overtimeData"
            :subtract="hours"
          ></OvertimeVisualizer>
        </div>
      </div>
      <hr />

      <small
        >Du har <b>{{ overtime }}</b> {{ hoursText }} tilgjengelig i timebanken.
        Tast inn antall timer du ønsker å ta ut som overtidsbetaling.
        Utbetalingen kommer på neste lønning.</small
      >

      <div class="order-payout-field">
        <Input
          v-model="hours"
          :error="erroneousInput"
          placeholder="Antall timer"
          type="number"
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
      <md-table v-model="sortedTransactions" md-fixed-header>
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
          <md-table-cell md-sort-by="total" md-label="Total">{{
            item.sum
          }}</md-table-cell>
          <md-table-cell md-sort-by="remove" md-label=""
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
import { Store } from "vuex";
import { State } from "../store/index";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";
import OvertimeVisualizer from "@/components/OvertimeVisualizer.vue";
import { MappedOvertimeTransaction } from "../store/overtime";

interface ValidationRule {
  errorMessage: string;
  validator: (hours: string, available: number) => boolean;
}

interface InternalTransaction {
  id: number;
  date: string;
  hours: number;
  type: string;
  rate?: number;
  sum?: number;
  active?: boolean;
}

const rules: ValidationRule[] = [
  {
    errorMessage: "Skriv inn gyldig tall",
    validator: (hours, _) => isFloat(hours as string),
  },
  {
    errorMessage: "Antall timer må være større enn 0",
    validator: (hours, _) => Number(hours) > 0,
  },
  {
    errorMessage: "Kun utbetaling i halve timer",
    validator: (hours, _) => Number(hours) % 0.5 === 0,
  },
  {
    errorMessage: "Du kan ikke ta ut flere timer enn du har i banken",
    validator: (hours, available) => Number(hours) <= available,
  },
];

export default Vue.extend({
  components: {
    YellowButton,
    Input,
    CenterColumnWrapper,
    OvertimeVisualizer,
  },
  data() {
    return {
      hours: "",
      transactions: [],
      today: moment().format("YYYY-MM-DD"),
      overtimeData: [],
      holidayData: [],
      holidaySubtractions: [],
      unsubscribe: () => {},
    };
  },
  computed: {
    overtime(): number {
      return this.$store.getters.getAvailableHours;
    },
    hoursText(): string {
      return this.overtime === 1 ? "time" : "timer";
    },
    sortedTransactions(): InternalTransaction[] {
      if (this.transactions) {
        return (this.transactions as InternalTransaction[]).sort((a, b) => {
          let aDate = new Date(a.date);
          let bDate = new Date(b.date);
          if (a.active) return 0 - aDate.getTime();
          return bDate.getTime() - aDate.getTime();
        });
      }
      return [];
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
        if (!rule.validator(this.hours, this.overtime)) {
          error = rule.errorMessage;
          break;
        }
      }
      return error;
    },
    disabled(): boolean {
      for (let rule of rules) {
        if (!rule.validator(this.hours, this.overtime)) return true;
      }
      return false;
    },
  },
  async created() {
    await this.$store.dispatch("FETCH_TRANSACTIONS");
    this.processTransactions();
    await this.$store.dispatch("FETCH_AVAILABLE_HOURS");
    this.overtimeData = (this.$store as Store<
      State
    >).getters.getCategorizedFlexHours;
    this.unsubscribe = (this.$store as Store<State>).subscribe(
      (mutation, _) => {
        if (mutation.type === "SET_AVAILABLEHOURS") {
          this.overtimeData = (this.$store as Store<
            State
          >).getters.getCategorizedFlexHours;
        }
      }
    );
    await this.$store.dispatch("FETCH_VACATIONOVERVIEW");
    this.holidayData = (this.$store as Store<State>).getters.getAbsenseOverview;
    this.holidaySubtractions = (this.$store as Store<
      State
    >).getters.getAbsenseOverviewSubtractions;
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
          sum: this.getTranslatedTotal(transaction.type, transaction),
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
          return value;
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
    getTranslatedTotal(
      type: string,
      transaction: MappedOvertimeTransaction
    ): number {
      switch (type) {
        case "available":
          return transaction.transaction.hours * transaction.transaction.rate!;
        case "payout":
          return transaction.transaction.hoursAfterCompRate!;
        case "flex":
          return transaction.transaction.hours * -1;
        default:
          return transaction.transaction.hours;
      }
    },
    async orderHours() {
      await this.$store.dispatch("POST_ORDER_PAYOUT", {
        hours: parseFloat(this.hours),
        date: this.today,
      });
      await this.$store.dispatch("FETCH_TRANSACTIONS");
      this.processTransactions();
      this.hours = "";
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
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  max-width: 50rem;
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

.md-table-head-label i:hover {
  background-color: #fff;
}

.availablehours .absense {
  margin-bottom: 50px;
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
