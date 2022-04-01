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

      <div class="md-content md-table md-theme-default">
        <div
          class="md-toolbar md-table-toolbar md-transparent md-theme-default md-elevation-0"
        >
          <h2 class="md-title">Transaksjoner</h2>
        </div>
        <div
          class="md-content md-table-content md-scrollbar md-theme-default"
          style="height: 400px; max-height: 400px;"
        >
          <table>
            <thead>
              <tr>
                <th>
                  <!-- more icom -->
                </th>
                <th class="md-table-head">
                  <div class="md-table-head-container md-ripple md-disabled">
                    <div class="md-table-head-label">Dato</div>
                  </div>
                </th>
                <th class="md-table-head">
                  <div class="md-table-head-container md-ripple md-disabled">
                    <div class="md-table-head-label">Type</div>
                  </div>
                </th>
                <th class="md-table-head">
                  <div class="md-table-head-container md-ripple md-disabled">
                    <div class="md-table-head-label">Timer</div>
                  </div>
                </th>
                <th class="md-table-head">
                  <div class="md-table-head-container md-ripple md-disabled">
                    <div class="md-table-head-label">Rate</div>
                  </div>
                </th>
                <th class="md-table-head">
                  <div class="md-table-head-container md-ripple md-disabled">
                    <div class="md-table-head-label">Total</div>
                  </div>
                </th>
                <th class="md-table-head">
                  <div class="md-table-head-container md-ripple md-disabled">
                    <div class="md-table-head-label"></div>
                  </div>
                </th>
              </tr>
            </thead>
            <tbody>
              <template v-for="transaction in sortedTransactions">
                <tr
                  class="md-table-row"
                  :class="
                    transaction.subItems && transaction.subItems.length !== 0
                      ? 'selectable'
                      : ''
                  "
                  @click="onRowSelect(transaction)"
                  :key="transaction.id"
                >
                  <td class="md-table-cell">
                    <!-- more icon -->
                    <div
                      v-if="
                        transaction.subItems &&
                          transaction.subItems.length !== 0
                      "
                      style="display:inline;"
                    >
                      <md-icon v-if="!isExpanded(transaction.id)"
                        >unfold_more</md-icon
                      >
                      <md-icon v-else>unfold_less</md-icon>
                    </div>
                  </td>
                  <td class="md-table-cell">
                    <div class="md-table-cell-container">
                      {{ transaction.date }}
                    </div>
                  </td>
                  <td class="md-table-cell">
                    <div class="md-table-cell-container">
                      {{ transaction.type }}
                    </div>
                  </td>
                  <td class="md-table-cell">
                    <div class="md-table-cell-container">
                      {{ transaction.hours }}
                    </div>
                  </td>
                  <td class="md-table-cell">
                    <div class="md-table-cell-container">
                      {{ transaction.rate }}
                    </div>
                  </td>
                  <td class="md-table-cell">
                    <div class="md-table-cell-container">
                      {{ transaction.sum }}
                    </div>
                  </td>
                  <td class="md-table-cell">
                    <md-icon
                      v-if="transaction.delete"
                      class="delete-transaction"
                      @click.native="removeHourOrder(transaction.date, $event)"
                      >delete</md-icon
                    >
                  </td>
                </tr>
                <template v-if="isExpanded(transaction.id)">
                  <tr
                    class="md-table-row md-table-subrow"
                    v-for="subItem in transaction.subItems"
                    :key="subItem.id"
                  >
                    <td class="md-table-cell"><!-- more icon --></td>
                    <td class="md-table-cell">
                      <div class="md-table-cell-container"></div>
                    </td>
                    <td class="md-table-cell">
                      <div class="md-table-cell-container"></div>
                    </td>
                    <td class="md-table-cell">
                      <div class="md-table-cell-container">
                        {{ subItem.hours }}
                      </div>
                    </td>
                    <td class="md-table-cell">
                      <div class="md-table-cell-container">
                        {{ subItem.rate }}
                      </div>
                    </td>
                    <td class="md-table-cell">
                      <div class="md-table-cell-container">
                        {{ subItem.sum }}
                      </div>
                    </td>
                    <td class="md-table-cell"></td>
                  </tr>
                </template>
              </template>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </CenterColumnWrapper>
