<template>
  <CenterColumnWrapper>
    <div class="padding">
      <div class="description">
        Her kan du bestille utbetaling av dine akkumulerte overtidstimer
      </div>
      <div class="availableOvertime">
        Tilgjengelige overtidstimer for 2020: {{ overtimeYTD }} timer
      </div>
      <div class="registeredPayouts">
        Tidligere utbetalte timer i 2020: {{ totalPayout }} timer
      </div>
      <div class="input-container">
        <Input v-model="hours" :error="!isFloat" placeholder="Antall timer" />
        <YellowButton
          icon-id="add_circle_outline"
          :text="buttonText"
          :disabled="!isNumber"
          @click="orderHours"
        />
      </div>

      <div class="list">
        <transition name="expand">
          <div v-if="isNumber" class="row">
            <div class="date">{{ today }}</div>
            <div class="hours">{{ hours }}</div>
          </div>
        </transition>
      </div>

      <div class="input-container">
        <div class="date-pickers">
          <div class="date-picker">
            <md-datepicker v-model="monthStart" md-immediately>
              <label>Fra</label>
            </md-datepicker>
          </div>

          <div class="date-picker">
            <md-datepicker v-model="toDate" md-immediately>
              <label>Til</label>
            </md-datepicker>
          </div>
        </div>

        <YellowButton
          icon-id="add_circle_outline"
          text="Hent overtidstimer"
          @click="getFlexihours"
        />

        <div class="overtime">
          <div class="sum">Total flex: {{ totalFlexHours }}</div>
          <div class="sum">Total overtid: {{ overtimeEquivalents }}</div>
        </div>

        <div class="row header">
          <div class="overtime-date">Dato</div>
          <div class="overtime-value">Overtidstimer</div>
        <div />
      </div>
      <div class="line" />
    </div>

        <div
          class="row"
          v-for="flexihour in formattedFlexihours"
          :key="flexihour.date"
        >
          <div class="date">{{ flexihour.date }}</div>
          <div class="hours">{{ flexihour.value }}</div>
        </div>
      </div>
    </div>
  </CenterColumnWrapper>
