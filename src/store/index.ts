import Vue from "vue";
import Vuex from "vuex";

Vue.use(Vuex);

export default new Vuex.Store({
  strict: true,
  state: {
    tasks: [
      {
        id: 1,
        name: "Alvtime frontend",
        description: "Code up some Alvtime frontend",
        projectId: 1,
        projectName: "Alvtime",
        customerId: 1,
        customerName: "Alv",
        hourRate: 0,
        favorite: true,
        locked: false,
      },
      {
        id: 2,
        name: "Bordtennis",
        description: "Vinne",
        projectId: 2,
        projectName: "Sosial trening",
        customerId: 1,
        customerName: "Alv",
        hourRate: 0,
        favorite: true,
        locked: false,
      },
      {
        id: 3,
        name: "Ferie",
        description: "Null niks nada",
        projectId: 3,
        projectName: "Fritid",
        customerId: 1,
        customerName: "Alv",
        hourRate: 0,
        favorite: true,
        locked: false,
      },
      {
        id: 4,
        name: "Manuelle endringer i database",
        description: "Fullstending It revolusjon",
        projectId: 4,
        projectName: "It revolusjon",
        customerId: 2,
        customerName: "Kunde AS",
        hourRate: 0,
        favorite: false,
        locked: false,
      },
    ],
    timeEntries: [
      {
        id: 1,
        date: "2020-01-15",
        value: 7.5,
        taskId: 4,
      },
      {
        id: 2,
        date: "2020-01-16",
        value: 7.5,
        taskId: 4,
      },
      {
        id: 3,
        date: "2020-01-17",
        value: 7.5,
        taskId: 4,
      },
      {
        id: 4,
        date: "2020-01-18",
        value: 7.5,
        taskId: 4,
      },
    ],
  },
  getters: {
    favoriteTasks: state => {
      return state.tasks.filter(task => task.favorite);
    },
    notFavoriteTasks: state => {
      return state.tasks.filter(task => !task.favorite);
    },
  },
  mutations: {
    updateTimeEntrie(store, { timeEntrie }) {
      const index = store.timeEntries.findIndex(
        entrie => entrie.id === timeEntrie.id && entrie.date === timeEntrie.date
      );

      if (index !== -1) {
        store.timeEntries[index] = timeEntrie;
      }
    },
  },
  actions: {},
  modules: {},
});
