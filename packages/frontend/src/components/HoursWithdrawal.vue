<template>
  <CenterColumnWrapper>
    <div class="padding">
				<div class="availablehours">
					<div class="available available-flex">
						<h4>Timer tilgjengelig for flex</h4>
						<div class="badge">
							{{overtime}}	
						</div>						
					</div>
					<div class="available available-flex">
						<h4>Kompenserte timer</h4>
						<div class="badge">
							{{overtimeCompensated}}
						</div> 
					</div>
				</div>
			
			<hr>
			
			<small style="padding: 10px">Tast inn antall timer du ønsker å ta ut. Maks antall timer er dine kompanserte timer</small>

      <div class="order-payout-field">
        <Input v-model="hours" :error="!isFloat" placeholder="Antall timer" />
        <YellowButton
          icon-id="add_circle_outline"
          :text="buttonText"
          :disabled="!isNumber"
          @click="orderHours"
        />
      </div>

			<hr>

			<md-table md-fixed-header v-model="transactions">
				<md-table-toolbar>
					<h2 class="md-title">Transaksjoner</h2>
				</md-table-toolbar>
				<md-table-row slot="md-table-row" slot-scope="{item}">
					<md-table-cell md-sort-by="date" md-label="Dato">{{item.date}}</md-table-cell>
					<md-table-cell md-sort-by="type" md-label="Type">{{item.type}}</md-table-cell>
					<md-table-cell md-sort-by="hours" md-label="Timer">{{item.hours}}</md-table-cell>
					<md-table-cell md-sort-by="rate" md-label="Rate">{{item.rate}}</md-table-cell>
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
import { adAuthenticatedFetch } from "@/services/auth";
import config from "@/config";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";
import { MappedTransaction } from "../store/overtime";
export default Vue.extend({
  components: {
    YellowButton,
    Input,
    CenterColumnWrapper,
  },
  data() {
    return {
      hours: '',
			transactions: [],
      today: moment().format("YYYY-MM-DD")
    };
  },
  async created() {
		// TODO FETCH Most data here
		await this.$store.dispatch("FETCH_TRANSACTIONS");
		this.processTransactions();
  },
  computed: {
		overtime(): number {
			return this.$store.getters.getAvailableHours;
		},
		overtimeCompensated(): number {
			return this.$store.getters.getAvailableCompensated;
		},
    buttonText(): string {
      // @ts-ignore
      return this.$mq === "sm" ? "" : "bestill";
    },
    isNumber(): boolean {
			if (this.hours.length > 0) {
				 return !Number.isNaN(parseFloat(this.hours));
			}
			return false;

    },
    isFloat(): boolean {
      const hours = this.hours ? this.hours : "";
      return isFloat(hours as string);
    },
  },
  methods: {
		processTransactions() {
			const transactions = this.$store.getters.getTransactionList as MappedTransaction[];
			const mapped = transactions.map(transaction => {
					return {
						date: transaction.transaction.date,
						hours: this.getTranslatedValue(transaction.transaction.hours, transaction.type),
						type: this.getTranslatedType(transaction.type),
						rate: transaction.transaction.rate ? `${transaction.transaction.rate * 100}%` : ''
					};
			});
			this.transactions = mapped as never[];
		},
		getTranslatedValue(value: number, type: string): number {
			switch(type) {
				case 'available':
					return value;
				case 'payout':
					return (value*-1);
				case 'flex':
					return value*-1;
			}
			return value;
		},
		getTranslatedType(type: string): string {
			switch(type) {
				case 'available':
					return 'Opptjent';
				case 'payout':
					return 'Utbetalt';
				case 'flex':
					return 'Flex';
				default: return '';
			}
		},
		async orderHours() {
			await this.$store.dispatch("POST_ORDER_PAYOUT", {hours: parseFloat(this.hours), date: this.today});
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
	justify-content: space-between;
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
</style>