</template>

<script lang="ts">
import Vue from "vue";
import { v4 as uuidv4 } from "uuid";
import YellowButton from "./YellowButton.vue";
import { isFloat } from "@/store/timeEntries";
import Input from "./Input.vue";
import moment, { Moment } from "moment";
import { Store } from "vuex";
import { State } from "../store/index";
import CenterColumnWrapper from "./CenterColumnWrapper.vue";
import OvertimeVisualizer from "./OvertimeVisualizer.vue";
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
  subItems?: InternalTransaction;
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

// Payout translations
const HOUR_TYPES = {
  TO_PAYOUT: "Til utbetaling",
  PAYED_OUT: "Utbetalt",
  AVAILABLE: "Opptjent",
  FLEX: "Flex",
};

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
      expandedTransaction: null,
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
        const transactions = (this.transactions as InternalTransaction[])
          .sort((a, b) => {
            let aDate = new Date(a.date);
            let bDate = new Date(b.date);
            if (a.active) return 0 - aDate.getTime();
            return bDate.getTime() - aDate.getTime();
          })
          .map(transaction => {
            let newIdTrans = { ...transaction };
            // Give new unique ids, for usage as key in list
            newIdTrans.id = uuidv4();
            return newIdTrans;
          });

        // remap list
        const remapedTransactions = transactions.reduce((acc: any, curr) => {
          if (
            curr.type === HOUR_TYPES.TO_PAYOUT ||
            curr.type == HOUR_TYPES.PAYED_OUT
          ) {
            // if date is the same as previously accumulated, current is a sub item of that
            if (acc.length > 0 && acc[acc.length - 1].date === curr.date)
              return acc;
            // Create accumulated row, and extract sub items
            const subItems = transactions.filter(transaction => {
              return transaction.date === curr.date;
            });
            let newTrans: any = {
              id: uuidv4(),
              type: curr.type,
              date: curr.date,
              subItems: subItems,
              hours: subItems.reduce((acc, curr) => {
                return curr.hours ? acc + curr.hours : acc;
              }, 0),
              sum: subItems.reduce((acc, curr) => {
                return curr.sum ? acc + curr.sum : acc;
              }, 0),
              delete: curr.type === HOUR_TYPES.TO_PAYOUT,
            };
            acc.push(newTrans);
          } else {
            acc.push(curr);
          }
          return acc;
        }, []);

        return remapedTransactions;
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
    onRowSelect(transaction: any) {
      // Don't do anything unless expandable
      if (
        transaction.type === HOUR_TYPES.TO_PAYOUT ||
        transaction.type === HOUR_TYPES.PAYED_OUT
      ) {
        // Set expand status
        if (this.expandedTransaction === transaction.id) {
          // null it to close
          this.expandedTransaction = null;
        } else {
          // Set id to expand
          this.expandedTransaction = transaction.id;
        }
      } else {
        return;
      }
    },
    isExpanded(id: number): boolean {
      return this.expandedTransaction === id;
    },
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
          sum: this.getTranslatedTotal(transaction),
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
          return HOUR_TYPES.AVAILABLE;
        case "payout":
          return active ? HOUR_TYPES.TO_PAYOUT : HOUR_TYPES.PAYED_OUT;
        case "flex":
          return HOUR_TYPES.FLEX;
        default:
          return "";
      }
    },
    getTranslatedTotal({
      type,
      transaction,
    }: MappedOvertimeTransaction): number {
      const { hours, rate, hoursAfterCompRate } = transaction;
      switch (type) {
        case "available":
          return hours * rate!;
        case "payout":
          return hoursAfterCompRate!;
        case "flex":
          return hours * -1;
        default:
          return hours;
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

    async removeHourOrder(id: number, event: any) {
      // Prevent row from expanding when deleting
      if (event && event.stopPropagation) event.stopPropagation();

      await this.$store.dispatch("CANCEL_PAYOUT_ORDER", { payoutDate: id });
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

.selectable {
  cursor: pointer;
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

.md-table-subrow {
  background-color: #e9e9e9;
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
