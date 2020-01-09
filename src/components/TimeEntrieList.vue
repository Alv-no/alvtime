<template>
  <div>
    <div v-for="(timeEntrie, index) in timeEntries" :key="index">
      <time-entrie :timeEntrie="timeEntrie"></time-entrie>
    </div>
  </div>
</template>

<script>
import TimeEntrie from "./TimeEntrie";

export default {
  components: {
    "time-entrie": TimeEntrie
  },
  props: ["number"],

  data() {
    return {
      timeEntries: [
        {
          activity: 1,
          customer: "Alv",
          activityName: "AlvTime",
          activityDesc: "Do Stuff",
          registeredHours: 7.5
        },
        {
          activity: 2,
          customer: "UDI",
          activityName: "Visualisering er veldig bra for alle sin produktivitet",
          activityDesc: "Do Crap",
          registeredHours: 0
        },
        {
          activity: 3,
          customer: "Alv",
          activityName: "Ferie",
          activityDesc: "Do Awesame",
          registeredHours: 0
        },
        {
          activity: 4,
          customer: "Alv",
          activityName: "Bordtennis",
          activityDesc: "Bli best",
          registeredHours: 1
        },
        {
          activity: 5,
          customer: "Alv",
          activityName: "AlvTime",
          activityDesc: "Do Stuff",
          registeredHours: 7.5
        },
        {
          activity: 6,
          customer: "UD",
          activityName: "Visualisering",
          activityDesc: "Do Crap",
          registeredHours: 0
        },
        {
          activity: 7,
          customer: "Alv",
          activityName: "Ferie",
          activityDesc: "Do Awesame",
          registeredHours: 0
        },
        {
          activity: 8,
          customer: "Alv",
          activityName: "Bordtennis",
          activityDesc: "Bli best",
          registeredHours: 1
        },
        {
          activity: 9,
          customer: "Alv",
          activityName: "AlvTime",
          activityDesc: "Do Stuff",
          registeredHours: 7.5
        },
        {
          activity: 10,
          customer: "UD",
          activityName: "Visualisering",
          activityDesc: "Do Crap",
          registeredHours: 0
        },
        {
          activity: 11,
          customer: "Alv",
          activityName: "Ferie",
          activityDesc: "Do Awesame",
          registeredHours: 0
        },
        {
          activity: 12,
          customer: "Alv",
          activityName: "Bordtennis",
          activityDesc: "Bli best",
          registeredHours: 1
        },
        {
          activity: 13,
          customer: "Alv",
          activityName: "Lommetennis",
          activityDesc: "Bli best",
          registeredHours: 1
        }
      ]
    };
  }
};
</script>
