<template>
  <CenterColumnWrapper>
    <div class="padding">
      <div class="vacation-used">
        <div><b>Ferietimer tatt ut så langt i år:</b> {{ hoursUsed }}</div>
        <div><b>Feriedager tatt ut så langt i år:</b> {{ daysUsed }}</div>
      </div>
    </div>
  </CenterColumnWrapper>
</template>

<script lang="ts">
import Vue from "vue";
import moment, { Moment } from "moment";
import config from "@/config";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";
import { adAuthenticatedFetch } from "@/services/auth";

interface VacationTimeModel {
  totalHours: number;
  entries: EntriesModel[];
}

interface EntriesModel {
  user: number;
  userEmail: string;
  id: number;
  date: Date;
  value: number;
  taskId: number;
}

export default Vue.extend({
  components: {
    CenterColumnWrapper,
  },
  data() {
    return {
      entries: {},
    };
  },
  computed: {
    hoursUsed(): number {
      return this.$store.getters.getUsedVacation;
    },
    daysUsed(): number {
      return this.hoursUsed / 7.5;
    },
  },

  async created() {
    await this.$store.dispatch("FETCH_USED_VACATION", { year: 2021 });
  },

  methods: {
    fetchUsedVacation: async (year: number) => {
      try {
        let url = new URL(
          config.API_HOST + `/api/user/UsedVacationHours?year=${year}`
        ).toString();
        const response = await adAuthenticatedFetch(url);
        const data = await response.json();
        // if (response.status !== 200) throw Error(`${response.statusText}`);
        return formatResponse(data);
      } catch (e) {
        console.error(e);
      }
    },
  },
});

function formatResponse(entries: EntriesModel[]): VacationTimeModel {
  return {
    totalHours: entries.reduce(
      (sum: number, { value }: { value: number }) => sum + value,
      0
    ),
    entries: entries,
  };
}
</script>

<style scoped>
.padding {
  padding: 1rem;
}
.vacation-used b {
  font-size: 0.7rem;
  font-weight: 600;
}
.vacation-used {
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 10px;
}
</style>
