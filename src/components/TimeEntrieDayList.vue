<template>
  <div>
    <p>{{ day }}</p>
    <TimeEntrieDay
      v-for="task in daysTasks"
      :key="task.timeEntrie.value + task.name"
      :task="task"
    />
  </div>
</template>

<script>
import TimeEntrieDay from "./TimeEntrieDay";
import moment from "moment";

export default {
  components: {
    TimeEntrieDay,
  },
  props: ["date"],

  computed: {
    day() {
      const d = this.date.format("dddd");
      return d.charAt(0).toUpperCase() + d.slice(1);
    },

    daysTasks() {
      const timeEntries = this.$store.state.timeEntries.filter(
        entrie => entrie.date === this.date.format("YYYY-MM-DD")
      );

      const tasksWithHours = timeEntries.map(entrie => {
        const { customerName, name } = this.$store.state.tasks.find(
          task => task.id === entrie.taskId
        );

        return {
          customerName,
          name,
          timeEntrie: entrie,
        };
      });

      const tasksWithoutHours = this.$store.getters.favoriteTasks
        .filter(task => !timeEntries.some(entrie => entrie.taskId === task.id))
        .map(({ customerName, name, id }) => ({
          customerName,
          name,
          timeEntrie: {
            id: 0,
            date: "",
            value: 0,
            taskId: id,
          },
        }));

      return [...tasksWithHours, ...tasksWithoutHours];
    },
  },
};
</script>