</template>
​
<script lang="ts">
import Vue from "vue";
import YellowButton from "./YellowButton.vue";
import { isFloat } from "@/store/timeEntries";
import Input from "./Input.vue";
import moment, { Moment } from "moment";
import { adAuthenticatedFetch } from "@/services/auth";
import config from "@/config";
import DatePicker from "./DatePicker.vue";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";
export default Vue.extend({
  components: {
    YellowButton,
    Input,
    DatePicker,
    CenterColumnWrapper,
  },
  data() {
    return {
      flexihours: [],
      hours: null,
      overtimeEquivalents: 0,
      overtimeYTD: 0,
      payoutsYTD: [],
      monthStart: moment()
        .startOf("month")
        .format("YYYY-MM-DD"),
      toDate: moment().format("YYYY-MM-DD"),
      yearStart: moment()
        .startOf("year")
        .format("YYYY-MM-DD"),
    };
  },
  created() {
    this.getOvertimeYTD(this.yearStart, this.toDate);
    this.getPayoutsYTD(this.yearStart, this.toDate);
  },
  computed: {
    formattedFlexihours(): { date: string; value: number }[] {
      return this.flexihours.map(({ date, value }) => ({
        date: this.formatDate(moment(date)),
        value,
      }));
    },
    totalFlexHours: function(): number {
      return this.flexihours.reduce(function(
        totalFlexHours: number,
        item: { value: number }
      ) {
        return totalFlexHours + item.value;
      },
      0);
    },
    totalPayout: function(): number {
      return this.payoutsYTD.reduce(function(
        totalHoursPaid: number,
        item: { value: number }
      ) {
        return totalHoursPaid + item.value;
      },
      0);
    },
    today(): string {
      return this.formatDate(moment());
    },
    isNumber(): boolean {
      return this.isFloat && !!this.hours;
    },
    showHours(): number | null {
      return this.hours ? this.hours : 99;
    },
    buttonText(): string {
      // @ts-ignore
      return this.$mq === "sm" ? "" : "bestill";
    },
    isFloat(): boolean {
      const hours = this.hours ? this.hours : "";
      return isFloat(hours as string);
    },
  },
  methods: {
    onButtonClick() {
      console.log("button clicked");
    },
    async getFlexihours() {
      await this.fetchFlexiHours(this.monthStart, this.toDate);
      await this.fetchOvertimeEquivalents(this.monthStart, this.toDate);
      console.log("button clicked");
      console.log(JSON.stringify(this.flexihours));
    },
    formatDate(d: Moment): string {
      const s = d.format("dddd D. MMMM");
      return s.charAt(0).toUpperCase() + s.slice(1);
    },
    async fetchFlexiHours(fromDateInclusive: string, toDateInclusive: string) {
      try {
        const url = new URL(config.API_HOST + "/api/user/FlexiHours");
        url.search = new URLSearchParams({
          fromDateInclusive,
          toDateInclusive,
        }).toString();
        const res = await adAuthenticatedFetch(url.toString());
        if (res.status !== 200) {
          throw res.statusText;
        }
        this.flexihours = await res.json();
      } catch (e) {
        console.error(e);
        this.$store.commit("ADD_TO_ERROR_LIST", e);
      }
    },
    async fetchOvertimeEquivalents(
      fromDateInclusive: string,
      toDateInclusive: string
    ) {
      try {
        const url = new URL(config.API_HOST + "/api/user/OvertimeEquivalents");
        url.search = new URLSearchParams({
          fromDateInclusive,
          toDateInclusive,
        }).toString();
        const res = await adAuthenticatedFetch(url.toString());
        if (res.status !== 200) {
          throw res.statusText;
        }
        this.overtimeEquivalents = await res.json();
      } catch (e) {
        console.error(e);
        this.$store.commit("ADD_TO_ERROR_LIST", e);
      }
    },
    async getOvertimeYTD(fromDateInclusive: string, toDateInclusive: string) {
      try {
        const url = new URL(config.API_HOST + "/api/user/OvertimeEquivalents");
        url.search = new URLSearchParams({
          fromDateInclusive,
          toDateInclusive,
        }).toString();
        const res = await adAuthenticatedFetch(url.toString());
        if (res.status !== 200) {
          throw res.statusText;
        }
        this.overtimeYTD = await res.json();
      } catch (e) {
        console.error(e);
        this.$store.commit("ADD_TO_ERROR_LIST", e);
      }
    },
    async getPayoutsYTD(fromDateInclusive: string, toDateInclusive: string) {
      try {
        const url = new URL(config.API_HOST + "/api/user/OvertimePayouts");
        url.search = new URLSearchParams({
          fromDateInclusive,
          toDateInclusive,
        }).toString();
        const res = await adAuthenticatedFetch(url.toString());
        if (res.status !== 200) {
          throw res.statusText;
        }
        this.payoutsYTD = await res.json();
      } catch (e) {
        console.error(e);
        this.$store.commit("ADD_TO_ERROR_LIST", e);
      }
    },
    async orderHours() {
      try {
        const method = "post";
        const headers = { "Content-Type": "application/json" };
        const body = JSON.stringify({
          date: this.toDate,
          value: parseFloat(this.hours.replace(/,/g, ".")),
        });
        const options = { method, headers, body };

        const response = await adAuthenticatedFetch(
          config.API_HOST + "/api/user/OvertimePayout",
          options
        );

        if (response.status !== 200) {
          throw response.statusText;
        }

        const payoutResponse = await response.json();
        console.log(payoutResponse);
      } catch (e) {
        console.error(e);
        this.$store.commit("ADD_TO_ERROR_LIST", e);
      }
    },
  },
});
</script>
​
<style scoped>
.padding {
  padding: 1rem;
}
.overtime-value {
  font-weight: 600;
  white-space: nowrap;
}
.overtime-date {
  font-weight: 600;
  text-transform: capitalize;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.overtime {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
}
.line {
  border: 0.5px solid #008dcf;
  margin: 0.3rem 0.3rem;
}
​.input-container {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
}
​.description {
  margin: 0.5rem 0;
}
​.expand-enter-active {
  animation: enter 0.1s;
  animation-timing-function: linear;
}
​ .expand-leave-active {
  animation: enter 0.1s reverse;
  animation-timing-function: linear;
}
.row {
  display: grid;
  grid-template-columns: 1fr 60px;
  align-items: center;
  color: #000;
  padding: 0 1rem;
  grid-gap: 0.5rem;
}
.date-pickers {
  display: flex;
  justify-content: space-between;
}
​ @keyframes enter {
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
</style>
